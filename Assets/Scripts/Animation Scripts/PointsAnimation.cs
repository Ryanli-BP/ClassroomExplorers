namespace UltimateClean
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class PointsAnimation : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private TextMeshProUGUI text;
        private Image image;
        private SlicedFilledImage slicedImage;

        private void Awake()
        {
            image = GetComponent<Image>();
            slicedImage = GetComponent<SlicedFilledImage>();
            
            // Set initial fill amount to 0
            if (image != null)
                image.fillAmount = 0f;
            else if (slicedImage != null) 
                slicedImage.fillAmount = 0f;
                
            if (text != null)
                text.text = "0/0";
        }

        /// <summary>
        /// Animate the points bar fill from current fill to (currentPoints / levelUpPoints).
        /// The bar is clamped at 100% even if currentPoints > levelUpPoints.
        /// </summary>
        public IEnumerator AnimatePoints(int currentPoints, int levelUpPoints)
        {
            StopAllCoroutines();
            yield return StartCoroutine(AnimatePointsRoutine(currentPoints, levelUpPoints));
        }

        private IEnumerator AnimatePointsRoutine(int currentPoints, int levelUpPoints)
        {
            float startFill = GetCurrentFill();
            float targetRatio = Mathf.Clamp01((float)currentPoints / levelUpPoints);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float ratio = Mathf.Lerp(startFill, targetRatio, elapsed / duration);

                UpdateBar(ratio);
                UpdateText(currentPoints, levelUpPoints);

                yield return null;
            }

            UpdateBar(targetRatio);
            UpdateText(currentPoints, levelUpPoints);
        }

        private float GetCurrentFill()
        {
            if (image != null)
                return image.fillAmount;
            else if (slicedImage != null)
                return slicedImage.fillAmount;
            return 0f;
        }

        private void UpdateBar(float ratio)
        {
            if (image != null)
                image.fillAmount = ratio;
            else if (slicedImage != null)
                slicedImage.fillAmount = ratio;
        }

        private void UpdateText(int currentPoints, int levelUpPoints)
        {
            if (text != null)
            {
                text.text = $"{currentPoints}/{levelUpPoints}";
            }
        }
    }
}