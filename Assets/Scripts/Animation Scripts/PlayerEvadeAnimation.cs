using UnityEngine;
using System.Collections;

public class PlayerEvadeAnimation : MonoBehaviour
{
    [SerializeField] private float evadeSpeed = 3f;
    [SerializeField] private float sideStepDistance = 2f * ARBoardPlacement.worldScale;
    
    private bool isEvading = false;
    public bool IsEvading => isEvading;

    public IEnumerator PerformEvade(bool evadeRight = true)
    {
        if (!isEvading)
        {
            yield return StartCoroutine(SideStepAnimation(evadeRight));
        }
    }

    private IEnumerator SideStepAnimation(bool evadeRight)
    {
        isEvading = true;
        Vector3 startPosition = transform.position;
        Vector3 evadeDirection = evadeRight ? transform.right : -transform.right;
        Vector3 evadePosition = startPosition + (evadeDirection * sideStepDistance);
        
        // Sidestep out
        float journey = 0f;
        while (journey <= 1f)
        {
            journey += Time.deltaTime * evadeSpeed;
            transform.position = Vector3.Lerp(startPosition, evadePosition, journey);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // Brief pause

        // Return to position
        journey = 0f;
        while (journey <= 1f)
        {
            journey += Time.deltaTime * evadeSpeed;
            transform.position = Vector3.Lerp(evadePosition, startPosition, journey);
            yield return null;
        }

        transform.position = startPosition;
        isEvading = false;
    }
}