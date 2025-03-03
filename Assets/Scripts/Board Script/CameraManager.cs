using UnityEngine;
using UnityEngine.InputSystem; // Add this for new Input System

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    [SerializeField] private Transform arenaAnchor;
    [SerializeField] private Transform boardAnchor;
    [SerializeField] private Transform combatPlayerSpot;
    [SerializeField] private Transform combatOpponentSpot;
    [SerializeField] private Transform combatDiceSpot;
    [SerializeField] private Transform combatCameraSpot;
    [SerializeField] private Transform boardCameraSpot;
    [SerializeField] private Transform boardDiceSpot;

    [SerializeField] private Transform arCamera;

    private Transform currentFollowTarget;
    private bool shouldFollowTarget = false;
    private float cameraHeight = 2.5f; // Height offset from target
    
    // Zoom parameters
    private float minZoomDistance = 4.0f;
    private float maxZoomDistance = 20.0f;
    private float zoomSpeed = 10.0f; // Increased from 1.0f to compensate for different input values
    private float cameraDistance; // Will be set in Awake
    
    // Transition parameters
    private float cameraTransitionSpeed = 2.0f; // Reduced from 5.0f for slower transitions
    
    // Flag to track if we're in combat
    private bool isInCombat = false;

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

        // Set default zoom to 70% of max distance
        cameraDistance = minZoomDistance + (maxZoomDistance - minZoomDistance) * 0.7f;

        if (PlatformUtils.IsRunningOnPC())
        {
            arCamera.transform.position = boardCameraSpot.transform.position;
            arCamera.transform.rotation = Quaternion.Euler(50, 0, 0);
            shouldFollowTarget = true;
        }
        
        // Subscribe to game state changes
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }
    
    private void HandleGameStateChanged(GameState state)
    {
        // Set the combat flag based on game state
        isInCombat = (state == GameState.PlayerCombat);
    }

    private void Update()
    {
        if (PlatformUtils.IsRunningOnPC() && !isInCombat)
        {
            // Handle zoom with mouse wheel
            if (shouldFollowTarget && currentFollowTarget != null)
            {
                HandleZoom();
                FollowCurrentEntity();
            }
        }
    }

    private void HandleZoom()
    {
        // Get mouse wheel input using new Input System
        float scrollInput = Mouse.current.scroll.ReadValue().y / 120.0f; // Normalize input value
        
        if (scrollInput != 0)
        {
            // Adjust camera distance based on scroll direction
            cameraDistance = Mathf.Clamp(cameraDistance - scrollInput * zoomSpeed, minZoomDistance, maxZoomDistance);
        }
    }

    private void FollowCurrentEntity()
    {
        if (currentFollowTarget == null || isInCombat) return;

        // Calculate position that maintains a 35-degree viewing angle looking down at the target
        Vector3 targetPosition = currentFollowTarget.position;
        
        // Calculate camera position for a 35-degree viewing angle
        float angleInRadians = 35f * Mathf.Deg2Rad;
        float horizontalDistance = cameraDistance * Mathf.Cos(angleInRadians);
        float verticalOffset = cameraHeight + cameraDistance * Mathf.Sin(angleInRadians);
        
        // Position the camera behind the target at the calculated distance
        Vector3 cameraPosition = new Vector3(
            targetPosition.x, 
            targetPosition.y + verticalOffset, 
            targetPosition.z - horizontalDistance
        );
        
        // Smoothly move the camera to the new position with a slower transition
        arCamera.position = Vector3.Lerp(arCamera.position, cameraPosition, Time.deltaTime * cameraTransitionSpeed);
        
        // Make the camera look at the target instead of using a fixed rotation
        Vector3 lookDirection = targetPosition - arCamera.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            arCamera.rotation = Quaternion.Slerp(arCamera.rotation, rotation, Time.deltaTime * cameraTransitionSpeed);
        }
    }

    public void SetFollowTarget(Transform target)
    {
        currentFollowTarget = target;
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