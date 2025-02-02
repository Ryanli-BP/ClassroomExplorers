using UnityEngine;
using System.Collections;

public class PlayerMovementAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveSpeed = 5f;
    private float baseHopHeight = 0.5f;
    private float scaledHopHeight;
    
    private bool isMoving = false;
    
    public bool IsMoving => isMoving;

    private void Start()
    {
        scaledHopHeight = baseHopHeight * ARBoardPlacement.worldScale;
    }

    public IEnumerator HopTo(Vector3 targetPosition)
    {
        if (!isMoving)
        {
            yield return StartCoroutine(HopAnimation(targetPosition));
        }
    }

    private IEnumerator HopAnimation(Vector3 targetPosition)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float journey = 0f;
        
        while (journey <= 1f)
        {
            journey += Time.deltaTime * moveSpeed;
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, journey);
            float z = Mathf.Lerp(startPosition.z, targetPosition.z, journey);
            float y = startPosition.y + (Mathf.Sin(journey * Mathf.PI) * scaledHopHeight);
            transform.position = new Vector3(x, y, z);
            yield return null;
        }
        
        // Ensure final position is exact
        transform.position = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);
        isMoving = false;
    }
}