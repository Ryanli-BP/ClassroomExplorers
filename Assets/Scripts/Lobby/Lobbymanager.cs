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
        if (PhotonNetwork.LocalPlayer.CustomProperties == null)
        {
            PhotonNetwork.LocalPlayer.CustomProperties = new ExitGames.Client.Photon.Hashtable();
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerNumber"))
        {
            Debug.LogWarning($"Player {PhotonNetwork.LocalPlayer.NickName} is already registered in the room! Skipping duplicate assignment.");
            return;
        }

        int playerNumber = AssignPlayerNumber();

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerNumber", playerNumber }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = $"Player {playerNumber}";
        }

        Debug.Log($"Assigned {PhotonNetwork.NickName} as Player {playerNumber}");

        if (photonView != null)
        {
            photonView.RPC("SyncPlayerList", RpcTarget.All);
        }
        else
        {
            Debug.LogError("PhotonView is NULL in OnJoinedRoom!");
        }
    }

    private int AssignPlayerNumber()
    {
        HashSet<int> usedNumbers = new HashSet<int>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("PlayerNumber"))
            {
                int num = (int)player.CustomProperties["PlayerNumber"];
                usedNumbers.Add(num);
            }
        }

        for (int i = 1; i <= 6; i++) 
        {
            if (!usedNumbers.Contains(i))
            {
                return i;
            }
        }

        return 6; 
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered. Current total: {PhotonNetwork.CurrentRoom.PlayerCount}");
        photonView.RPC("SyncPlayerList", RpcTarget.All);
    }

    [PunRPC]
    void SyncPlayerList()
    {
        Debug.Log("Syncing player list across all clients...");
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
