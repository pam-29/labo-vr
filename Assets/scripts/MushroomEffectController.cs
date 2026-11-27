using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MushroomEffectController : MonoBehaviour
{
    [SerializeField] private Volume volumeEffetChampignon;
    [SerializeField] private AudioSource sonAmbiance;
    [SerializeField] private WobbleDeform[] objetsADeformer;
    [SerializeField] private ColorCycle[] objetsCouleurChangeante;
    [SerializeField] private CameraTripEffect cameraTripEffect;
    [SerializeField] private float dureeTransition = 3f;

    private bool dejaDeclenche = false;

    private void Awake()
    {
        if (volumeEffetChampignon != null)
        {
            volumeEffetChampignon.weight = 0f;
        }
        if (sonAmbiance != null)
        {
            sonAmbiance.volume = 0f;
        }
    }

    private void Start()
    {
        DeclencherEffet();
    }

    public void DeclencherEffet()
    {
        if (dejaDeclenche) return;
        dejaDeclenche = true;

        StartCoroutine(TransitionEffet());

        foreach (WobbleDeform obj in objetsADeformer)
        {
            obj.SetActif(true);
        }

        foreach (ColorCycle obj in objetsCouleurChangeante)
        {
            obj.SetActif(true);
        }

        if (sonAmbiance != null)
        {
            sonAmbiance.Play();
        }

        if (cameraTripEffect != null)
        {
            cameraTripEffect.SetActif(true);
        }
    }

    private IEnumerator TransitionEffet()
    {
        float t = 0f;
        while (t < dureeTransition)
        {
            t += Time.deltaTime;
            if (volumeEffetChampignon != null)
            {
                volumeEffetChampignon.weight = Mathf.Clamp01(t / dureeTransition);
            }
            if (sonAmbiance != null)
            {
                sonAmbiance.volume = Mathf.Clamp01(t / dureeTransition);
            }
            yield return null;
        }
    }
}