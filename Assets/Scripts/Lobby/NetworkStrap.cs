using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkStrap : MonoBehaviourPunCallbacks
{
    public static NetworkStrap Instance;
    public const string GAME_SCENE = "AR Board Scene";
    public const string LOBBY_SCENE = "Lobby";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
    }

    private void ConnectToPhoton()
    {
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master!");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void LoadLobbyScene()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LoadLevel(LOBBY_SCENE);
        }
        else
        {
            Debug.LogWarning("Not connected to Photon! Cannot load lobby.");
        }
    }
}
