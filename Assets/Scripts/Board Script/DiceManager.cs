using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation.VisualScripting;
using Photon.Pun;
using ExitGames.Client.Photon.StructWrapping;
public class DiceManager : MonoBehaviourPun
{
    public static DiceManager Instance;
    [SerializeField] private Dice DiceToThrow;
    private int numDice;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float rollForce = 10f;
    [SerializeField] private int testDiceResult = 5;

    private List<Dice> liveDice = new List<Dice>();
    private int remainingDice;  // Tracks remaining dice to finish rolling
    private int totalDiceResult; // Tracks the total sum of dice rolls
    private bool canRollDice = false; // Flag to control dice rolling
    private bool isInCombat = false; // Track if we're in combat

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnEnable()
    {
        remainingDice = numDice;  // Initialize remaining dice count
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Update()
    {
        // Update dice position when not in combat to follow the boardDiceSpot
        if (PlatformUtils.IsRunningOnPC() && !isInCombat)
        {
            transform.position = CameraManager.Instance.GetboardDicePosition();
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        isInCombat = (newState == GameState.PlayerCombat);
        
        // Update dice position based on game state
        if (isInCombat)
        {
            transform.position = CameraManager.Instance.GetCombatDicePosition();
        }
        else 
        {
            // For non-AR/PC version, we'll update position in Update method
            if (!PlatformUtils.IsRunningOnPC())
            {
                transform.position = CameraManager.Instance.GetboardDicePosition();
            }
        }
    }

    public int NumDice
    {
        get { return numDice; }
        set { numDice = value; }
    }

    public void EnableDiceRoll(bool isBossRollingDice)
    {
        // Set number of dice based on current turn
        if (isBossRollingDice)
        {
            numDice = 1 + BossManager.Instance.activeBoss.BossBuffs.ExtraDiceBonus;
            Debug.Log($"Boss dice count: {numDice} FROM BONUS {BossManager.Instance.activeBoss.BossBuffs.ExtraDiceBonus}");
        }
        else
        {
            numDice = 1 + PlayerManager.Instance.GetPlayerList()[RoundManager.Instance.Turn - 1].PlayerBuffs.ExtraDiceBonus;
            Debug.Log($"Player dice count: {numDice} FROM BONUS {PlayerManager.Instance.GetPlayerList()[RoundManager.Instance.Turn - 1].PlayerBuffs.ExtraDiceBonus}");
        }

        totalDiceResult = 0;
        canRollDice = true;
        UIManager.Instance.SetRollDiceButtonVisibility(true);
    }

    private Vector3 originalGravity;

    public void RollDice()
    {
        if (canRollDice)
        {
            remainingDice = numDice;  // Reset remaining dice count to the total number of dice
            canRollDice = false;
            UIManager.Instance.SetRollDiceButtonVisibility(false);

            originalGravity = Physics.gravity;
            Physics.gravity = new Vector3(0, -20f * ARBoardPlacement.worldScale, 0); // Increase gravity

            // Unsubscribe from previous dice finish events before destroying old dice
            foreach (var die in liveDice)
            {
                die.OnDiceFinishedRolling -= HandleDiceFinishedRolling;
                PhotonNetwork.Destroy(die.gameObject);
            }
            liveDice.Clear();

            // Force camera manager to update the dice position if it's on PC
            if (PlatformUtils.IsRunningOnPC() && !isInCombat)
            {
                CameraManager.Instance.ForceUpdateDicePosition();
            }

            // Get the current dice position from the camera manager
            Vector3 dicePosition = isInCombat ? 
                CameraManager.Instance.GetCombatDicePosition() : 
                CameraManager.Instance.GetboardDicePosition();

            // Update our position to match
            transform.position = dicePosition;

            // Instantiate and roll new dice
            for (int i = 0; i < numDice; i++)
            {
                Dice diceLive = PhotonNetwork.Instantiate(DiceToThrow.name, transform.position, ARBoardPlacement.boardRotation * transform.rotation).GetComponent<Dice>();
                diceLive.transform.localScale = diceLive.transform.localScale * ARBoardPlacement.worldScale;
                liveDice.Add(diceLive);
                diceLive.RollDice(throwForce, rollForce, i);
                diceLive.OnDiceFinishedRolling += HandleDiceFinishedRolling;  // Subscribe to dice finish event
            }
        }
    }

    private void HandleDiceFinishedRolling()
    {
        remainingDice--;

        if (remainingDice <= 0)
        {
            // Reset the gravity to its original value
            Physics.gravity = originalGravity;
            photonView.RPC("RPC_OnDiceRollComplete", RpcTarget.All); // Directly call the GameManager method
        }
    }
    [PunRPC]
    public void RPC_HandleDiceResult(int diceResult)
    {
        totalDiceResult += 6;
        Debug.Log($"[RPC] Dice result received: {diceResult} (Total: {totalDiceResult})");

    }

    public int GetTotalDiceResult()
    {
        return totalDiceResult;
    }
    [PunRPC]
    public void RPC_OnDiceRollComplete()
    {
        StartCoroutine(GameManager.Instance.OnDiceRollComplete());
    }
}