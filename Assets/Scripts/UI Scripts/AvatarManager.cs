using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public GameObject[] avatars; // Array to hold all avatar GameObjects

    void Start()
    {
        LoadSelectedAvatar();
    }

    void LoadSelectedAvatar()
    {
        // Check if a selected avatar is saved in PlayerPrefs
        if (PlayerPrefs.HasKey("SelectedAvatar"))
        {
            string savedAvatarId = PlayerPrefs.GetString("SelectedAvatar");

            // Find the avatar with the saved name and highlight it
            foreach (GameObject avatar in avatars)
            {
                if (avatar.name == savedAvatarId)
                {
                    AvatarSelector avatarSelector = avatar.GetComponent<AvatarSelector>();
                    avatarSelector.OnAvatarSelected(); // Trigger the selection border to show
                    break;
                }
            }
        }
    }
}
