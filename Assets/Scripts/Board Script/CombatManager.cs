using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private GameObject arCamera; // Reference to the AR camera

    //FirstTA means FIRST TO ACT, SecondTA means SECOND TO ACT

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator HandleFight(Entity FirstTA, Entity SecondTA)
    {
        // Record original positions
        Vector3 originalFirstTAPosition = FirstTA.transform.position;
        Vector3 originalSecondTAPosition = SecondTA.transform.position;
        Quaternion originalFirstTARotation = FirstTA.transform.rotation;
        Quaternion originalSecondTARotation = SecondTA.transform.rotation;
        Vector3 originalCameraPosition = arCamera.transform.position;

        // Teleport to fighting area
        TeleportToFightingArea(FirstTA, SecondTA);

        yield return StartCoroutine(CombatSequence(FirstTA, SecondTA));
        
        // Teleport back to board
        TeleportBackToBoard(FirstTA, SecondTA, originalFirstTAPosition, originalSecondTAPosition, 
            originalFirstTARotation, originalSecondTARotation, originalCameraPosition);
    }

    private void TeleportToFightingArea(Entity FirstTA, Entity SecondTA)
    {
        arCamera.transform.position = ArenaManager.Instance.GetCombatCameraPosition();
        FirstTA.transform.position = ArenaManager.Instance.GetCombatPlayerPosition();
        SecondTA.transform.position = ArenaManager.Instance.GetCombatOpponentPosition();
        
        FirstTA.transform.LookAt(SecondTA.transform);
        FirstTA.transform.Rotate(0, 180, 0);
        SecondTA.transform.LookAt(FirstTA.transform);
        SecondTA.transform.Rotate(0, 180, 0);
    }

    private void TeleportBackToBoard(Entity FirstTA, Entity SecondTA, Vector3 originalFirstTAPosition, 
        Vector3 originalSecondTAPosition, Quaternion originalFirstTARotation, 
        Quaternion originalSecondTARotation, Vector3 originalCameraPosition)
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

        FirstTA.transform.position = originalFirstTAPosition;
        SecondTA.transform.position = originalSecondTAPosition;
        FirstTA.transform.rotation = originalFirstTARotation;
        SecondTA.transform.rotation = originalSecondTARotation;
    }

    private IEnumerator CombatSequence(Entity FirstTA, Entity SecondTA)
    {
        bool BossisFirstTA = FirstTA is Boss;
        bool BossisSecondTA = SecondTA is Boss;

        for (int turn = 0; turn < 2; turn++)
        {
            int atkValue = 0;
            int dfdValue = 0;
            bool? isEvade = null;
            int evdValue = 0;

            // Attack Phase
            if ((turn == 0 && BossisFirstTA) || (turn == 1 && BossisSecondTA))
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
            if ((turn == 0 && BossisSecondTA) || (turn == 1 && BossisFirstTA))
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

            Entity target = (turn == 0) ? SecondTA : FirstTA;

            yield return StartCoroutine(PerformCombatAnimation(FirstTA, SecondTA, isEvade, atkValue, evdValue, turn == 1));
            
            yield return StartCoroutine(ApplyDamage(target, isEvade, atkValue, dfdValue, evdValue));

            yield return new WaitForSeconds(0.4f);

            if (FirstTA.Health <= 0)
            {
                FirstTA.Dies();
                break;
            }
            else if (SecondTA.Health <= 0)
            {
                SecondTA.Dies();
                break;
            }
        }

        UIManager.Instance.SetRollDiceButtonText("Roll Dice");
        GameManager.Instance.HandleCombatEnd();
    }

    private IEnumerator PerformAttackAnimation(Entity attacker, Entity target)
    {
        var attackerPos = target.transform.position;

        if (attacker is Boss)
        {
            yield return StartCoroutine(attacker.GetComponent<BossFightAnimation>().PerformAttack(attackerPos));
        }
        else
        {
            var swordObject = attacker.transform.GetChild(0).gameObject;
            swordObject.SetActive(true);
            yield return StartCoroutine(attacker.GetComponent<PlayerFightAnimation>().PerformAttack(attackerPos));
            swordObject.SetActive(false);
        }
    }

    private IEnumerator PerformDefendAnimation(Entity defender)
    {
        if (defender is Boss)
        {
            // TODO: Implement boss defense animation if needed
            yield break;
        }

        var shieldObject = defender.transform.GetChild(1).gameObject;
        shieldObject.SetActive(true);
        yield return new WaitForSeconds(1f); // Animation duration
        shieldObject.SetActive(false);
    }

    private IEnumerator PerformEvadeAnimation(Entity defender, bool evadeRight)
    {
        if (defender is Boss)
        {
            yield break;
        }

        yield return StartCoroutine(defender.GetComponent<PlayerEvadeAnimation>().PerformEvade(evadeRight));
    }

    private IEnumerator NoAnimation()
    {
        yield break;
    }

    //Basically, the initial FirstTA is FirstTA, the "SecondTA" then counter-attacks. so both attack/defends
        //it's only indictative of the order of how the fight happens(bad naming I know, bit lazy to change now)
    private IEnumerator PerformCombatAnimation(Entity FirstTA, Entity SecondTA, bool? isEvade, int atkValue, int evdValue, bool isTurn1)
    {
        Entity attacker = isTurn1 ? SecondTA : FirstTA;
        Entity defender = isTurn1 ? FirstTA : SecondTA;

        // Start both animations simultaneously
        IEnumerator attackAnim = PerformAttackAnimation(attacker, defender);
        IEnumerator defendAnim;

        if (defender is Boss)
        {
            defendAnim = PerformDefendAnimation(defender);
        }

        else if (isEvade == true)
        {
            defendAnim = evdValue > atkValue ? 
                PerformEvadeAnimation(defender, isTurn1) : 
                NoAnimation();
        }
        else
        {
            defendAnim = PerformDefendAnimation(defender);
        }

        // Run both animations concurrently
        StartCoroutine(attackAnim);
        StartCoroutine(defendAnim);

        // Wait for both animations to complete
        while (attacker.GetComponent<PlayerFightAnimation>()?.IsAttacking == true || 
            attacker.GetComponent<BossFightAnimation>()?.IsAttacking == true ||
            defender.GetComponent<PlayerEvadeAnimation>()?.IsEvading == true)
        {
            yield return null;
        }
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
