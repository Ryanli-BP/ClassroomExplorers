using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class DiceDisplay : MonoBehaviour
{
    public TextMeshProUGUI diceResultText; 
    private int totalResult = 0;          // Tracks the total sum of dice rolls

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

    private void HandleDiceResult(int diceNum, int diceResult)
    {
        totalResult += diceResult;
    }

    private async void DisplayTotalResult()
    {
        diceResultText?.SetText($"{totalResult}");

        // Reset the display after 2 seconds
        await Task.Delay(2000);
        ResetDisplay();
    }

    private void ResetDisplay()
    {
        totalResult = 0;
        diceResultText?.SetText("");
    }
}
