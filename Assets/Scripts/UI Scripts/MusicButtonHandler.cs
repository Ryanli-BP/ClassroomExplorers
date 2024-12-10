using UnityEngine;
using UnityEngine.UI;

public class MusicButtonHandler : MonoBehaviour
{
    public Button musicButton; // Assign your button in the Inspector

    void Start()
    {
        // Make sure the MusicManager persists across scenes
        MusicManager musicManager = MusicManager.instance;

        // If MusicManager exists, add the listener
        if (musicManager != null)
        {
            musicButton.onClick.AddListener(musicManager.ToggleMusic);
        }
        else
        {
            Debug.LogError("MusicManager instance not found!");
        }
    }
}
