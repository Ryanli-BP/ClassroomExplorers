using UnityEngine;
using UnityEngine.UI;

public class ARManager : MonoBehaviour
{
    public Image avatarImage; // Reference to the UI Image component to display the avatar

    void Start()
    {
        LoadSelectedAvatar();
    }

    void LoadSelectedAvatar()
    {
        if (PlayerPrefs.HasKey("SelectedAvatar"))
        {
            string savedAvatarId = PlayerPrefs.GetString("SelectedAvatar");
            Debug.Log("Loading avatar: " + savedAvatarId);

            // Find all avatars using the "Avatar" tag
            GameObject avatarPrefab = Resources.Load<GameObject>("Prefabs/UI prefab/" + savedAvatarId);
            if (avatarPrefab != null)
            {
                Transform playerImageTransform = avatarPrefab.transform.Find("player image");
                if (playerImageTransform == null)
                {
                    Debug.LogError("PlayerImage object not found within the avatar.");
                    return;
                }

                Image playerImageComponent = playerImageTransform.GetComponent<Image>();
                if (playerImageComponent == null)
                {
                    Debug.LogError("No Image component found on PlayerImage object.");
                    return;
                }

                // Assign the sprite to the avatarImage
                avatarImage.sprite = playerImageComponent.sprite;
                Debug.Log("Sprite assignment successful.");
            }
            else
            {
                Debug.LogError("Avatar prefab not found: " + savedAvatarId);
            }


            
        }
    }
}
