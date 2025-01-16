// Copyright (C) 2019 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// The component used for launching notifications.
    /// </summary>
    public class NotificationLauncher : MonoBehaviour
    {
        public GameObject Prefab;
        public Canvas Canvas;

        public NotificationType Type;
        public NotificationPositionType Position;

        public float Duration;
        public string Title;
        public string Message;

        private NotificationQueue queue;

        private void Start()
        {
            queue = FindFirstObjectByType<NotificationQueue>();
        }

        public void LaunchNotification()
        {
            if (queue != null)
            {
                queue.EnqueueNotification(Prefab, Canvas, Type, Position, Duration, Title, Message);
            }
            else
            {
                var go = Instantiate(Prefab);
                go.transform.SetParent(Canvas.transform, false);

                var notification = go.GetComponent<Notification>();
                notification.Launch(Type, Position, Duration, Title, Message);
            }
        }
    }
}