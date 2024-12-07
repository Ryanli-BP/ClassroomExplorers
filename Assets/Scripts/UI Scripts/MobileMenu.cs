using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MobileMenu : MonoBehaviour
{
    public TMP_InputField userNameInputField; // Reference to the input field

    // Make sure this method is public so that it appears in the OnClick event
    public void OnSubmitUsername()
    {
        // Save the username using PlayerPrefs
        PlayerPrefs.SetString("UserName", userNameInputField.text);

        // Transition to Scene 2
        SceneManager.LoadScene("Select Avatar Scene");

        // Debug log to ensure username is saved
        Debug.Log("Username saved: " + PlayerPrefs.GetString("UserName"));
    }
}
