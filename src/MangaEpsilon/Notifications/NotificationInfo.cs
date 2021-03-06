﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaEpsilon.Notifications
{
    public class NotificationInfo
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int Duration { get; set; }
        public bool IsUrgent { get; set; }
        public object Image { get; set; }
        public NotificationType Type { get; set; }
        public Action<NotificationInfo> OnClickCallback { get; set; }
    }
}
