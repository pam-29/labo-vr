using UnityEngine;

// Fait apparaître automatiquement des zombies autour du joueur au lancement (Play),
// avec le script ZombieImprevisible déjà réglé. Rien d'autre à configurer.
// Utilisation : créer un objet vide dans la scène (clic droit > Create Empty), lui ajouter ce script,
// glisser zombie1 et zombie2 (les modèles importés depuis Assets) dans la liste "Modeles".
public class ApparitionZombies : MonoBehaviour
{
    [Tooltip("Les modèles de zombies importés (glisser zombie1 et zombie2 depuis Assets).")]
    public GameObject[] modeles;

    [Tooltip("Clip de marche de chaque modèle, dans le même ordre (à remplir seulement si les zombies ne bougent pas les jambes).")]
    public AnimationClip[] clipsMarche;

    [Tooltip("Nombre total de zombies.")]
    public int nombre = 5;

    [Tooltip("Ils apparaissent entre ces deux distances du joueur (m).")]
    public float distanceMin = 6f;
    public float distanceMax = 11f;

    [Tooltip("La caméra du casque. Vide = Camera.main.")]
    public Transform joueur;

    [Tooltip("La forêt importée. Vide = trouvée automatiquement.")]
    public Transform foret;

    void Start()
    {
        if (modeles == null || modeles.Length == 0) { Debug.LogError("ApparitionZombies : glisser au moins un modèle de zombie dans Modeles."); return; }
        if (joueur == null && Camera.main != null) joueur = Camera.main.transform;
        Vector3 c = joueur != null ? joueur.position : Vector3.zero;
        for (int i = 0; i < nombre; i++)
        {
            int m = i % modeles.Length;
            float a = (i + Random.Range(-0.3f, 0.3f)) * Mathf.PI * 2f / nombre;
            float d = Random.Range(distanceMin, distanceMax);
            Vector3 p = new Vector3(c.x + Mathf.Sin(a) * d, c.y + 5f, c.z + Mathf.Cos(a) * d);
            RaycastHit hit;
            if (Physics.Raycast(p, Vector3.down, out hit, 30f)) p.y = hit.point.y; else p.y = 0f;
            GameObject z = Instantiate(modeles[m], p, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            z.name = "Zombie_" + (i + 1);
            float echelle = Random.Range(0.92f, 1.08f);
            z.transform.localScale = z.transform.localScale * echelle;
            ZombieImprevisible zi = z.AddComponent<ZombieImprevisible>();
            zi.joueur = joueur;
            zi.foret = foret;
            // le modèle 2 marche plus lentement : sinon ses pieds glissent
            zi.vitesseAnimationReference = modeles[m].name.ToLower().Contains("zombie2") ? 0.37f : 0.5f;
            if (clipsMarche != null && m < clipsMarche.Length) zi.clipMarche = clipsMarche[m];
        }
    }
}
