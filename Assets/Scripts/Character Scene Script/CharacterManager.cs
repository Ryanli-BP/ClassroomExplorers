using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text statusText; 

    private GameObject[] bodyColors;
    private int selectedBodyColorIndex = 0;

    private GameObject[] hats;
    private int selectedHatIndex = 0;
    
    private void Start()
    {
        InitializeBodyColors();
        InitializeHats();

        if (bodyColors.Length > 0)
        {
            bodyColors[selectedBodyColorIndex].SetActive(true);
        }
        if (hats.Length > 0)
        {
            hats[selectedHatIndex].SetActive(true);
        }

        startButton.interactable = false; 
        StartCoroutine(WaitForPhotonConnection());
    }

    private IEnumerator WaitForPhotonConnection()
    {
        statusText.text = "Connecting to Photon...";
        
        while (!PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.JoinedLobby)
        {
            yield return null; 
        }

        startButton.interactable = true;
        statusText.text = "Connected! Ready to Start";
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
                bodyColors[i].SetActive(false);
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
                hats[i].SetActive(false);
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
        // Store selected customization options in PlayerPrefs (local storage)
        PlayerPrefs.SetInt("SelectedBodyColorIndex", selectedBodyColorIndex);
        PlayerPrefs.SetInt("SelectedHatIndex", selectedHatIndex);
        PlayerPrefs.Save();

        // Store customization data in Photon Custom Properties (networked storage)
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "BodyColor", selectedBodyColorIndex },
            { "Hat", selectedHatIndex }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
        else
        {
            Debug.Log("Wait for Photon Connection");
        }
    }
}
