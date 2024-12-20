using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip[] clipsofAudio;
    // put the main audio source
    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
}
    public void PlayAudioWrong()
    {
        foreach (AudioClip clip in clipsofAudio)
        {
            if (clip.name == "SFX_Wrong")
            {
                PlayAudio(clip);
            }
        }

    }

    void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
