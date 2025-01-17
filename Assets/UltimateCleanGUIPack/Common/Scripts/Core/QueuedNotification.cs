// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// This type is used to store the information of a queued notification.
    /// </summary>
    public class QueuedNotification
    {
        public GameObject Prefab;
        public Canvas Canvas;
        public NotificationType Type;
        public NotificationPositionType Position;
        public float Duration;
        public string Title;
        public string Message;
    }
}