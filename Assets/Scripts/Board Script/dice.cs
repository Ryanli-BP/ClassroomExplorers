using UnityEngine;
using System.Threading.Tasks;

public class Dice : MonoBehaviour
{
    public Transform[] dicefaces;
    public Rigidbody rb;

    public int diceNum = -1;
    public bool rollfin;
    public bool delayfin;

    private int previousTopFace = -1;
    private float stableTime = 0f;
    private const float STABILITY_THRESHOLD = 3f;
    private const float BASE_VELOCITY_THRESHOLD = 0.01f;
    private const float BASE_ANGULAR_VELOCITY_THRESHOLD = 0.01f;
    private float VELOCITY_THRESHOLD => BASE_VELOCITY_THRESHOLD * ARBoardPlacement.worldScale;
    private float ANGULAR_VELOCITY_THRESHOLD => BASE_ANGULAR_VELOCITY_THRESHOLD * ARBoardPlacement.worldScale;
    private bool isTrackingStability = false;
    private const float MAX_ROLL_TIME = 5f;
    private float rollTimer = 0f;

    public delegate void DiceFinishedRolling();
    public event DiceFinishedRolling OnDiceFinishedRolling;

    private void Update()
    {
        if (!delayfin || rollfin) return;

        rollTimer += Time.deltaTime;
        int currentTopFace = GetCurrentTopFace();

        // Failsafe: Force random result after MAX_ROLL_TIME
        if (rollTimer >= MAX_ROLL_TIME)
        {
            rollfin = true;
            int randomResult = Random.Range(1, 7);
            DiceManager.Instance.HandleDiceResult(randomResult);
            OnDiceFinishedRolling?.Invoke();
            return;
        }

        // Check if dice has completely stopped
        if (rb.linearVelocity.magnitude < VELOCITY_THRESHOLD && 
            rb.angularVelocity.magnitude < ANGULAR_VELOCITY_THRESHOLD)
        {
            rollfin = true;
            DiceManager.Instance.HandleDiceResult(currentTopFace + 1);
            OnDiceFinishedRolling?.Invoke();
            return;
        }

        // Stability check for wobbling dice
        if (currentTopFace != previousTopFace)
        {
            stableTime = 0f;
            previousTopFace = currentTopFace;
            isTrackingStability = true;
        }
        else if (isTrackingStability)
        {
            stableTime += Time.deltaTime;
            
            if (stableTime >= STABILITY_THRESHOLD)
            {
                rollfin = true;
                DiceManager.Instance.HandleDiceResult(currentTopFace + 1);
                OnDiceFinishedRolling?.Invoke();
                isTrackingStability = false;
            }
        }
    }

    private int GetCurrentTopFace()
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

        return topFace;
    }

    public void RollDice(float throwForce, float rollForce, int i)
    {
        diceNum = i;
        rollfin = false;
        delayfin = false;
        isTrackingStability = false;
        stableTime = 0f;
        previousTopFace = -1;

        var randomVar = Random.Range(-1f, 1f);
        rb.linearDamping = 0.1f * ARBoardPlacement.worldScale;
        rb.angularDamping = 0.1f * ARBoardPlacement.worldScale;
        
        // Scale forces by worldScale
        float scaledThrowForce = throwForce * ARBoardPlacement.worldScale;
        float scaledRollForce = rollForce * ARBoardPlacement.worldScale;
        
        rb.AddForce(transform.forward * (scaledThrowForce + randomVar), ForceMode.Impulse);

        var randX = Random.Range(-1f, 1f);
        var randY = Random.Range(-1f, 1f);
        var randZ = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(randX, randY, randZ) * (scaledRollForce + randomVar), ForceMode.Impulse);

        if (DiceManager.Instance.GetNumDice() == 1)
        {
            var initialRandomForce = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * scaledThrowForce;
            rb.AddForce(initialRandomForce, ForceMode.Impulse);

            var initialRandomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * scaledRollForce;
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
