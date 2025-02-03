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

    [SerializeField] private Transform arCamera;

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

        if (PlatformUtils.IsRunningOnPC())
        {

            arCamera.transform.position = boardCameraSpot.transform.position;
            arCamera.transform.rotation = Quaternion.Euler(50, 0, 0);
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
