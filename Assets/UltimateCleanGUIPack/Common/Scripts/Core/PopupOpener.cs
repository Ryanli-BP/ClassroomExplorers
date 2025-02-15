using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    public class PopupOpener : MonoBehaviour
    {
        public GameObject popupPrefab;
        protected Canvas m_canvas;
        protected GameObject m_popup;

        protected void Start()
        {
            m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }

        public virtual void OpenPopup()
        {
            int playerID = GetPlayerIDFromName(); // Extract Player ID
            Debug.Log($"Player ID on Opener: {playerID}");

            m_popup = Instantiate(popupPrefab, m_canvas.transform, false);
            m_popup.SetActive(true);
            m_popup.transform.localScale = Vector3.zero;

            // Find the RawImage in Mask -> Avatar
            RawImage avatarImage = transform.Find("Mask/Avatar")?.GetComponent<RawImage>();

            // Pass player ID and avatar texture to the popup
            Popup popupScript = m_popup.GetComponent<Popup>();
            if (popupScript != null)
            {
                popupScript.SetPlayerInfo(playerID);

                if (avatarImage != null)
                {
                    popupScript.SetAvatarImage(avatarImage.texture);
                }
                else
                {
                    Debug.LogError("Avatar RawImage not found in hierarchy.");
                }
            }
            else
            {
                Debug.LogError("Popup script not found on instantiated popup prefab.");
            }

            m_popup.GetComponent<Popup>().Open();
        }

        private int GetPlayerIDFromName()
        {
            string objectName = gameObject.name; // Example: "Avatar 1"
            string[] nameParts = objectName.Split(' '); 
            if (nameParts.Length > 1 && int.TryParse(nameParts[1], out int id))
            {
                return id - 1; // Extracted Player ID
            }
            return -1; // Invalid ID if parsing fails
        }
    }
}
