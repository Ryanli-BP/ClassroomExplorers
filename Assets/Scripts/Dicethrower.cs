using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class DiceThrower : MonoBehaviour
{
    public Dice DiceToThrow;
    public int numDice = 3;
    public float throwForce = 5f;
    public float rollForce = 10f;

    private List<Dice> liveDice = new List<Dice>();
    private int remainingDice;  // Tracks remaining dice to finish rolling

    public static UnityAction OnAllDiceFinished;  // Event triggered when all dice finish rolling

    private void OnEnable()
    {
        remainingDice = numDice;  // Initialize remaining dice count
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollDice();
        }
    }

    private void RollDice()
    {
        remainingDice = numDice;  // Reset remaining dice count to the total number of dice

        // Unsubscribe from previous dice finish events before destroying old dice
        foreach (var die in liveDice)
        {
            die.OnDiceFinishedRolling -= HandleDiceFinishedRolling;
            Destroy(die.gameObject);
        }
        liveDice.Clear();

        // Instantiate and roll new dice
        for (int i = 0; i < numDice; i++)
        {
            Dice diceLive = Instantiate(DiceToThrow, transform.position, transform.rotation);
            liveDice.Add(diceLive);
            diceLive.RollDice(throwForce, rollForce, i);
            diceLive.OnDiceFinishedRolling += HandleDiceFinishedRolling;  // Subscribe to dice finish event
        }
    }

    private void HandleDiceFinishedRolling()
    {
        remainingDice--;

        if (remainingDice <= 0)
        {
            OnAllDiceFinished?.Invoke();
        }
    }
}
