using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Dicethrower : MonoBehaviour
{
    public Dice DiceToThrow;  
    public int numDice = 1;
    public float throwForce = 5f;
    public float rollForce = 10f;

    private List<GameObject> liveDice = new List<GameObject>();

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollDice();
        }
    }

    private async void RollDice()
    {
        if (DiceToThrow == null) return;

        foreach (var die in liveDice)
        {
            Destroy(die);
        }
        liveDice.Clear();  

        for (int i = 0; i < numDice; i++)
        {
            Dice diceLive = Instantiate(DiceToThrow, transform.position, transform.rotation);
            liveDice.Add(diceLive.gameObject);  
            diceLive.RollDice(throwForce, rollForce, i);
            await Task.Yield();
        }
    }
}
