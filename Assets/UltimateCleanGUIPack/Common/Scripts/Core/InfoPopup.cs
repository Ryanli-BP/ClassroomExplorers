// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// The main component of a popup containing an associated image and text. This is
    /// uses in the demo's equipment popups.
    /// </summary>
    public class InfoPopup : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI text;

        public void SetInfo(Sprite iconSprite, string iconText)
        {
            image.sprite = iconSprite;
            text.text = iconText;
        }
    }
}
