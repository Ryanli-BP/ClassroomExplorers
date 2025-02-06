using UnityEngine;

public class Boss : MonoBehaviour
{
    private Tile currentTile;
    public Tile CurrentTile
    {
        get { return currentTile; }
        set { currentTile = value; }
    }
    public int Health { get; set; }
    public Status Status { get; set; }

    private void Start()
    {
        Health = 100;
        Status = Status.Alive;
    }
}