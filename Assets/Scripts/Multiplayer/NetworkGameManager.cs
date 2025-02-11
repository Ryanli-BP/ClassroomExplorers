using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public static NetworkGameManager Instance;
    private const string GAME_VERSION = "1.0";
    private const byte MAX_PLAYERS = 4;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        Connect();
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = GAME_VERSION;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        JoinOrCreateRoom();
    }

    private void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MAX_PLAYERS,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinRandomOrCreateRoom(null, MAX_PLAYERS, MatchmakingMode.FillRoom, 
            null, null, null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.InitializeGame();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} joined room");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.ActorNumber} left room");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room creation failed: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room join failed: {message}");
    }
}