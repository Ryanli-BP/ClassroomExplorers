// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// Custom toggle component that has an associated label.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleWithLabel : MonoBehaviour
    {
    #pragma warning disable 649
        [SerializeField]
        private GameObject onLabel;
        [SerializeField]
        private GameObject offLabel;
    #pragma warning restore 649

        private Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void OnValueChanged(bool value)
        {
            onLabel.SetActive(value);
            offLabel.SetActive(!value);
        }
    }
}