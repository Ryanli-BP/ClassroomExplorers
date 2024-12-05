using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AvatarMenu : MonoBehaviour
{
    public TMP_Text userNameDisplay; // Reference to TMP_Text (used to display the username)

    void Start()
    {
        // Retrieve the saved username from PlayerPrefs
        string userName = PlayerPrefs.GetString("UserName", "Guest"); // Default to "Guest" if not found

        // Display the username
        userNameDisplay.text = "Welcome, " + userName + "!";
        Debug.Log("Displayed username: " + userName);
    }
}
