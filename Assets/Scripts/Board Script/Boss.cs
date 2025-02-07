using UnityEngine;

public class Boss : Entity
{
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

    public override void LoseHealth(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        Debug.Log($"Boss now has {Health} health.");
        // Add UI update for boss health if needed
    }

    public override void Dies()
    {
        Status = Status.Dead;
        Debug.Log("Boss has been defeated!");
        // Add any boss death logic
    }
}