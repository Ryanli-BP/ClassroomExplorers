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

    public IEnumerator HandleFight(Entity attacker, Entity defender)
    {
        // Record original positions
        Vector3 originalAttackerPosition = attacker.transform.position;
        Vector3 originalDefenderPosition = defender.transform.position;
        Quaternion originalAttackerRotation = attacker.transform.rotation;
        Quaternion originalDefenderRotation = defender.transform.rotation;
        Vector3 originalCameraPosition = arCamera.transform.position;

        // Teleport to fighting area
        TeleportToFightingArea(attacker, defender);

        yield return StartCoroutine(CombatSequence(attacker, defender));
        
        // Teleport back to board
        TeleportBackToBoard(attacker, defender, originalAttackerPosition, originalDefenderPosition, 
            originalAttackerRotation, originalDefenderRotation, originalCameraPosition);
    }

    private void TeleportToFightingArea(Entity attacker, Entity defender)
    {
        arCamera.transform.position = ArenaManager.Instance.GetCombatCameraPosition();
        attacker.transform.position = ArenaManager.Instance.GetCombatPlayerPosition();
        defender.transform.position = ArenaManager.Instance.GetCombatOpponentPosition();
        
        attacker.transform.LookAt(defender.transform);
        attacker.transform.Rotate(0, 180, 0);
        defender.transform.LookAt(attacker.transform);
        defender.transform.Rotate(0, 180, 0);
    }

    private void TeleportBackToBoard(Entity attacker, Entity defender, Vector3 originalAttackerPosition, 
        Vector3 originalDefenderPosition, Quaternion originalAttackerRotation, 
        Quaternion originalDefenderRotation, Vector3 originalCameraPosition)
    {
        if (PlatformUtils.IsRunningOnPC())
        {
            arCamera.transform.position = ArenaManager.Instance.GetBoardCameraPosition();
        }
        else
        {
            arCamera.transform.position = originalCameraPosition;
            arCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        attacker.transform.position = originalAttackerPosition;
        defender.transform.position = originalDefenderPosition;
        attacker.transform.rotation = originalAttackerRotation;
        defender.transform.rotation = originalDefenderRotation;
    }

    public IEnumerator CombatSequence(Entity attacker, Entity defender)
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
                var swordObject = attacker.transform.GetChild(0).gameObject;
                swordObject.SetActive(true);

                if (isEvade == true)
                {
                    // Start both animations simultaneously
                    IEnumerator attackAnim = attacker.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatOpponentPosition());
                    IEnumerator evadeAnim = null;
                    
                    if (evdValue > atkValue)
                    {
                        evadeAnim = defender.GetComponent<PlayerEvadeAnimation>().PerformEvade();
                        StartCoroutine(evadeAnim);
                    }
                    
                    yield return attackAnim;
                    if (evadeAnim != null)
                    {
                        while (defender.GetComponent<PlayerEvadeAnimation>().IsEvading)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    var shieldObject = defender.transform.GetChild(1).gameObject;
                    shieldObject.SetActive(true);
                    yield return StartCoroutine(attacker.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatOpponentPosition()));
                    shieldObject.SetActive(false);
                }
                
                swordObject.SetActive(false);
            }
            else
            {
                var swordObject = defender.transform.GetChild(0).gameObject;
                swordObject.SetActive(true);
                
                if (isEvade == true)
                {
                    IEnumerator attackAnim = defender.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatPlayerPosition());
                    IEnumerator evadeAnim = null;
                    
                    if (evdValue > atkValue)
                    {
                        evadeAnim = attacker.GetComponent<PlayerEvadeAnimation>().PerformEvade(false);
                        StartCoroutine(evadeAnim);
                    }
                    
                    yield return attackAnim;
                    if (evadeAnim != null)
                    {
                        while (attacker.GetComponent<PlayerEvadeAnimation>().IsEvading)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    var shieldObject = attacker.transform.GetChild(1).gameObject;
                    shieldObject.SetActive(true);
                    yield return StartCoroutine(defender.GetComponent<PlayerFightAnimation>().PerformAttack(ArenaManager.Instance.GetCombatPlayerPosition()));
                    shieldObject.SetActive(false);
                }
                
                swordObject.SetActive(false);
            }

            Entity target = (i == 0) ? defender : attacker;
            
            //Calculate damage
            if (isEvade == true)
            {
                int damage = (evdValue > atkValue) ? 0 : atkValue;
                target.LoseHealth(damage);
                if (damage > 0)
                {
                    UIManager.Instance.DisplayDamageNumber(target.transform.position, damage);
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                int damage = Math.Max(1, atkValue - dfdValue);
                target.LoseHealth(damage);
                UIManager.Instance.DisplayDamageNumber(target.transform.position, damage);
                yield return new WaitForSeconds(1f);
            }

            //Trigger death if die
            if (attacker.Health <= 0)
            {
                attacker.Dies();
                break;
            }
            else if (defender.Health <= 0)
            {
                defender.Dies();
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
