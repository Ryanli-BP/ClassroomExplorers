// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// The messages popup used in the demos.
    /// </summary>
    public class MessagesPopup : MonoBehaviour
    {
        private Animator animator;

        private static readonly int MoveLeft = Animator.StringToHash("MoveLeft");
        private static readonly int MoveRight = Animator.StringToHash("MoveRight");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void OpenDetailedView()
        {
            animator.SetTrigger(MoveLeft);
        }

        public void CloseDetailedView()
        {
            animator.SetTrigger(MoveRight);
        }
    }
}
