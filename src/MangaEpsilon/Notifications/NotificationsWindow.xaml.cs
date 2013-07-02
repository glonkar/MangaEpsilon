using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Timers;
using System.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro;

namespace MangaEpsilon.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationsWindow.xaml
    /// Based on code written by XAMPP (http://github.com/XAMPP) for Notifications in MangaEplision (http://github.com/Amrykid/MangaEplision)
    /// </summary>
    public partial class NotificationsWindow : MetroWindow
    {
        private Storyboard aniStry;
        private DoubleAnimation heightAni;
        private System.Timers.Timer tm;
        private bool r = false;

        public NotificationsWindow()
        {
            InitializeComponent();

            ThemeManager.ChangeTheme(this, (Accent)App.CurrentThemeAccent, (Theme)App.CurrentTheme);

            this.Loaded += NotificationsWindow_Loaded;
            this.Unloaded += NotificationsWindow_Unloaded;
            this.MouseDoubleClick += NotificationsWindow_MouseDoubleClick;
            aniStry = new Storyboard();
            heightAni = new DoubleAnimation();
            tm = new System.Timers.Timer();
            aniStry.Completed += aniStry_Completed;
            tm.Elapsed += tm_Elapsed;

            this.Top = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth - this.Width;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

            this.T = System.Windows.SystemParameters.WorkArea.Height - this.Height;

            heightAni.To = this.T;
            heightAni.From = this.Top;
            heightAni.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            aniStry.Children.Add(heightAni);
            Storyboard.SetTarget(heightAni, this);
            Storyboard.SetTargetProperty(heightAni, new PropertyPath(Window.TopProperty));
        }

        void NotificationsWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.PreviewMouseLeftButtonUp -= NotificationsWindow_PreviewMouseLeftButtonUp;
            this.Loaded -= NotificationsWindow_Loaded;
            this.Unloaded -= NotificationsWindow_Unloaded;
            this.MouseDoubleClick -= NotificationsWindow_MouseDoubleClick;

            aniStry.Completed -= aniStry_Completed;
            tm.Elapsed -= tm_Elapsed;

            BindingOperations.ClearAllBindings(this);
        }

        ~NotificationsWindow()
        {
            tm.Stop();
            tm.Dispose();
        }

        void NotificationsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var info = ((NotificationInfo)this.DataContext);

            if (info.OnClickCallback != null)
            {
                this.Cursor = Cursors.Hand;
                this.ForceCursor = true;
            }

            //this.PreviewMouseLeftButtonUp += NotificationsWindow_PreviewMouseLeftButtonUp;

            tm.Interval = info.Duration;
            this.Show();
            aniStry.Begin(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Action<NotificationInfo> act = ((NotificationInfo)this.DataContext).OnClickCallback;

            if (act != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                {
                    Dispatcher.Invoke(new MangaEpsilon.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        act((NotificationInfo)this.DataContext);

                        ((NotificationInfo)this.DataContext).OnClickCallback = null;

                    }));

                }));
            }
        }


        private void thisWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnPreviewMouseLeftButtonUp(e);
        }

        void NotificationsWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tm.Stop();
            this.Retract();
        }

        void tm_Elapsed(object sender, ElapsedEventArgs e)
        {
            tm.Stop();
            if (!r)
            {
                while (this.IsMouseOver) { System.Threading.Thread.Sleep(50); } //If the mouse is over, keep the window up.

                Dispatcher.Invoke(new MangaEpsilon.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        this.Retract();
                    }));
            }
            else
            {
                Dispatcher.Invoke(new MangaEpsilon.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        //DialogResult = true;
                        this.Close();
                    }));
            }
        }

        void aniStry_Completed(object sender, EventArgs e)
        {
            tm.Start();
            this.Topmost = true;
        }
        public void Retract()
        {
            aniStry.Children.Remove(heightAni);
            heightAni.To = System.Windows.SystemParameters.PrimaryScreenHeight;
            heightAni.From = this.Top;
            heightAni.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            aniStry.Children.Add(heightAni);
            Storyboard.SetTarget(heightAni, this);
            Storyboard.SetTargetProperty(heightAni, new PropertyPath(Window.TopProperty));
            r = true;
            this.Topmost = false;
            tm.Interval = 1000;
            tm.Start();
            aniStry.Begin(this);
        }
        private double T { get; set; }
    }
}
