using UnityEngine;
using System.Collections;

public class PlayerFightAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float attackSpeed = 2f;
    [SerializeField] private float jumpHeight = 2f;
    
    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;

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
        Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.6f); // Strike position
        float journey = 0f;

        // Forward leap
        while (journey <= 1f)
        {
            journey += Time.deltaTime * attackSpeed;
            
            // Calculate forward movement
            float x = Mathf.Lerp(startPosition.x, midPoint.x, journey);
            
            // Parabolic jump with higher arc
            float y = startPosition.y + (-4f * jumpHeight * journey * journey + 4f * jumpHeight * journey);
            
            // Keep Z position constant
            float z = startPosition.z;
            
            transform.position = new Vector3(x, y, z);
            
            // Rotate for strike at peak
            if (journey > 0.4f && journey < 0.6f)
            {
                transform.Rotate(0, 0, -360 * Time.deltaTime); // Add a spin
            }
            
            yield return null;
        }

        // Return to start position
        journey = 0f;
        Vector3 attackPosition = transform.position;
        
        while (journey <= 1f)
        {
            journey += Time.deltaTime * attackSpeed;
            
            float x = Mathf.Lerp(attackPosition.x, startPosition.x, journey);
            float y = attackPosition.y + (-4f * jumpHeight * journey * journey + 4f * jumpHeight * journey);
            float z = startPosition.z;
            
            transform.position = new Vector3(x, y, z);
            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        isAttacking = false;
    }
}