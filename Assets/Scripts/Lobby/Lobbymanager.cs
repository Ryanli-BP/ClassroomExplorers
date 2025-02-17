using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private HashSet<int> assignedPlayerNumbers = new HashSet<int>();
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text playersListText; 
    [SerializeField] private Button startButton;
    private bool isConnecting;
    private void Start()
    {
        startButton.gameObject.SetActive(false);
        startButton.onClick.AddListener(StartGame);

        if (PhotonNetwork.IsConnected)
        {
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
    public override void OnConnectedToMaster()
    {
        JoinRoom();
    }

    private void JoinRoom()
    {
 
        RoomOptions options = new RoomOptions { MaxPlayers = 6, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("GameRoom", options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if (photonView != null)
        {
            // Assign player number and update UI
            int playerNumber = AssignPlayerNumber();
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("PlayerNumber", playerNumber);
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            
            UpdateUI();
        }
    }


    private int AssignPlayerNumber()
    {
        HashSet<int> usedNumbers = new HashSet<int>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("PlayerNumber"))
            {
                usedNumbers.Add((int)player.CustomProperties["PlayerNumber"]);
            }
        }

        for (int i = 1; i <= 6; i++)
        {
            if (!usedNumbers.Contains(i)) return i;
        }
        return 6;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        photonView.RPC("SyncPlayerList", RpcTarget.All);
    }
    [PunRPC]
    void SyncPlayerList()
    {
        UpdateUI();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber}) left the room.");

        // No need to manually clear properties - Photon handles it
        RenumberPlayers();
        photonView.RPC("SyncPlayerList", RpcTarget.All);
    }

    private void RenumberPlayers()
    {
        HashSet<int> newAssignedNumbers = new HashSet<int>();
        int newNumber = 1;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            // Only allow the local player to change its own properties
            if (player.IsLocal)
            {
                PhotonNetwork.NickName = $"Player {newNumber}";
            }

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerNumber", newNumber }
            };
            player.SetCustomProperties(props);

            Debug.Log($"Reassigned {player.NickName} to Player {newNumber}");
            newAssignedNumbers.Add(newNumber);
            newNumber++;
        }

        assignedPlayerNumbers = newAssignedNumbers;
    }

    private void UpdateUI()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Update UI called");
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
