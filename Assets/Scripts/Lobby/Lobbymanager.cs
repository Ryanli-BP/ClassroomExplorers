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
        RoomOptions options = new RoomOptions { MaxPlayers = 6, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("GameRoom", options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        UpdateUI();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
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
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}
