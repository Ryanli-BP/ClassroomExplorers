using UnityEngine;
using System.Collections;

public class PlayerMovementAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float hopHeight = 0.5f;
    
    private bool isMoving = false;
    
    public bool IsMoving => isMoving;

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
            
            // Calculate horizontal movement
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, journey);
            float z = Mathf.Lerp(startPosition.z, targetPosition.z, journey);
            
            // Calculate vertical hop using sine wave
            float y = startPosition.y + Mathf.Sin(journey * Mathf.PI) * hopHeight;
            
            // Apply position
            transform.position = new Vector3(x, y, z);
            
            yield return null;
        }
        
        // Ensure final position is exact
        transform.position = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);
        isMoving = false;
    }
}