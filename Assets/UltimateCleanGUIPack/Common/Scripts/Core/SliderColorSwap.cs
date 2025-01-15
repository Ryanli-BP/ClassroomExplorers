// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// This utility component makes it possible to swap the slider's color when
    /// its value goes from/to zero.
    /// </summary>
    [ExecuteInEditMode]
    public class SliderColorSwap : MonoBehaviour
    {
        public Color EnabledColor;
        public Color DisabledColor;

        public Image Handle;

        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
        }

        private void Start()
        {
            OnValueChanged(slider.value);
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void OnValueChanged(float value)
        {
            Handle.color = value == 0 ? DisabledColor : EnabledColor;
        }
    }
}