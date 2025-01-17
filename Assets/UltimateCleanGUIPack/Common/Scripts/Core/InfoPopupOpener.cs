// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// Utility component to open an info popup. See the associated InfoPopup script.
    /// </summary>
    public class InfoPopupOpener : PopupOpener
    {
        public Sprite iconSprite;
        public string iconText;

        public override void OpenPopup()
        {
            base.OpenPopup();
            m_popup.GetComponent<InfoPopup>().SetInfo(iconSprite, iconText);
        }
    }
}
