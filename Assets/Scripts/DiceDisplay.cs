using UnityEngine;
using TMPro;

public class DiceDisplay : MonoBehaviour
{
    public TextMeshProUGUI diceResultText;

    private void OnEnable()
    {
        Dice.OnDiceResult += HandleDiceResult;
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= HandleDiceResult;
    }

    private void HandleDiceResult(int diceNum, int diceResult)
    {
       diceResultText.text = $"Dice {diceNum} Result: {diceResult}";

    }
}
