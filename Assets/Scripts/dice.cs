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

        Debug.Log($"dice result {topFace + 1}");

        OnDiceResult?.Invoke(diceNum, topFace + 1);

        return topFace + 1;
    }

    public void RollDice(float throwForce, float rollForce, int i)
    {
        int diceIndex = i;
        var randomVar = Random.Range(-1f, 1f);
        rb.AddForce(transform.forward * (throwForce + randomVar), ForceMode.Impulse);

        var randY = Random.Range(-1f, 1f);
        var randX = Random.Range(-1f, 1f);
        var randZ = Random.Range(-1f, 1f);

        rb.AddTorque(new Vector3(randX, randY, randZ) * (rollForce + randomVar), ForceMode.Impulse);

        DelayResult();
    }

    private async void DelayResult()
    {
        await Task.Delay(1000);
        delayfin = true;
    }
}
