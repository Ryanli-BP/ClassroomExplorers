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
            JoinRoom();
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
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Joined"))
        {
            Debug.LogWarning("Player already registered in the room!");
            return;
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "Joined", true }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"Joined Room with {PhotonNetwork.CurrentRoom.PlayerCount} players");

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
