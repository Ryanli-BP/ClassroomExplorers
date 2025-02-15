using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

namespace UltimateClean
{
    public class Popup : MonoBehaviour
    {
        public RawImage avatarDisplay;
        public GameObject healthBar;
        public TextMeshProUGUI pointsBar;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI AttackBonus;
        public TextMeshProUGUI DefenseBonus;
        public TextMeshProUGUI DodgeBonus;
        public GameObject doublePointsBuff;
        public GameObject triplePointsBuff;
        public GameObject ExtraDiceBuff;


        public Color backgroundColor = new Color(10.0f / 255.0f, 10.0f / 255.0f, 0.6f);
        public float destroyTime = 0.5f;

        private GameObject m_background;
        private int playerID;
        private Player player;

        public TextMeshProUGUI playerInfoText; 

        public void Open()
        {
            Debug.Log($"Player ID on infoPage: {playerID}");

            if (PlayerManager.Instance != null)
            {
                player = PlayerManager.Instance.GetPlayerList()[playerID];

                if (player != null)
                {
                    Debug.Log($"Player Points: {player.Points}");
                    DisplayPlayerInfo();
                }
                else
                {
                    Debug.LogError($"Player with ID {playerID} not found!");
                }
            }
            else
            {
                Debug.LogError("PlayerManager is not initialized or playerID is invalid!");
            }

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

        public void SetPlayerInfo(int playerID)
        {
            this.playerID = playerID;

            if (PlayerManager.Instance != null)
            {
                player = PlayerManager.Instance.GetPlayerList()[playerID];
                DisplayPlayerInfo();
            }
            else
            {
                Debug.LogError("PlayerManager instance not found or invalid playerID!");
            }
        }

        public void SetAvatarImage(Texture texture){
            if(avatarDisplay != null){
                avatarDisplay.texture = texture;
            }
            else{
                Debug.Log("Avatar display RawImage not assigned in the pop up prefab.");
            }
        }

        private void DisplayPlayerInfo()
        {
            if (healthBar != null && pointsBar != null && levelText != null && player != null)
            {

                var activeBuffs = player.PlayerBuffs.GetActiveBuffs(); // Get active buffs list from the player

                // Check and display DoublePoints and TriplePoints buffs
                if (activeBuffs.Any(b => b.Type == BuffType.DoublePoints))
                {
                    doublePointsBuff.SetActive(true);  // Show the DoublePoints buff
                }
                else
                {
                    doublePointsBuff.SetActive(false);  // Hide the DoublePoints buff
                }

                if (activeBuffs.Any(b => b.Type == BuffType.TriplePoints))
                {
                    triplePointsBuff.SetActive(true);  // Show the TriplePoints buff
                }
                else
                {
                    triplePointsBuff.SetActive(false);  // Hide the TriplePoints buff
                }

                var healthAnimation = healthBar.GetComponent<HealthAnimation>();
                if (healthAnimation != null)
                {
                    healthAnimation.AnimateHealth(player.Health, Player.MAX_HEALTH);
                }
                else
                {
                    Debug.LogError("HealthAnimation component missing on healthBar GameObject.");
                }

                pointsBar.text = $"{player.Points}";
                levelText.text = $"Level: {player.Level}";

                // Display attack, defense, and evade bonuses
                int attackBonus = activeBuffs.Where(b => b.Type == BuffType.AttackUp).Sum(b => b.Value);
                int defenseBonus = activeBuffs.Where(b => b.Type == BuffType.DefenseUp).Sum(b => b.Value);
                int evadeBonus = activeBuffs.Where(b => b.Type == BuffType.EvadeUp).Sum(b => b.Value);
                int ExtraDice = activeBuffs.Where(b => b.Type == BuffType.ExtraDice).Sum(b => b.Value);

                if(ExtraDice > 0){
                    ExtraDiceBuff.SetActive(true);
                }
                else{
                    ExtraDiceBuff.SetActive(false);
                }
                AttackBonus.text = $"{attackBonus}";
                DefenseBonus.text = $"{defenseBonus}";
                DodgeBonus.text = $"{evadeBonus}";

            }
            else
            {
                Debug.LogWarning("One or more UI elements are not assigned or player is null!");
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
