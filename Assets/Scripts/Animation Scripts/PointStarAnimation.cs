using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PointStarAnimation : MonoBehaviour
{
    [SerializeField] private float starMoveDuration = 1f;

    public void AnimatePointStar(GameObject starImage, RectTransform pointBar)
    {
        starImage.SetActive(true); // Ensure the star image is active
        RectTransform starRectTransform = starImage.GetComponent<RectTransform>();
        starRectTransform.SetAsLastSibling(); // Move the star to the front
        starRectTransform.anchoredPosition = Vector2.zero; // Reset position to center of UI
        StartCoroutine(MoveStar(starRectTransform, pointBar));
    }

    private IEnumerator MoveStar(RectTransform starRectTransform, RectTransform pointBar)
    {
        Canvas canvas = starRectTransform.GetComponentInParent<Canvas>();

        Vector2 endPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, pointBar.position),
            canvas.worldCamera,
            out endPos
        );

        Vector2 startPos = starRectTransform.anchoredPosition;

        Debug.Log($"Start Position: {startPos}");
        Debug.Log($"End Position: {endPos}");

        float elapsed = 0f;
        while (elapsed < starMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / starMoveDuration;
            t = EaseInOutQuad(t); // Apply easing function
            starRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        starRectTransform.anchoredPosition = endPos; // Ensure it ends exactly at the end position
        starRectTransform.gameObject.SetActive(false); // Hide the star instead of destroying it
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    public void AnimateLosePointStar(GameObject starImage)
    {
        starImage.SetActive(true); // Ensure the star image is active
        RectTransform starRectTransform = starImage.GetComponent<RectTransform>();
        starRectTransform.SetAsLastSibling(); // Move the star to the front
        starRectTransform.anchoredPosition = Vector2.zero; // Reset position to center of UI
        StartCoroutine(MoveStarFall(starRectTransform));
    }

    private IEnumerator MoveStarFall(RectTransform starRectTransform)
    {
        Vector2 startPos = starRectTransform.anchoredPosition;
        float initialVelocityY = 400f;  // Reduced initial velocity
        float gravity = 900f;           // Reduced gravity
        float velocityY = initialVelocityY;
        
        // Smaller horizontal movement
        float horizontalSpeed = Random.Range(-30f, 30f);
        float elapsed = 0f;

        while (starRectTransform.anchoredPosition.y > -600f) // Changed to match canvas size
        {
            elapsed += Time.deltaTime;
            
            velocityY -= gravity * Time.deltaTime;
            
            // Small X movement
            float newX = startPos.x + (horizontalSpeed * Time.deltaTime);
            // Y movement scaled to canvas
            float newY = startPos.y + (velocityY * Time.deltaTime);
            
            starRectTransform.anchoredPosition = new Vector2(newX, newY);
            startPos = starRectTransform.anchoredPosition;

            yield return null;
        }
        
        Destroy(starRectTransform.gameObject);
    }
}