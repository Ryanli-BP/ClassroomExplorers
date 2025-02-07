using UnityEngine;

public class Boss : MonoBehaviour
{
    public int Health { get; set; }
    public Status Status { get; set; }
    public BossMovement Movement { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<BossMovement>();
    }

    private void Start()
    {
        Health = 100;
        Status = Status.Alive;
    }
}