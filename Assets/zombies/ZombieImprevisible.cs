using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;

// Zombie imprévisible : il erre autour du joueur à vitesse variable, a des coups de boost,
// vient parfois vers le joueur en marchant puis change de direction,
// et de temps en temps fait une FEINTE : il charge droit sur le joueur, pile net, le fixe,
// puis repart dans une autre direction. Il contourne les troncs et ne s'approche jamais
// plus près que "Distance Securite".
// À placer sur l'objet racine de chaque zombie (celui qui contient l'Animator).
public class ZombieImprevisible : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("La caméra du casque (Main Camera du XR Origin). Vide = Camera.main.")]
    public Transform joueur;
    [Tooltip("L'objet racine de la forêt importée : sert à repérer les troncs à contourner.")]
    public Transform foret;
    [Tooltip("Calques touchés par le rayon qui cherche le sol (mettre un Mesh Collider sur Sol_Terrain).")]
    public LayerMask masqueSol = ~0;

    [Header("Zone d'errance autour du joueur (mètres)")]
    public float distanceMin = 5f;
    public float distanceMax = 12f;
    [Tooltip("Il ne s'approche jamais plus près que ça, même pendant une feinte.")]
    public float distanceSecurite = 2.5f;

    [Header("Marche (m/s)")]
    public Vector2 vitesseMarche = new Vector2(0.35f, 0.9f);
    [Tooltip("Il change de direction toutes les X secondes (min, max).")]
    public Vector2 dureeTrajet = new Vector2(4f, 10f);

    [Header("Coups de boost")]
    public Vector2 vitesseBoost = new Vector2(2.2f, 3.5f);
    public Vector2 dureeBoost = new Vector2(0.8f, 2.2f);
    [Tooltip("Chance de boost par seconde de marche (0,05 = en moyenne un boost toutes les 20 s).")]
    public float chanceBoost = 0.05f;

    [Header("Feintes")]
    public float vitesseFeinte = 4.5f;
    [Tooltip("Distance au joueur où il pile net (min, max).")]
    public Vector2 distanceArret = new Vector2(3f, 4.5f);
    [Tooltip("Temps d'immobilité après avoir pilé (min, max), en fixant le joueur.")]
    public Vector2 dureeArret = new Vector2(1f, 2.5f);
    [Tooltip("Temps entre deux feintes (min, max). Mettre de grandes valeurs pour les rendre rares.")]
    public Vector2 delaiEntreFeintes = new Vector2(25f, 50f);

    [Header("Approches (il marche vers le joueur, puis change de direction)")]
    [Tooltip("Chance par seconde de marche qu'il se mette à venir vers le joueur (0,06 = environ toutes les 15 s).")]
    public float chanceApproche = 0.06f;
    public Vector2 vitesseApproche = new Vector2(0.5f, 1.1f);
    [Tooltip("Distance au joueur où il renonce et part ailleurs (min, max).")]
    public Vector2 distanceFinApproche = new Vector2(3f, 5f);
    [Tooltip("Durée max d'une approche (min, max), en secondes.")]
    public Vector2 dureeApproche = new Vector2(5f, 12f);
    [Tooltip("Chance qu'il s'arrête et fixe le joueur avant de repartir (0 à 1).")]
    public float chanceRegarder = 0.35f;

    [Header("Animation (facultatif)")]
    [Tooltip("Seulement si le zombie n'a pas d'Animator Controller : glisser ici le clip de marche (dans le .glb/.fbx).")]
    public AnimationClip clipMarche;

    [Header("Réglages")]
    [Tooltip("Mettre 180 si le zombie avance à reculons.")]
    public float correctionRotation = 0f;
    [Tooltip("Vitesse (m/s) pour laquelle l'animation de marche ne glisse pas.")]
    public float vitesseAnimationReference = 0.5f;
    public float acceleration = 6f;

    enum Etat { Errance, Boost, Feinte, Arret, Approche }
    Etat etat;
    Vector3 centre;
    Vector3 cible;
    float vitesse, vitesseVoulue, minuteur, prochaineFeinte, arretA, finApproche;
    Animator anim;
    Animation animLegacy;
    AnimationState etatLegacy;
    PlayableGraph graphe;
    AnimationClipPlayable lecture;
    readonly List<Vector3> troncs = new List<Vector3>();
    readonly List<float> rayons = new List<float>();

    void Start()
    {
        if (joueur == null && Camera.main != null) joueur = Camera.main.transform;
        anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.applyRootMotion = false;
        DemarrerAnimation();
        if (foret == null)
        {
            GameObject f = GameObject.Find("foret_sombre_vr");
            if (f == null) { GameObject sol = GameObject.Find("Sol_Terrain"); if (sol != null) f = sol.transform.root.gameObject; }
            if (f != null) foret = f.transform;
        }
        if (foret != null)
        {
            foreach (Transform t in foret.GetComponentsInChildren<Transform>())
            {
                if (!t.name.StartsWith("Arbre_")) continue;
                float r = t.name.Contains("chene") ? 1.3f : (t.name.Contains("jeune") ? 0.5f : 0.9f);
                troncs.Add(t.position);
                rayons.Add(r);
            }
        }
        prochaineFeinte = Time.time + Aleatoire(delaiEntreFeintes);
        Errer(false);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (joueur != null) centre = new Vector3(joueur.position.x, 0f, joueur.position.z);
        Vector3 pos = transform.position;
        float distJoueur = Plat(pos - centre).magnitude;

        switch (etat)
        {
            case Etat.Errance:
                minuteur -= dt;
                if (Plat(cible - pos).magnitude < 0.8f || minuteur <= 0f) Errer(false);
                if (Time.time > prochaineFeinte && distJoueur > distanceArret.y + 2f)
                {
                    etat = Etat.Feinte;
                    arretA = Aleatoire(distanceArret);
                    vitesseVoulue = vitesseFeinte;
                }
                else if (Random.value < chanceApproche * dt && distJoueur > distanceFinApproche.y + 1f)
                {
                    etat = Etat.Approche;                  // il vient vers le joueur, en marchant
                    finApproche = Aleatoire(distanceFinApproche);
                    minuteur = Aleatoire(dureeApproche);
                    vitesseVoulue = Aleatoire(vitesseApproche);
                }
                else if (Random.value < chanceBoost * dt)
                {
                    etat = Etat.Boost;
                    minuteur = Aleatoire(dureeBoost);
                    vitesseVoulue = Aleatoire(vitesseBoost);
                }
                break;

            case Etat.Boost:
                minuteur -= dt;
                if (Plat(cible - pos).magnitude < 0.8f) cible = PointAleatoire(false);
                if (minuteur <= 0f) Errer(false);
                break;

            case Etat.Feinte:
                cible = centre;
                if (distJoueur <= arretA)
                {
                    etat = Etat.Arret;              // il pile net
                    vitesse = 0f;
                    vitesseVoulue = 0f;
                    minuteur = Aleatoire(dureeArret);
                    prochaineFeinte = Time.time + Aleatoire(delaiEntreFeintes);
                }
                break;

            case Etat.Approche:
                cible = centre;
                minuteur -= dt;
                if (distJoueur <= finApproche || minuteur <= 0f)
                {
                    if (Random.value < chanceRegarder)
                    {
                        etat = Etat.Arret;                 // il s'arrête et le fixe un instant...
                        vitesseVoulue = 0f;
                        minuteur = Aleatoire(dureeArret);
                    }
                    else Errer(true);                      // ...ou bifurque directement
                }
                break;

            case Etat.Arret:
                minuteur -= dt;
                if (minuteur <= 0f)
                {
                    prochaineFeinte = Mathf.Max(prochaineFeinte, Time.time + Aleatoire(delaiEntreFeintes) * 0.5f);
                    Errer(true);                     // repart dans une autre direction
                }
                break;
        }

        // déplacement
        vitesse = Mathf.MoveTowards(vitesse, vitesseVoulue, acceleration * dt);
        Vector3 dir = Plat(cible - pos);
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;
        dir = Eviter(pos, dir);
        Vector3 nouv = pos + dir * vitesse * dt;
        Vector3 rel = Plat(nouv - centre);
        if (rel.magnitude < distanceSecurite)
        {
            Vector3 bord = centre + rel.normalized * distanceSecurite;
            nouv = new Vector3(bord.x, nouv.y, bord.z);
        }
        RaycastHit hit;
        if (Physics.Raycast(nouv + Vector3.up * 5f, Vector3.down, out hit, 30f, masqueSol, QueryTriggerInteraction.Ignore))
            nouv.y = hit.point.y;
        transform.position = nouv;

        // orientation : dans le sens de la marche, ou vers le joueur quand il pile
        Vector3 regard = etat == Etat.Arret ? Plat(centre - nouv) : dir;
        if (regard.sqrMagnitude > 0.0001f)
        {
            Quaternion r = Quaternion.LookRotation(regard) * Quaternion.Euler(0f, correctionRotation, 0f);
            float virage = etat == Etat.Feinte || etat == Etat.Arret ? 10f : 3f;
            transform.rotation = Quaternion.Slerp(transform.rotation, r, virage * dt);
        }

        // l'animation suit la vitesse (figée quand il pile : effet glaçant)
        float vAnim = Mathf.Clamp(vitesse / vitesseAnimationReference, 0f, 4f);
        if (lecture.IsValid())
        {
            lecture.SetSpeed(vAnim);
            double duree = lecture.GetAnimationClip().length;
            if (duree > 0.01 && lecture.GetTime() > duree) lecture.SetTime(lecture.GetTime() % duree);
        }
        else if (etatLegacy != null) etatLegacy.speed = vAnim;
        else if (anim != null) anim.speed = vAnim;
    }

    // Trois cas gérés : Animator avec Controller ; Animator sans Controller + clipMarche ; composant Animation (legacy).
    void DemarrerAnimation()
    {
        if (anim != null && anim.runtimeAnimatorController == null && clipMarche != null)
        {
            graphe = PlayableGraph.Create("Marche_" + name);
            AnimationPlayableOutput sortie = AnimationPlayableOutput.Create(graphe, "Marche", anim);
            lecture = AnimationClipPlayable.Create(graphe, clipMarche);
            sortie.SetSourcePlayable(lecture);
            graphe.Play();
            return;
        }
        animLegacy = GetComponentInChildren<Animation>();
        if (animLegacy != null)
        {
            foreach (AnimationState st in animLegacy)
            {
                if (etatLegacy == null || st.name.Contains("Marche")) etatLegacy = st;
            }
            if (etatLegacy != null)
            {
                etatLegacy.wrapMode = WrapMode.Loop;
                animLegacy.Play(etatLegacy.name);
            }
        }
        else if (anim != null && anim.runtimeAnimatorController == null)
            Debug.LogWarning(name + " : pas d'animation. Glisser le clip de marche dans le champ Clip Marche.");
    }

    void OnDestroy()
    {
        if (graphe.IsValid()) graphe.Destroy();
    }

    void Errer(bool autreSens)
    {
        etat = Etat.Errance;
        cible = PointAleatoire(autreSens);
        vitesseVoulue = Aleatoire(vitesseMarche);
        minuteur = Aleatoire(dureeTrajet);
    }

    Vector3 PointAleatoire(bool autreSens)
    {
        // il tourne autour du joueur (sans couper au milieu) : prochain point à 25-80° de sa position,
        // ou 60-100° après une feinte, pour repartir franchement ailleurs
        Vector3 rel = Plat(transform.position - centre);
        float actuel = rel.sqrMagnitude > 0.01f ? Mathf.Atan2(rel.x, rel.z) * Mathf.Rad2Deg : Random.Range(0f, 360f);
        float ecart = autreSens ? Random.Range(60f, 100f) : Random.Range(25f, 80f);
        float angle = actuel + (Random.value < 0.5f ? -1f : 1f) * ecart;
        float d = Random.Range(distanceMin, distanceMax);
        float a = angle * Mathf.Deg2Rad;
        return centre + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * d;
    }

    Vector3 Eviter(Vector3 pos, Vector3 dir)
    {
        Vector3 pousse = Vector3.zero;
        for (int i = 0; i < troncs.Count; i++)
        {
            Vector3 d = Plat(pos - troncs[i]);
            float dist = d.magnitude;
            float zone = rayons[i] + 1.2f;
            if (dist < zone && dist > 0.001f) pousse += d / dist * ((zone - dist) / zone);
        }
        Vector3 r = dir + pousse * 2f;
        r.y = 0f;
        return r.sqrMagnitude > 0.0001f ? r.normalized : dir;
    }

    static Vector3 Plat(Vector3 v) { return new Vector3(v.x, 0f, v.z); }
    static float Aleatoire(Vector2 plage) { return Random.Range(plage.x, plage.y); }

    void OnDrawGizmosSelected()
    {
        Vector3 c = joueur != null ? new Vector3(joueur.position.x, transform.position.y, joueur.position.z) : transform.position;
        Gizmos.color = Color.yellow; DessinerCercle(c, distanceMin); DessinerCercle(c, distanceMax);
        Gizmos.color = Color.red; DessinerCercle(c, distanceSecurite);
    }

    static void DessinerCercle(Vector3 c, float r)
    {
        Vector3 prec = c + new Vector3(0f, 0f, r);
        for (int i = 1; i <= 48; i++)
        {
            float a = i * Mathf.PI * 2f / 48f;
            Vector3 p = c + new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
            Gizmos.DrawLine(prec, p);
            prec = p;
        }
    }
}
