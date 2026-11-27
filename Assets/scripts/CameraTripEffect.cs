using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraTripEffect : MonoBehaviour
{
    [SerializeField] private Volume volumeTrip;
    [SerializeField] private Transform camera;
    [SerializeField] private float dureeMontee = 4f;
    [SerializeField] private float intensiteDistortion = 0.8f;
    [SerializeField] private float intensiteMotionBlur = 1f;
    [SerializeField] private float intensiteWiggle = 0.04f;
    [SerializeField] private float vitesseWiggle = 3f;
    [SerializeField] private float vitessePulsation = 0.5f;

    private LensDistortion lensDistortion;
    private MotionBlur motionBlur;
    private Vector3 positionOrigine;
    private bool actif = false;
    private float t = 0f;

    private void Awake()
    {
        if (volumeTrip != null && volumeTrip.profile != null)
        {
            volumeTrip.profile.TryGet(out lensDistortion);
            volumeTrip.profile.TryGet(out motionBlur);
        }

        if (camera != null)
        {
            positionOrigine = camera.localPosition;
        }
    }

    public void SetActif(bool value)
    {
        actif = value;
        if (!actif)
        {
            if (lensDistortion != null) lensDistortion.intensity.value = 0f;
            if (motionBlur != null) motionBlur.intensity.value = 0f;
            if (camera != null) camera.localPosition = positionOrigine;
            t = 0f;
        }
    }

    private void Update()
    {
        if (!actif) return;

        t += Time.deltaTime;
        float montee = Mathf.Clamp01(t / dureeMontee);

        float pulsation = (Mathf.Sin(Time.time * vitessePulsation) + 1f) * 0.5f;

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = montee * pulsation * intensiteDistortion;
        }

        if (motionBlur != null)
        {
            motionBlur.intensity.value = montee * pulsation * intensiteMotionBlur;
        }

        if (camera != null)
        {
            float decalageX = (Mathf.PerlinNoise(Time.time * vitesseWiggle, 0f) - 0.5f) * intensiteWiggle;
            float decalageY = (Mathf.PerlinNoise(0f, Time.time * vitesseWiggle) - 0.5f) * intensiteWiggle;
            camera.localPosition = positionOrigine + new Vector3(decalageX, decalageY, 0f) * montee;
        }
    }
}