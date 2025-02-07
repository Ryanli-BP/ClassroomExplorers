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

    private IEnumerator CombatSequence(Entity attacker, Entity defender)
    {
        bool isAttackerBoss = attacker is Boss;
        bool isDefenderBoss = defender is Boss;

        for (int i = 0; i < 2; i++)
        {
            int atkValue = 0;
            int dfdValue = 0;
            bool? isEvade = null;
            int evdValue = 0;

            // Attack Phase
            if ((i == 0 && isAttackerBoss) || (i == 1 && isDefenderBoss))
            {
                // Automatic boss attack roll
                DiceManager.Instance.EnableDiceRoll();
                DiceManager.Instance.RollDice();
                yield return StartCoroutine(RollForCombatValue(result => atkValue = result));
            }
            else
            {
                // Player attack roll
                UIManager.Instance.SetRollDiceButtonText("Attack");
                DiceManager.Instance.EnableDiceRoll();
                yield return StartCoroutine(RollForCombatValue(result => atkValue = result));
            }

            // Defense Phase
            if ((i == 0 && isDefenderBoss) || (i == 1 && isAttackerBoss))
            {
                // Automatic boss defense roll (bosses always defend, never evade)
                isEvade = false;
                DiceManager.Instance.EnableDiceRoll();
                DiceManager.Instance.RollDice();
                yield return StartCoroutine(RollForCombatValue(result => dfdValue = result));
            }
            else
            {
                // Player defense/evade choice
                UIManager.Instance.SetRollDiceButtonText("Defend");
                DiceManager.Instance.EnableDiceRoll();
                UIManager.Instance.SetEvadeButtonVisibility(true);
            
                UIManager.Instance.rollDiceButton.onClick.AddListener(() => isEvade = false);
                UIManager.Instance.evadeButton.onClick.AddListener(() => isEvade = true);

                yield return new WaitUntil(() => isEvade != null);
                UIManager.Instance.SetEvadeButtonVisibility(false);

                if (isEvade == true)
                {
                    yield return StartCoroutine(RollForCombatValue(result => evdValue = result));
                }
                else
                {
                    yield return StartCoroutine(RollForCombatValue(result => dfdValue = result));
                }
            }

            Entity target = (i == 0) ? defender : attacker;

            // Only perform animations if neither entity is a boss
            if (!isAttackerBoss && !isDefenderBoss)
            {
                yield return StartCoroutine(PerformCombatAnimation(attacker, defender, isEvade, atkValue, evdValue, i == 1));
            }
            
            yield return StartCoroutine(ApplyDamage(target, isEvade, atkValue, dfdValue, evdValue));

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

    private IEnumerator PerformCombatAnimation(Entity attacker, Entity defender, bool? isEvade, int atkValue, int evdValue, bool isCounterAttack)
    {
        // Get the position of the attacker and coresponding sword for this attack round
        var attackerPos = isCounterAttack ? ArenaManager.Instance.GetCombatPlayerPosition() : ArenaManager.Instance.GetCombatOpponentPosition();
        var swordObject = (isCounterAttack ? defender : attacker).transform.GetChild(0).gameObject;
        swordObject.SetActive(true);

        if (isEvade == true)
        {
            IEnumerator attackAnim = (isCounterAttack ? defender : attacker).GetComponent<PlayerFightAnimation>().PerformAttack(attackerPos);
            IEnumerator evadeAnim = null;
            
            if (evdValue > atkValue) //start evade animation
            {
                evadeAnim = (isCounterAttack ? attacker : defender).GetComponent<PlayerEvadeAnimation>().PerformEvade(!isCounterAttack);
                StartCoroutine(evadeAnim);
            }
            yield return attackAnim; //wait for attack animation to finish

            if (evadeAnim != null) //wait for evade animation to finish
            {
                while ((isCounterAttack ? attacker : defender).GetComponent<PlayerEvadeAnimation>().IsEvading)
                {
                    yield return null;
                }
            }
        }
        else
        {
            // Display shield for defender
            var shieldObject = (isCounterAttack ? attacker : defender).transform.GetChild(1).gameObject;
            shieldObject.SetActive(true);
            yield return StartCoroutine((isCounterAttack ? defender : attacker).GetComponent<PlayerFightAnimation>().PerformAttack(attackerPos));
            shieldObject.SetActive(false);
        }
        
        swordObject.SetActive(false);
    }

    private IEnumerator ApplyDamage(Entity target, bool? isEvade, int atkValue, int dfdValue, int evdValue)
    {
        int damage;
        if (isEvade.HasValue && isEvade.Value) // Evade Calculation
        {
            damage = (evdValue > atkValue) ? 0 : atkValue;
            target.LoseHealth(damage);
            if (damage > 0)
            {
                UIManager.Instance.DisplayDamageNumber(target.transform.position, damage);
                yield return new WaitForSeconds(1f);
            }
        }
        else //Defend Calculation
        {
            damage = Math.Max(1, atkValue - dfdValue);
            target.LoseHealth(damage);
            UIManager.Instance.DisplayDamageNumber(target.transform.position, damage);
            yield return new WaitForSeconds(1f);
        }
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
