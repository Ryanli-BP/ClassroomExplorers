using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private GameObject[] bodyColors;
    private int selectedBodyColorIndex = 0;

    private GameObject[] hats;
    private int selectedHatIndex = 0;

    private void Start()
    {
        InitializeBodyColors();
        InitializeHats();

        // Show the first body color and hat by default
        if (bodyColors.Length > 0)
        {
            bodyColors[selectedBodyColorIndex].SetActive(true);
        }
        if (hats.Length > 0)
        {
            hats[selectedHatIndex].SetActive(true);
        }
    }

    private void InitializeBodyColors()
    {
        Transform bodyParent = player.transform.Find("Mesh Object/Bone_Body");
        if (bodyParent != null)
        {
            int colorCount = bodyParent.childCount;
            bodyColors = new GameObject[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                bodyColors[i] = bodyParent.GetChild(i).gameObject;
                bodyColors[i].SetActive(false); // Deactivate all body colors initially
            }
        }
        else
        {
            bodyColors = new GameObject[0];
        }
    }

    private void InitializeHats()
    {
        Transform hatsParent = player.transform.Find("hats");
        if (hatsParent != null)
        {
            int hatCount = hatsParent.childCount;
            hats = new GameObject[hatCount];

            for (int i = 0; i < hatCount; i++)
            {
                hats[i] = hatsParent.GetChild(i).gameObject;
                hats[i].SetActive(false); // Deactivate all hats initially
            }
        }
        else
        {
            hats = new GameObject[0];
        }
    }

    public void NextBodyColor()
    {
        if (bodyColors.Length == 0) return;

        bodyColors[selectedBodyColorIndex].SetActive(false);
        selectedBodyColorIndex = (selectedBodyColorIndex + 1) % bodyColors.Length;
        bodyColors[selectedBodyColorIndex].SetActive(true);
    }

    public void PreviousBodyColor()
    {
        if (bodyColors.Length == 0) return;

        bodyColors[selectedBodyColorIndex].SetActive(false);
        selectedBodyColorIndex = (selectedBodyColorIndex - 1 + bodyColors.Length) % bodyColors.Length;
        bodyColors[selectedBodyColorIndex].SetActive(true);
    }

    public void NextHat()
    {
        if (hats.Length == 0) return;

        hats[selectedHatIndex].SetActive(false);
        selectedHatIndex = (selectedHatIndex + 1) % hats.Length;
        hats[selectedHatIndex].SetActive(true);
    }

    public void PreviousHat()
    {
        if (hats.Length == 0) return;

        hats[selectedHatIndex].SetActive(false);
        selectedHatIndex = (selectedHatIndex - 1 + hats.Length) % hats.Length;
        hats[selectedHatIndex].SetActive(true);
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("SelectedBodyColorIndex", selectedBodyColorIndex);
        PlayerPrefs.SetInt("SelectedHatIndex", selectedHatIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene("AR Board Scene");
    }
}
