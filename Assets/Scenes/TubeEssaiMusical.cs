using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class TubeEssaiMusical : MonoBehaviour
{
    [SerializeField] private AudioClip son;
    [SerializeField] private bool enBoucle = true;
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;

    private AudioSource audioSource;
    private XRSimpleInteractable interactable;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = son;
        audioSource.loop = enBoucle;
        audioSource.playOnAwake = false;

        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnSelect);
    }

    private void OnSelect(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            return;
        }

        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        audioSource.Play();
    }
}