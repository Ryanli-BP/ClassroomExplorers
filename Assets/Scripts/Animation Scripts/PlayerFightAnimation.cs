using UnityEngine;
using System.Collections;

public class PlayerFightAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float attackSpeed = 2.5f;
    private float baseJumpHeight = 3f;
    private float scaledJumpHeight;
    
    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    private void Start()
    {
        scaledJumpHeight = baseJumpHeight * ARBoardPlacement.worldScale;
    }

    public IEnumerator PerformAttack(Vector3 targetPosition)
    {
        if (!isAttacking)
        {
            yield return StartCoroutine(LeapStrikeAnimation(targetPosition));
        }
    }

        private IEnumerator LeapStrikeAnimation(Vector3 targetPosition)
    {
        isAttacking = true;
        Vector3 startPosition = transform.position;
        Vector3 strikePosition = new Vector3(
            targetPosition.x + ((startPosition.x - targetPosition.x) * 0.1f), 
            targetPosition.y, 
            targetPosition.z
        );
        
        float journey = 0f;
        Quaternion originalRotation = transform.rotation;

        // Forward leap
        while (journey <= 1f)
        {
            journey += Time.deltaTime * attackSpeed;
            
            float x = Mathf.Lerp(startPosition.x, strikePosition.x, journey);
            float heightCurve = Mathf.Sin(journey * Mathf.PI);
            float y = startPosition.y + (heightCurve * scaledJumpHeight);
            float z = startPosition.z;
            
            transform.position = new Vector3(x, y, z);
            
            // Forward tilt during leap
            float tiltAngle = Mathf.Lerp(0, -30, journey);
            transform.rotation = originalRotation * Quaternion.Euler(tiltAngle, 0, 0);
            
            // Attack spin at apex
            if (journey > 0.45f && journey < 0.55f)
            {
                transform.Rotate(0, 0, -720 * Time.deltaTime);
            }
            
            yield return null;
        }

        // Return to start position
        journey = 0f;
        Vector3 returnStartPos = transform.position;
        Quaternion attackRotation = transform.rotation;
        
        while (journey <= 1f)
        {
            journey += Time.deltaTime * attackSpeed;
            
            float x = Mathf.Lerp(returnStartPos.x, startPosition.x, journey);
            float heightCurve = Mathf.Sin(journey * Mathf.PI);
            float y = startPosition.y + (heightCurve * scaledJumpHeight * 0.5f);
            float z = startPosition.z;
            
            transform.position = new Vector3(x, y, z);
            
            // Return tilt to original
            float tiltAngle = Mathf.Lerp(-30, 0, journey);
            transform.rotation = originalRotation * Quaternion.Euler(tiltAngle, 0, 0);
            
            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = originalRotation;
        isAttacking = false;
    }
}