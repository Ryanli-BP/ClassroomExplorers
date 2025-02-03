using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ARBoardPlacement : MonoBehaviour
{
    [SerializeField] GameObject boardRoot;
    public static float worldScale; 
    [SerializeField] Vector3 desiredScale = new Vector3(worldScale, worldScale, worldScale);
    private ARRaycastManager ARRaycastManager;
    private ARPlaneManager ARPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool hasPlacedBoard = false;
    public static Quaternion boardRotation { get; private set; }

    private void Awake()
    {
        ARRaycastManager = GetComponent<ARRaycastManager>();
        ARPlaneManager = GetComponent<ARPlaneManager>();
        if (PlatformUtils.IsRunningOnPC())
        {
            worldScale = 1f; 
        }
        else
        {
            worldScale = 0.05f; 
        }
        
        // Ensure board starts inactive
        boardRoot.SetActive(false);
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.EnhancedTouchSupport.Disable();
    }
    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0 || hasPlacedBoard) return; 

        if (ARRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;
                
                // Set position and scale
                boardRoot.transform.position = pose.position;
                boardRoot.transform.localScale = desiredScale;
                
                
                if (ARPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                {
                    Vector3 cameraPosition = Camera.main.transform.position;
                    Vector3 direction = cameraPosition - pose.position;
                    direction.y = 0;
                    
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(-direction);
                        boardRoot.transform.rotation = targetRotation;
                        boardRotation = targetRotation; // Store the rotation
                    }
                }
                
                // Activate the board
                boardRoot.SetActive(true);
                hasPlacedBoard = true;
                GameInitializer.Instance.InitializeGame();

                // Hide planes
                ARPlaneManager.planePrefab.SetActive(false);
                foreach (var plane in ARPlaneManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }
                ARPlaneManager.enabled = false;
                break;
            }
        }
    }

    public void ResetPlacement()
    {
        hasPlacedBoard = false;
    }

}