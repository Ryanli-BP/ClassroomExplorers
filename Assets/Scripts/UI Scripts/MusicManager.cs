using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Check if an instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // Destroy this duplicate object
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Make this object persist across scenes
        }

        // Automatically get the AudioSource component if it's not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Optionally start music if it's not playing
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }


    // This method will toggle the music (play/pause)
    public void ToggleMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();  // Pause the music
        }
        else
        {
            audioSource.Play();  // Start the music if it's not already playing
        }
    }
}
