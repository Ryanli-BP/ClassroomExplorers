using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text playersListText; 
    [SerializeField] private Button startButton;

    private void Start()
    {
        startButton.gameObject.SetActive(false);
        startButton.onClick.AddListener(StartGame);

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC("SyncPlayerList", RpcTarget.All);
            }
            else
            {
                JoinRoom();
            }
        }
    }

    private void JoinRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Already in a room! Skipping JoinRoom.");
            return;
        }

        RoomOptions options = new RoomOptions { MaxPlayers = 6, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("GameRoom", options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Nickname"))
        {
            PhotonNetwork.NickName = (string)PhotonNetwork.LocalPlayer.CustomProperties["Nickname"];
        }
        else
        {
            PhotonNetwork.NickName = $"Player {PhotonNetwork.LocalPlayer.ActorNumber}"; // Fallback
        }

        Debug.Log($"[LobbyManager] {PhotonNetwork.NickName} joined the lobby");

        if (photonView != null)
        {
            photonView.RPC("SyncPlayerList", RpcTarget.All);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered. Current total: {PhotonNetwork.CurrentRoom.PlayerCount}");


        photonView.RPC("SyncPlayerList", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber}) left the room.");
        
        
        photonView.RPC("SyncPlayerList", RpcTarget.All);
    }

    [PunRPC]
    void SyncPlayerList()
    {
        Debug.Log($"[SyncPlayerList] Running. Total Players: {PhotonNetwork.PlayerList.Length}");
        Debug.Log("")
        foreach (var player in PhotonNetwork.PlayerList)
        {
        string nickname = player.CustomProperties.ContainsKey("Nickname") 
            ? player.CustomProperties["Nickname"].ToString() 
            : "Unnamed Player";
            Debug.Log($"[SyncPlayerList] Player in list: {player.NickName}");
            Debug.Log($"[SyncPlayerList] Player: {nickname}, Actor Number: {player.ActorNumber}");

        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Update UI called");
            StringBuilder playerList = new StringBuilder();

            foreach (var player in PhotonNetwork.PlayerList)
            {
                string displayName = player.CustomProperties.ContainsKey("Nickname") 
                    ? (string)player.CustomProperties["Nickname"] 
                    : player.NickName; 

                playerList.AppendLine(displayName);
            }

            playersText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/6";
            playersListText.text = playerList.ToString();
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2);
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("AR Board Scene");
        }
    }
}
