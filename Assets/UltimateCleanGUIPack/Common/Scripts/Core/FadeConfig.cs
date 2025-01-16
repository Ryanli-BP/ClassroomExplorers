// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// This component contains the configuration settings of a FadeButton:
    ///     - fadeTime: the fading time in seconds.
    ///     - onHoverAlpha: the target alpha value when hovering over the button.
    ///     - onClickAlpha: the target alpha value when clicking the button.
    /// </summary>
    public class FadeConfig : MonoBehaviour
    {
        public float fadeTime = 0.2f;
        public float onHoverAlpha = 0.6f;
        public float onClickAlpha = 0.7f;
    }
}