using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public class Dice : MonoBehaviour
{
    public Transform[] dicefaces;
    public Rigidbody rb;

    public int diceNum = -1;
    public bool rollfin;
    public bool delayfin;

    public static UnityAction<int, int> OnDiceResult;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!delayfin) return;

        if (!rollfin && rb.linearVelocity.sqrMagnitude == 0f)
        {
            rollfin = true;
            GetRes();
        }
    }

    [ContextMenu(itemName: "Dice Result")]
    private int GetRes()
    {
        if (dicefaces == null || dicefaces.Length == 0) return -1;

        var topFace = 0;
        var highestY = dicefaces[0].position.y;

        for (int i = 1; i < dicefaces.Length; i++)
        {
            if (dicefaces[i].position.y > highestY)
            {
                highestY = dicefaces[i].position.y;
                topFace = i;
            }
        }

        int diceResult = topFace + 1; 
        Debug.Log($"Dice result: {diceResult}");

        // Invoke the event with dice number and the result.
        OnDiceResult?.Invoke(diceNum, diceResult);

        return diceResult;
    }

    public void RollDice(float throwForce, float rollForce, int i)
    {
        diceNum = i;  

        var randomVar = Random.Range(-1f, 1f);
        rb.AddForce(transform.forward * (throwForce + randomVar), ForceMode.Impulse);

        var randTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        );
        rb.AddTorque(randTorque * (rollForce + randomVar), ForceMode.Impulse);

        DelayResult();
    }

    private async void DelayResult()
    {
        await Task.Delay(1000);  // Wait before checking the result
        delayfin = true;
    }
}
