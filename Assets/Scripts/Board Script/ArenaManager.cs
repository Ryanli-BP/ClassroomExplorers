using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instance { get; private set; }
    
    [SerializeField] private Transform arenaAnchor;
    [SerializeField] private Transform boardAnchor;
    [SerializeField] private Transform combatPlayerSpot;
    [SerializeField] private Transform combatOpponentSpot;
    [SerializeField] private Transform combatDiceSpot;
    [SerializeField] private Transform combatCameraSpot;
    [SerializeField] private Transform boardCameraSpot;
    [SerializeField] private Transform boardDiceSpot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 GetCombatPlayerPosition() => combatPlayerSpot.position;
    public Vector3 GetCombatOpponentPosition() => combatOpponentSpot.position;
    public Vector3 GetCombatDicePosition() => combatDiceSpot.position;
    public Vector3 GetboardDicePosition() => boardDiceSpot.position;
    public Vector3 GetCombatCameraPosition() => combatCameraSpot.position;
    public Transform GetBoardAnchor() => boardAnchor;
    public Transform GetArenaAnchor() => arenaAnchor;
    public Vector3 GetBoardCameraPosition() => boardCameraSpot.position;
}
