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

    public delegate void DiceFinishedRolling(); // Event for dice finished rolling
    public event DiceFinishedRolling OnDiceFinishedRolling;

    private void Update()
    {
        if (!delayfin) return;

        if (!rollfin && rb.linearVelocity.sqrMagnitude == 0f)
        {
            rollfin = true;
            int result = GetRes();
            DiceManager.Instance.HandleDiceResult(result); // Send result to GameManager

            OnDiceFinishedRolling?.Invoke();
        }
    }

    private int GetRes()
    {
        if (dicefaces == null) return -1;

        var topFace = 0;
        var lastY = dicefaces[0].position.y;

        for (int i = 0; i < dicefaces.Length; i++)
        {
            if (dicefaces[i].position.y > lastY)
            {
                lastY = dicefaces[i].position.y;
                topFace = i;
            }
        }

        int diceResult = topFace + 1;
        Debug.Log($"Dice {diceNum} result: {diceResult}");

        return diceResult;
    }

    public void RollDice(float throwForce, float rollForce, int i)
    {
        diceNum = i;
        rollfin = false;
        delayfin = false;

        var randomVar = Random.Range(-1f, 1f);
        rb.linearDamping = 0.1f; // Reduce linear damping
        rb.angularDamping = 0.1f; // Reduce angular damping
        rb.AddForce(transform.forward * (throwForce + randomVar), ForceMode.Impulse);

        var randX = Random.Range(-1f, 1f);
        var randY = Random.Range(-1f, 1f);
        var randZ = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(randX, randY, randZ) * (rollForce + randomVar), ForceMode.Impulse);

        // Add initial random movement if it's the only die
        if (DiceManager.Instance.GetNumDice() == 1)
        {
            var initialRandomForce = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * throwForce;
            rb.AddForce(initialRandomForce, ForceMode.Impulse);

            var initialRandomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * rollForce;
            rb.AddTorque(initialRandomTorque, ForceMode.Impulse);
        }

        DelayResult();
    }

    private async void DelayResult()
    {
        await Task.Delay(1000);
        delayfin = true;
    }
}
