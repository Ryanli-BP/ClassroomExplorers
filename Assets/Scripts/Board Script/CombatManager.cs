using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private GameObject arCamera; // Reference to the AR camera



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
        Vector3 originalCameraPosition = arCamera.transform.position;

        // Teleport to fighting area
        TeleportToFightingArea(currentPlayer, opponentPlayer);

        yield return StartCoroutine(PlayerCombat(currentPlayer, opponentPlayer));
        Debug.Log("Teleporting back to board...");

        // Teleport back to board
        TeleportBackToBoard(currentPlayer, opponentPlayer, originalCurrentPlayerPosition, originalOpponentPlayerPosition, originalCurrentPlayerRotation, originalOpponentPlayerRotation, originalCameraPosition);
    }

    private void TeleportToFightingArea(Player currentPlayer, Player opponentPlayer)
    {
        
        arCamera.transform.position = ArenaManager.Instance.GetCombatCameraPosition();
        currentPlayer.transform.position = ArenaManager.Instance.GetCombatPlayerPosition();
        opponentPlayer.transform.position = ArenaManager.Instance.GetCombatOpponentPosition();
        
        currentPlayer.transform.LookAt(opponentPlayer.transform);
        currentPlayer.transform.Rotate(0, 180, 0);
        opponentPlayer.transform.LookAt(currentPlayer.transform);
        opponentPlayer.transform.Rotate(0, 180, 0);
    }

    private void TeleportBackToBoard(Player currentPlayer, Player opponentPlayer, Vector3 originalCurrentPlayerPosition, Vector3 originalOpponentPlayerPosition, Quaternion originalCurrentPlayerRotation, Quaternion originalOpponentPlayerRotation, Vector3 originalCameraPosition)
    {
        if (PlatformUtils.IsRunningOnPC()) //for camera
        {
            arCamera.transform.position = ArenaManager.Instance.GetBoardCameraPosition();

        }
        else
        {
            arCamera.transform.position = originalCameraPosition;
            arCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

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
            Debug.Log("enabling dice within combat");
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
                var swordObject = currentPlayer.transform.GetChild(0).gameObject;
                swordObject.SetActive(true);

                if (isEvade == true)
                {
                    // Start both animations simultaneously
                    IEnumerator attackAnim = currentPlayer.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatOpponentPosition());
                    IEnumerator evadeAnim = null;
                    
                    if (evdValue > atkValue)
                    {
                        evadeAnim = opponentPlayer.GetComponent<PlayerEvadeAnimation>().PerformEvade();
                        StartCoroutine(evadeAnim);
                    }
                    
                    yield return attackAnim;
                    if (evadeAnim != null)
                    {
                        while (opponentPlayer.GetComponent<PlayerEvadeAnimation>().IsEvading)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    var shieldObject = opponentPlayer.transform.GetChild(1).gameObject;
                    shieldObject.SetActive(true);
                    yield return StartCoroutine(currentPlayer.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatOpponentPosition()));
                    shieldObject.SetActive(false);
                }
                
                swordObject.SetActive(false);
            }
            else
            {
                var swordObject = opponentPlayer.transform.GetChild(0).gameObject;
                swordObject.SetActive(true);
                
                if (isEvade == true)
                {
                    IEnumerator attackAnim = opponentPlayer.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatPlayerPosition());
                    IEnumerator evadeAnim = null;
                    
                    if (evdValue > atkValue)
                    {
                        evadeAnim = currentPlayer.GetComponent<PlayerEvadeAnimation>().PerformEvade(false);
                        StartCoroutine(evadeAnim);
                    }
                    
                    yield return attackAnim;
                    if (evadeAnim != null)
                    {
                        while (currentPlayer.GetComponent<PlayerEvadeAnimation>().IsEvading)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    var shieldObject = currentPlayer.transform.GetChild(1).gameObject;
                    shieldObject.SetActive(true);
                    yield return StartCoroutine(opponentPlayer.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatPlayerPosition()));
                    shieldObject.SetActive(false);
                }
                
                swordObject.SetActive(false);
            }

            Player targetPlayer = (i == 0) ? opponentPlayer : currentPlayer;
            
            //Calculate damage
            if (isEvade == true)
            {
                int damage = (evdValue > atkValue) ? 0 : atkValue;
                targetPlayer.LoseHealth(damage);
                if (damage > 0)
                {
                    UIManager.Instance.DisplayDamageNumber(targetPlayer.transform.position, damage);
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                int damage = Math.Max(1, atkValue - dfdValue);
                targetPlayer.LoseHealth(damage);
                UIManager.Instance.DisplayDamageNumber(targetPlayer.transform.position, damage);
                yield return new WaitForSeconds(1f);
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
        GameManager.Instance.HandleCombatEnd();
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
