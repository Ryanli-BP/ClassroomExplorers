using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Events;

public class DiceDisplay : MonoBehaviour
{
    public TextMeshProUGUI diceResultText; 
    private int totalResult = 0;          // Tracks the total sum of dice rolls

    public static UnityAction<int> OnDiceTotal; // Event for dice total calculated + 2s delay

    private void OnEnable()
    {
        Dice.OnDiceResult += HandleDiceResult;                 //For getting results of dice rolls
        DiceThrower.OnAllDiceFinished += DisplayTotalResult;   //For the bool of when all dice finish rolling
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= HandleDiceResult;
        DiceThrower.OnAllDiceFinished -= DisplayTotalResult;
    }

    private void HandleDiceResult(int diceResult)
    {
        totalResult += diceResult;
    }

    private async void DisplayTotalResult()
    {
        diceResultText?.SetText($"{totalResult}");

        // Reset the display after 2 seconds
        await Task.Delay(1000);
        ResetDisplay();
    }

    private void ResetDisplay()
    {
        OnDiceTotal?.Invoke(totalResult);   //Notify playermovement of dice result
        totalResult = 0;
        diceResultText?.SetText("");

        
    }
}
