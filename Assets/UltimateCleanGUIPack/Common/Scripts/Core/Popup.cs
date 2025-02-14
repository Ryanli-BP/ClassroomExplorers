using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro; // If using TextMeshPro

namespace UltimateClean
{
    public class Popup : MonoBehaviour
    {
        public Color backgroundColor = new Color(10.0f / 255.0f, 10.0f / 255.0f, 10.0f / 255.0f, 0.6f);
        public float destroyTime = 0.5f;

        private GameObject m_background;
        private int playerID;

        // Reference to text field inside popup UI (assign in Inspector)
        public TextMeshProUGUI playerInfoText; 

        public void Open()
        {
            Debug.Log($"Player ID on infoPage: {playerID}");
            AddBackground();
        }

        public void Close()
        {
            var animator = GetComponent<Animator>();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
            {
                animator.Play("Close");
            }

            RemoveBackground();
            StartCoroutine(RunPopupDestroy());
        }

        // New Method: Set Player Info
        public void SetPlayerInfo(int playerID)
        {
            this.playerID = playerID; // Assign the player ID here

            // Update the text field in the popup
            if (playerInfoText != null)
            {
                playerInfoText.text = $"Player ID: {playerID}";
            }
            else
            {
                Debug.LogWarning("Player Info Text is not assigned!");
            }
        }

        private IEnumerator RunPopupDestroy()
        {
            yield return new WaitForSeconds(destroyTime);
            Destroy(m_background);
            Destroy(gameObject);
        }

        private void AddBackground()
        {
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, backgroundColor);
            bgTex.Apply();

            m_background = new GameObject("PopupBackground");
            var image = m_background.AddComponent<Image>();
            var rect = new Rect(0, 0, bgTex.width, bgTex.height);
            var sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
            image.material = new Material(image.material);
            image.material.mainTexture = bgTex;
            image.sprite = sprite;
            image.canvasRenderer.SetAlpha(0.0f);
            image.CrossFadeAlpha(1.0f, 0.4f, false);

            var canvas = GameObject.Find("Canvas");
            m_background.transform.localScale = new Vector3(1, 1, 1);
            m_background.GetComponent<RectTransform>().sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;
            m_background.transform.SetParent(canvas.transform, false);
            m_background.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }

        private void RemoveBackground()
        {
            var image = m_background.GetComponent<Image>();
            if (image != null)
            {
                image.CrossFadeAlpha(0.0f, 0.2f, false);
            }
        }
    }
}
