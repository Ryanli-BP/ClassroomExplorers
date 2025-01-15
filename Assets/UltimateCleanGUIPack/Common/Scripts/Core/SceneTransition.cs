// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// This component is responsible for loading the next scene in a transition
    /// (note the core of this work is performed by the Transition component).
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public string scene = "<Insert scene name>";
        public float duration = 1.0f;
        public Color color = Color.black;

        public void PerformTransition()
        {
            Transition.LoadLevel(scene, duration, color);
        }
    }
}
