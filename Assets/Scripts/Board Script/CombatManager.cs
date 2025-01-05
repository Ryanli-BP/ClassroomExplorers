using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private Camera arCamera; // Reference to the AR camera

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator HandleFight(int opponentPlayerID, int currentPlayerID)
    {
        // Record original positions
        Player currentPlayer = PlayerManager.Instance.GetPlayerByID(currentPlayerID);
        Player opponentPlayer = PlayerManager.Instance.GetPlayerByID(opponentPlayerID);
        Vector3 originalCurrentPlayerPosition = currentPlayer.gameObject.transform.position;
        Vector3 originalOpponentPlayerPosition = opponentPlayer.gameObject.transform.position;

        // Teleport to fighting area
        arCamera.transform.position = new Vector3(30, 9, -9);
        currentPlayer.transform.position = new Vector3(29, 2, 0);
        opponentPlayer.transform.position = new Vector3(28, 2, 0);

        // Implement fighting logic here
        yield return StartCoroutine(PlayerCombat(currentPlayer, opponentPlayer));
        Debug.Log("Teleporting back to board...");

        // Teleport back to board
        arCamera.transform.position = new Vector3(0, 9, -9);
        currentPlayer.transform.position = originalCurrentPlayerPosition;
        opponentPlayer.transform.position = originalOpponentPlayerPosition;

        // Handle fight result (e.g., update player health, determine winner, etc.)
        Debug.Log("Fight ended. Handle fight result here.");
    }

    public IEnumerator PlayerCombat(Player currentPlayer, Player opponentPlayer)
    {
        for (int i = 0; i < 2; i++)
        {
            int atkValue = 0;
            int dfdValue = 0;

            yield return StartCoroutine(RollForCombatValue(result => atkValue = result));
            yield return StartCoroutine(RollForCombatValue(result => dfdValue = result));

            Debug.Log($"Combat result: Attack = {atkValue}, Defense = {dfdValue}");

            Player targetPlayer = (i == 0) ? opponentPlayer : currentPlayer;
            targetPlayer.LoseHealth(Math.Max(1, atkValue - dfdValue));

            if (currentPlayer.Health <= 0)
            {
                currentPlayer.Dies();
                break;
            }
            else if (opponentPlayer.Health <= 0)
            {
                opponentPlayer.Dies();
                break;
            }
        }
    }

    private IEnumerator RollForCombatValue(System.Action<int> callback)
    {
        bool diceRollComplete = false;
        bool diceDisplayComplete = false;
        int diceResult = 0;

        // Enable dice roll
        DiceManager.Instance.EnableDiceRoll();

        // Set up the callback to handle the result of the dice roll
        GameManager.Instance.OnDiceRollResultForCombat = (result) =>
        {
            diceResult = result;
            diceRollComplete = true;
        };

        // Wait for the dice roll to complete
        yield return new WaitUntil(() => diceRollComplete);

        // Set up the callback to handle the display completion
        GameManager.Instance.OnDiceResultDisplayForCombat = (result) =>
        {
            diceDisplayComplete = result; // Set the flag to true when the display is complete
        };

        // Wait until the dice result display is finished
        yield return new WaitUntil(() => diceDisplayComplete);

        // Use the callback to return the dice result
        callback(diceResult);
    }
}