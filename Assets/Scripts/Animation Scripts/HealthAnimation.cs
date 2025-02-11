namespace UltimateClean
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class HealthAnimation : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private TextMeshProUGUI text;
        private Image image;
        private SlicedFilledImage slicedImage;
        private int lastPercentage;

        private void Awake()
        {
            image = GetComponent<Image>();
            slicedImage = GetComponent<SlicedFilledImage>();
        }

        /// <summary>
        /// Animate health bar fill from current fill to newHealth/maxHealth.
        /// Call this method whenever the player's health changes (LoseHealth or Revives).
        /// </summary>
        public void AnimateHealth(int newHealth, int maxHealth)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateHealthRoutine(newHealth, maxHealth));
        }

        private IEnumerator AnimateHealthRoutine(int newHealth, int maxHealth) 
        {
            float startFill = GetCurrentFill();
            float endFill = (float)newHealth / maxHealth;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float ratio = Mathf.Lerp(startFill, endFill, elapsed / duration);

                UpdateFill(ratio);
                UpdateText(Mathf.RoundToInt(ratio * maxHealth), maxHealth);

                yield return null;
            }

            UpdateFill(endFill);
            UpdateText(newHealth, maxHealth);
        }

        private float GetCurrentFill()
        {
            if (image != null)
                return image.fillAmount;
            else if (slicedImage != null)
                return slicedImage.fillAmount;
            return 0f;
        }

        private void UpdateFill(float ratio)
        {
            if (image != null)
                image.fillAmount = ratio;
            else if (slicedImage != null)
                slicedImage.fillAmount = ratio;
        }

        private void UpdateText(int currentHealth, int maxHealth)
        {
            if (text != null)
            {
                text.text = $"{currentHealth}/{maxHealth}";
            }
        }
    }
}