// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// Utility class for swapping the sprite of a UI Image between two
    /// predefined values.
    /// </summary>
    public class SpriteSwapper : MonoBehaviour
    {
        public Sprite enabledSprite;
        public Sprite disabledSprite;

        private bool m_swapped = true;

        private Image m_image;

        public void Awake()
        {
            m_image = GetComponent<Image>();
        }

        public void SwapSprite()
        {
            if (m_swapped)
            {
                m_swapped = false;
                m_image.sprite = disabledSprite;
            }
            else
            {
                m_swapped = true;
                m_image.sprite = enabledSprite;
            }
        }
    }
}
