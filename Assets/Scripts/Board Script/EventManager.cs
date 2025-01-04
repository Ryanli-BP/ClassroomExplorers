using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

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
        bool diceRollComplete = false;
        bool diceDisplayComplete = false;
        int diceResult = 0;
        int atkValue = 0;
        int dfdValue = 0;

        // First dice roll for AtkValue
        DiceManager.Instance.EnableDiceRoll();

        // Set up the callback to handle the result of the first dice roll
        GameManager.Instance.OnDiceRollResultForCombat = (result) =>
        {
            diceResult = result;
            diceRollComplete = true;
        };

        // Wait for the first dice roll to complete
        yield return new WaitUntil(() => diceRollComplete);

        atkValue = diceResult;
        Debug.Log($"Attack Value: {atkValue}");

        // Set up the callback to handle the display completion
        GameManager.Instance.OnDiceResultDisplayForCombat = (result) =>
        {
            diceDisplayComplete = result; // Set the flag to true when the display is complete
        };

        // Wait until the dice result display is finished
        yield return new WaitUntil(() => diceDisplayComplete);

        // Second dice roll for DfdValue
        diceRollComplete = false; // Reset the dice roll completion flag
        diceDisplayComplete = false; // Reset the display completion flag
        DiceManager.Instance.EnableDiceRoll();

        // Set up the callback to handle the result of the second dice roll
        GameManager.Instance.OnDiceRollResultForCombat = (result) =>
        {
            diceResult = result;
            diceRollComplete = true;
        };

        // Wait for the second dice roll to complete
        yield return new WaitUntil(() => diceRollComplete);

        dfdValue = diceResult;
        Debug.Log($"Defense Value: {dfdValue}");

        // Set up the callback to handle the display completion after the second roll
        GameManager.Instance.OnDiceResultDisplayForCombat = (result) =>
        {
            diceDisplayComplete = result; // Set the flag to true when the display is complete
        };

        // Wait until the dice result display is finished
        yield return new WaitUntil(() => diceDisplayComplete);

        // Proceed with combat logic using atkValue and dfdValue
        Debug.Log($"Combat result: Attack = {atkValue}, Defense = {dfdValue}");
    }


}