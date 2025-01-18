using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private GameObject arCamera; // Reference to the AR camera

    private static readonly Vector3 currentPlayerPosition = new Vector3(47, 0.2f, 0);
    private static readonly Vector3 opponentPlayerPosition = new Vector3(54, 0.2f, 0);
    private static readonly Vector3 defaultCameraPosition = new Vector3(0, 8, -10);

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
        Quaternion originalCurrentPlayerRotation = currentPlayer.gameObject.transform.rotation;
        Quaternion originalOpponentPlayerRotation = opponentPlayer.gameObject.transform.rotation;

        // Teleport to fighting area
        TeleportToFightingArea(currentPlayer, opponentPlayer);

        yield return StartCoroutine(PlayerCombat(currentPlayer, opponentPlayer));
        Debug.Log("Teleporting back to board...");

        // Teleport back to board
        TeleportBackToBoard(currentPlayer, opponentPlayer, originalCurrentPlayerPosition, originalOpponentPlayerPosition, originalCurrentPlayerRotation, originalOpponentPlayerRotation);
    }

    private void TeleportToFightingArea(Player currentPlayer, Player opponentPlayer)
    {
        arCamera.transform.position = defaultCameraPosition + new Vector3(50, 0, 0);
        currentPlayer.transform.position = currentPlayerPosition;
        opponentPlayer.transform.position = opponentPlayerPosition;
        currentPlayer.transform.LookAt(opponentPlayer.transform);
        currentPlayer.transform.Rotate(0, 180, 0); // offset
        opponentPlayer.transform.LookAt(currentPlayer.transform);
        opponentPlayer.transform.Rotate(0, 180, 0);
    }

    private void TeleportBackToBoard(Player currentPlayer, Player opponentPlayer, Vector3 originalCurrentPlayerPosition, Vector3 originalOpponentPlayerPosition, Quaternion originalCurrentPlayerRotation, Quaternion originalOpponentPlayerRotation)
    {
        arCamera.transform.position = defaultCameraPosition;
        currentPlayer.transform.position = originalCurrentPlayerPosition;
        opponentPlayer.transform.position = originalOpponentPlayerPosition;
        currentPlayer.transform.rotation = originalCurrentPlayerRotation;
        opponentPlayer.transform.rotation = originalOpponentPlayerRotation;
    }

    public IEnumerator PlayerCombat(Player currentPlayer, Player opponentPlayer)
    {
        for (int i = 0; i < 2; i++)
        {
            int atkValue = 0;
            int dfdValue = 0;
            bool? isEvade = null;
            int evdValue = 0;

            //Leverages the RolDiceButton for attack and defend by simply changing the text
            UIManager.Instance.SetRollDiceButtonText("Attack");
            DiceManager.Instance.EnableDiceRoll();
            yield return StartCoroutine(RollForCombatValue(result => atkValue = result));

            UIManager.Instance.SetRollDiceButtonText("Defend");
            DiceManager.Instance.EnableDiceRoll();
            UIManager.Instance.SetEvadeButtonVisibility(true);
           
            // Wait for player to choose between defend and evade
            UIManager.Instance.rollDiceButton.onClick.AddListener(() => isEvade = false);
            UIManager.Instance.evadeButton.onClick.AddListener(() => isEvade = true);

            yield return new WaitUntil(() => isEvade != null);
            UIManager.Instance.SetEvadeButtonVisibility(false);

            if (isEvade == true)
            {
                yield return StartCoroutine(RollForCombatValue(result => evdValue = result));
                Debug.Log($"Combat result: Attack = {atkValue}, Evade = {evdValue}");
            }
            else
            {
                yield return StartCoroutine(RollForCombatValue(result => dfdValue = result));
                Debug.Log($"Combat result: Attack = {atkValue}, Defense = {dfdValue}");
            }

            //Attack "animation"
            if (i == 0)
            {
                yield return StartCoroutine(currentPlayer.GetComponent<PlayerFightAnimation>().PerformAttack(opponentPlayerPosition));
            }
            else
            {
                yield return StartCoroutine(opponentPlayer.GetComponent<PlayerFightAnimation>().PerformAttack(currentPlayerPosition));
            }

            Player targetPlayer = (i == 0) ? opponentPlayer : currentPlayer;
            
            //Calculate damage
            if (isEvade == true)
            {
                targetPlayer.LoseHealth((evdValue > atkValue) ? 0 : atkValue);
            }
            else
            {
                targetPlayer.LoseHealth(Math.Max(1, atkValue - dfdValue));
            }

            //Trigger death if die
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
        UIManager.Instance.SetRollDiceButtonText("Roll Dice");
    }

    private IEnumerator RollForCombatValue(System.Action<int> callback)
    {
        bool diceRollComplete = false;
        bool diceDisplayComplete = false;
        int diceResult = 0;

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