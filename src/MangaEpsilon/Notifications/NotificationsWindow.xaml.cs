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
using System.Threading.Tasks;
using Crystal.Core;

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

        public bool IsSlideIn { get; set; }
        public double HiddenLeftValue { get; set; }
        public double SlideOutLeft { get; set; }

        public NotificationsWindow()
        {
            InitializeComponent();

            ThemeManager.ChangeTheme(this, (Accent)App.CurrentThemeAccent, (Theme)App.CurrentTheme);

            this.Loaded += NotificationsWindow_Loaded;
            this.Unloaded += NotificationsWindow_Unloaded;
            this.MouseDoubleClick += NotificationsWindow_MouseDoubleClick;
            tm = new System.Timers.Timer();
            tm.Elapsed += tm_Elapsed;

            this.Top = System.Windows.SystemParameters.PrimaryScreenHeight / 8;
            HiddenLeftValue = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Left = HiddenLeftValue;
            SlideOutLeft = HiddenLeftValue - this.Width;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

            Dispatcher.InvokeAsync(() =>
                {
                    SlideIn();
                });
        }

        void NotificationsWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.PreviewMouseLeftButtonUp -= NotificationsWindow_PreviewMouseLeftButtonUp;
            this.Loaded -= NotificationsWindow_Loaded;
            this.Unloaded -= NotificationsWindow_Unloaded;
            this.MouseDoubleClick -= NotificationsWindow_MouseDoubleClick;

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
            tm.Start();
            this.Show();
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
            SlideOut();
        }

        async void tm_Elapsed(object sender, ElapsedEventArgs e)
        {
            tm.Stop();

            while (this.IsMouseOver) { System.Threading.Thread.Sleep(50); } //If the mouse is over, keep the window up.

            Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                 {
                     SlideOut().ContinueWith((t) =>
                         Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                             {
                                 this.Hide();
                             })));
                 }));



        }

        private void SlideIn()
        {
            this.Focus();

            Storyboard storyboard = Resources["SlideIn"] as Storyboard;
            storyboard.Begin(this);

            IsSlideIn = true;

            this.Topmost = true;
        }
        private async Task SlideOut()
        {
            Storyboard storyboard = Resources["SlideOut"] as Storyboard;

            Task waitTask = storyboard.WaitForStoryboardCompletion();
            storyboard.Begin(this);

            await Task.WhenAny(waitTask);

            IsSlideIn = false;
        }
        private double T { get; set; }
    }
}
