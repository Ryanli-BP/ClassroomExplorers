// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateClean
{
    /// <summary>
    /// Basic tooltip component used in the kit.
    /// </summary>
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject tooltip;

        public float fadeTime = 0.1f;

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (tooltip != null)
            {
                StartCoroutine(Utils.FadeIn(tooltip.GetComponent<CanvasGroup>(), 1.0f, fadeTime));
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (tooltip != null)
            {
                StartCoroutine(Utils.FadeOut(tooltip.GetComponent<CanvasGroup>(), 0.0f, fadeTime));
            }
        }
    }
}
