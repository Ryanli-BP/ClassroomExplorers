using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem; // Required for the new Input System

public class DiceThrower : MonoBehaviour
{
    public Dice DiceToThrow;
    public int numDice = 3;
    public float throwForce = 5f;
    public float rollForce = 10f;

    private List<Dice> liveDice = new List<Dice>();
    private int remainingDice;  // Tracks remaining dice to finish rolling
    public int totalResult = 0;

    public static UnityAction OnAllDiceFinished;  // Event triggered when all dice finish rolling

    private InputAction rollDiceAction; // New InputAction for rolling dice

    private void OnEnable()
    {
        remainingDice = numDice;  // Initialize remaining dice count

        // Get and enable the InputAction for rolling the dice
        rollDiceAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        rollDiceAction.Enable();
        rollDiceAction.performed += OnRollDicePerformed;
    }

    private void OnDisable()
    {
        // Disable the InputAction and unsubscribe from the event
        rollDiceAction.performed -= OnRollDicePerformed;
        rollDiceAction.Disable();
    }

    private void OnRollDicePerformed(InputAction.CallbackContext context)
    {
        RollDice(); // Trigger the dice roll when the action is performed
    }

    private void RollDice()
    {
        remainingDice = numDice;  // Reset remaining dice count to the total number of dice
        totalResult = 0; 
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
        foreach (var die in liveDice)
        {
            totalResult += die.GetResult();
        }
        if (remainingDice <= 0)
        {
            OnAllDiceFinished?.Invoke();
        }
    }

    public int GetDiceTotal()
    {
        return totalResult;
    }
}
