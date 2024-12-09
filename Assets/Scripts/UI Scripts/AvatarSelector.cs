using UnityEngine;

public class AvatarSelector : MonoBehaviour
{
    public GameObject selectionBorder;
    private static AvatarSelector currentlySelected;

    void Start()
    {
        selectionBorder.SetActive(false); // Hide selection border initially
    }

    public void OnAvatarSelected()
    {
        // Log the name of the selected avatar
        Debug.Log("Avatar selected: " + gameObject.name);  // This will log the name of the avatar GameObject, which will be used as the avatarId

        // Deselect the currently selected avatar (if any)
        if (currentlySelected != null)
        {
            currentlySelected.Deselect();
        }

        // Set this avatar as the currently selected avatar
        currentlySelected = this;
        selectionBorder.SetActive(true); // Show the selection border

        // Use the GameObject's name as its unique ID (avatarId)
        string avatarId = gameObject.name; 

        // Save the selected avatar's name (ID) to PlayerPrefs
        PlayerPrefs.SetString("SelectedAvatar", avatarId);
        PlayerPrefs.Save(); // Save the changes

        // Log the avatarId for debugging
        Debug.Log("Selected Avatar ID saved: " + avatarId); // This will log the avatar ID
    }

    public void Deselect()
    {
        selectionBorder.SetActive(false); // Hide the selection border when deselected
    }
}
