using System.Collections;
using UnityEngine;

public class SoundHorror : MonoBehaviour
{
    [Header("Sources Audio")]
    public AudioSource sourceHorreur; 
    public AudioSource sourceCri;    

    [Header("Réglages son horreur (toutes les 5s environ)")]
    public float delaiHorreurMin = 4f; 
    public float delaiHorreurMax = 6f;

    [Header("Réglages cri au loin (de temps en temps)")]
    public float delaiCriMin = 8f;    
    public float delaiCriMax = 15f;    

    void Start()
    {
        AudioSource src = GetComponent<AudioSource>();
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        if (src != null && !src.isPlaying) src.Play();
        Debug.Log("[DIAG_AUDIO] Clip=" + (src != null && src.clip != null ? src.clip.name : "NULL") + " | Playing=" + (src != null && src.isPlaying) + " | ListenerVol=" + AudioListener.volume);

        
        if (sourceHorreur != null)
            StartCoroutine(JouerHorreurEnBoucle());

        if (sourceCri != null)
            StartCoroutine(JouerCriEnBoucle());
    }

    private IEnumerator JouerHorreurEnBoucle()
    {
        while (true)
        {
            float attente = Random.Range(delaiHorreurMin, delaiHorreurMax);
            yield return new WaitForSeconds(attente);
            if (sourceHorreur.clip != null)
                sourceHorreur.PlayOneShot(sourceHorreur.clip);
        }
    }

    private IEnumerator JouerCriEnBoucle()
    {
        while (true)
        {
            float attente = Random.Range(delaiCriMin, delaiCriMax);
            yield return new WaitForSeconds(attente);

            if (sourceCri.clip != null)
                sourceCri.PlayOneShot(sourceCri.clip);
        }
    }
}
