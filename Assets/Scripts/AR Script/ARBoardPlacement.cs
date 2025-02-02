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
                GameObject board = Instantiate(boardRoot, pose.position, pose.rotation);
                board.transform.localScale = desiredScale;
                hasPlacedBoard = true;

                // Initialize game after board placement
                GameInitializer.Instance.InitializeGame();

                if (ARPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                {
                    Vector3 position = board.transform.position;
                    
                    Vector3 cameraPosition = Camera.main.transform.position;
                    Vector3 direction = cameraPosition - position;
                    
                    Vector3 targetRotationEuler = Quaternion.LookRotation(direction).eulerAngles;
                    Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, board.transform.up.normalized);
                    Quaternion targetRotation = Quaternion.Euler(scaledEuler);
                    board.transform.rotation = board.transform.rotation * targetRotation;
                }

                // Hide all planes after placement
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