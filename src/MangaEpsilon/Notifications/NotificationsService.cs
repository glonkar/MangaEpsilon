using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Media;
using MangaEpsilon.Notifications;
using MangaEpsilon;

namespace MangaEpsilon.Notifications
{
    public class NotificationsService
    {
        static NotificationsService()
        {
            Notifications = new ObservableQueue<NotificationInfo>();
        }

        public static ObservableQueue<NotificationInfo> Notifications { get; private set; }
        public static bool QueueRunning { get; private set; }

        public static void ClearNotificationQueue()
        {
            lock (Notifications)
            {
                Queue<NotificationInfo> tmp = new Queue<NotificationInfo>();

                while (!Notifications.IsEmpty)
                {
                    var d = Notifications.Peek();

                    if (d.IsUrgent)
                        tmp.Enqueue(d);

                    Notifications.Dequeue();
                }

                foreach (NotificationInfo ni in tmp)
                    Notifications.Enqueue(ni);
            }
        }


        public static void AddNotification(string title, string message = "", object image = null, int duration = 3000, bool isUrgent = false, NotificationType type = NotificationType.Information, Action<NotificationInfo> onClickCallback = null)
        {
            if (System.Windows.Application.Current == null)
                return;

            if (title == null)
                throw new ArgumentNullException("title");
            if (message == null)
                throw new ArgumentNullException("message");
            if (duration == 0)
                throw new ArgumentOutOfRangeException("duration");

            lock (Notifications)
            {
                Notifications.Enqueue(
                    new NotificationInfo()
                    {
                        Title = title,
                        Message = message,
                        Duration = duration,
                        IsUrgent = isUrgent,
                        Image = image,
                        Type = type,
                        OnClickCallback = onClickCallback
                    });
            }

            HandleQueue();
        }

        static void HandleQueue()
        {
            if (QueueRunning)
                return;

            ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                {

                    while (!Notifications.IsEmpty)
                    {
                        if (System.Windows.Application.Current == null)
                        {
                            QueueRunning = false;
                            return;
                        }

                        QueueRunning = true;
                        try
                        {
                            NotificationsWindow nw = null;
                            var msg = Notifications.Dequeue();

                            if (msg != null)
                            {
                                Application.Current.Dispatcher.Invoke(new EmptyDelegate(
                                    () =>
                                    {
                                        nw = new NotificationsWindow();

                                        nw.DataContext = msg;

                                        nw.Show();

                                        if (App.EnableNotificationsSounds)
                                        {
                                            try
                                            {
                                                SoundManager.WindowsPrintCompleted.Play();
                                            }
                                            catch (Exception) { }
                                        }
                                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                                while (nw.IsVisible)
                                    Thread.Sleep(200);

                                Application.Current.Dispatcher.Invoke(new EmptyDelegate(
                                    () =>
                                    {
                                        nw.Close();
                                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }

                        }
                        catch (Exception)
                        {
                            if (Application.Current == null)
                                System.Diagnostics.Process.GetCurrentProcess().Kill(); //If by some rare bug, Hanasu doesn't exit, force itself to die.
                        }
                    }

                    QueueRunning = false;
                }));
        }

        public delegate void EmptyDelegate();
        public delegate object EmptyReturnDelegate();

    }
}
