using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] private GameObject[] characters;
    private int selectedCharacterIndex = 0;

    public void NextCharacter()
    {
        characters[selectedCharacterIndex].SetActive(false);
        selectedCharacterIndex = (selectedCharacterIndex + 1) % characters.Length;
        characters[selectedCharacterIndex].SetActive(true);
    }

    public void PreviousCharacter()
    {
        characters[selectedCharacterIndex].SetActive(false);
        selectedCharacterIndex = (selectedCharacterIndex - 1 + characters.Length) % characters.Length;
        characters[selectedCharacterIndex].SetActive(true);
    }

    public void StartGame()
    {
        // Save the selected character's index to PlayerPrefs
        PlayerPrefs.SetInt("SelectedCharacterIndex", selectedCharacterIndex);
        PlayerPrefs.Save();

        // Load the next scene
        SceneManager.LoadScene("AR Board Scene");
    }

    private void Start()
    {
        if (characters.Length > 0)
        {
            characters[selectedCharacterIndex].SetActive(true); // Initialize character visibility
        }
    }
}
