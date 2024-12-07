using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSelectionManager : MonoBehaviour
{
    public Image[] avatarButtons;    // Assign avatar buttons (images) in the inspector
    public Button confirmButton;     // Assign confirm button in the inspector

    private int selectedAvatarIndex = -1;

    private Color greyColor = Color.grey; // Default circle color
    private Color redColor = Color.red;   // Selected circle color

    void Start()
    {
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i;  // Capture the current index for the delegate
            avatarButtons[i].GetComponent<Button>().onClick.AddListener(() => SelectAvatar(index));
        }

        confirmButton.onClick.AddListener(ConfirmSelection);
    }

    public void SelectAvatar(int index)
    {
        if (selectedAvatarIndex == index)
        {
            // Deselect if the same avatar is clicked again
            avatarButtons[index].color = greyColor;
            selectedAvatarIndex = -1;
            Debug.Log("Deselected Avatar: " + index);
        }
        else
        {
            // Deselect all avatars
            for (int i = 0; i < avatarButtons.Length; i++)
            {
                avatarButtons[i].color = greyColor;
            }

            // Select the new avatar
            avatarButtons[index].color = redColor;
            selectedAvatarIndex = index;
            Debug.Log("Selected Avatar: " + index);
        }
    }

    void ConfirmSelection()
    {
        if (selectedAvatarIndex != -1)
        {
            Debug.Log("Confirmed Avatar: " + selectedAvatarIndex);
            SceneManager.LoadScene("NextSceneName"); // Replace with actual scene name
        }
        else
        {
            Debug.LogWarning("No avatar selected. Please select an avatar.");
        }
    }
}
