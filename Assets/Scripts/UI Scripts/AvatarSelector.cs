using UnityEngine;

public class AvatarSelector : MonoBehaviour
{
    public GameObject selectionBorder; // Reference to the border GameObject
    private static AvatarSelector currentlySelected; // Tracks the currently selected avatar

    void Start()
    {
        // Ensure the selection border is initially disabled
        selectionBorder.SetActive(false);
    }

    // Called when the avatar is clicked
    public void OnAvatarSelected()
    {
        // Deselect the previously selected avatar
        if (currentlySelected != null)
        {
            currentlySelected.Deselect();
        }

        // Select this avatar
        currentlySelected = this;
        selectionBorder.SetActive(true);

        Debug.Log("Avatar selected: " + gameObject.name); // For debugging purposes
    }

    // Deselects this avatar
    public void Deselect()
    {
        selectionBorder.SetActive(false);
    }
}
