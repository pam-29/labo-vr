using UnityEngine;
using UnityEngine.Audio;

public class CS_PlayOnInput : MonoBehaviour
{
    public AudioClip clipToPlay;
    public AudioSource audioSourceToPlay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("i"))
        {
            audioSourceToPlay.Play();
        }
    }
}
