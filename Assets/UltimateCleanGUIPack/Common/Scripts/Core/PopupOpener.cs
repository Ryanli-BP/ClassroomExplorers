// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// This class is responsible for creating and opening a popup of the
    /// given prefab and adding it to the UI canvas of the current scene.
    /// </summary>
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
            m_popup = Instantiate(popupPrefab, m_canvas.transform, false);
            m_popup.SetActive(true);
            m_popup.transform.localScale = Vector3.zero;
            m_popup.GetComponent<Popup>().Open();
        }
    }
}
