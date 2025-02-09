using UnityEngine;
using System.Collections;

public class BossFightAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float attackSpeed = 4.0f;
    private float baseJumpHeight = 4.5f; // Higher jump for more imposing effect
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
            yield return StartCoroutine(StompAttackAnimation(targetPosition));
        }
    }

    private IEnumerator StompAttackAnimation(Vector3 targetPosition)
    {
        isAttacking = true;
        Vector3 startPosition = transform.position;
        Vector3 aboveTargetPosition = new Vector3(
            targetPosition.x,
            targetPosition.y + scaledJumpHeight,
            targetPosition.z
        );
        float journey = 0f;

        // Arc glide to above target position
        while (journey <= 1f)
        {
            journey += Time.deltaTime * attackSpeed;
            
            // Use quadratic curve for smoother arc that peaks at destination
            float t = journey;
            float heightCurve = 4 * t * (1 - t); // Quadratic curve peaking at t=0.5
            float x = Mathf.Lerp(startPosition.x, aboveTargetPosition.x, t);
            float y = Mathf.Lerp(startPosition.y, aboveTargetPosition.y, t) + (heightCurve * scaledJumpHeight * 0.5f);
            float z = Mathf.Lerp(startPosition.z, aboveTargetPosition.z, t);
            
            transform.position = new Vector3(x, y, z);
            yield return null;
        }

        // Hold at apex
        yield return new WaitForSeconds(0.2f);

        // Quick stomp down
        journey = 0f;
        Vector3 currentPosition = transform.position;
        Vector3 stompPosition = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        
        while (journey <= 1f)
        {
            journey += Time.deltaTime * (attackSpeed * 4f); // Increased from 2.5x to 4x speed
            transform.position = Vector3.Lerp(currentPosition, stompPosition, journey);
            
            if (journey > 0.8f)
            {
                // Increased shake intensity for faster impact
                transform.position += Random.insideUnitSphere * 0.2f * ARBoardPlacement.worldScale;
            }
            
            yield return null;
        }

        // Pause after impact
        yield return new WaitForSeconds(0.15f);

        // Return to start position
        journey = 0f;
        while (journey <= 1f)
        {
            journey += Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(stompPosition, startPosition, journey);
            yield return null;
        }

        transform.position = startPosition;
        isAttacking = false;
    }
}