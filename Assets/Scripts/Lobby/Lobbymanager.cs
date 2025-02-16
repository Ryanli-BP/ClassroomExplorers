using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text playersListText; 
    [SerializeField] private Button startButton;

    private void Start()
    {
        startButton.gameObject.SetActive(false);

        if (PhotonNetwork.IsConnected)
        {
            // If already in a room, update UI; otherwise, join a room.
            if (PhotonNetwork.InRoom)
            {
                UpdateUI();
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
        int playerNumber = PhotonNetwork.CurrentRoom.PlayerCount; 

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerNumber", playerNumber } 
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        PhotonNetwork.NickName = $"Player {playerNumber}"; 

        Debug.Log($"Joined Room as {PhotonNetwork.NickName} with Player Number: {playerNumber}");
        UpdateUI();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
        }
    }


    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered. Current total: {PhotonNetwork.CurrentRoom.PlayerCount}");

        if (PhotonNetwork.CurrentRoom.Players.ContainsKey(newPlayer.ActorNumber))
        {
            Debug.LogError($"Duplicate player detected: {newPlayer.ActorNumber}");
        }

        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
    
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Update Ui called");
            StringBuilder playerList = new StringBuilder();
            foreach (var player in PhotonNetwork.PlayerList)
            {
                playerList.AppendLine(player.NickName);
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
