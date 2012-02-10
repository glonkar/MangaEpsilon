﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MangaEplision.Metro;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using MangaEplision.Base;
using System.Windows.Threading;
using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace MangaEplision
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        internal bool QueueRunning = false;
        internal List<QueueItem> DlQueue;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.StateChanged += new EventHandler(MainWindow_StateChanged);
            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
            this.CatalogListBox.MouseDoubleClick += new MouseButtonEventHandler(CatalogListBox_MouseDoubleClick);
            DlQueue = new List<QueueItem>();
            this.SourceInitialized += new EventHandler(MainWindow_SourceInitialized);
        }

        void CatalogListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewInfoTile_Click(sender, e);
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (metroBanner.Visibility == System.Windows.Visibility.Collapsed)
                CatalogListBox.Height = (this.Height - ((this.Height - metroTabControl1.ActualHeight) - LatestReleaseGB.FontSize)) - DashboardTab.ActualHeight * 3;
            else if (metroBanner.Visibility == System.Windows.Visibility.Visible)
                CatalogListBox.Height = (this.Height - (this.Height - metroTabControl1.ActualHeight)) - DashboardTab.ActualHeight * 2 - metroBanner.ActualHeight - LatestReleaseGB.FontSize;
        }


        void MainWindow_StateChanged(object sender, EventArgs e)
        {


        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
            {
                //if Win7
                this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            }


            var mainTsk = Task.Factory.StartNew(() =>
                {
                    #region
                    System.Threading.Thread.Sleep(100);
                    Global.Initialize();
                    Dispatcher.Invoke(
                        new EmptyDelegate(
                        () =>
                        {
                            if (Global.Mangas != null)
                            {
                                CatalogListBox.ItemsSource = Global.Mangas;
                                metroGroupBox1.NotificationsCount = Global.Mangas.Length;
                            }

                    #endregion
                        }));
                }).ContinueWith((task) =>
                    {
                        var t1 = Task.Factory.StartNew(() =>
                        {
                            if (Global.SavedQueue())
                            {
                                Global.LoadQueue(ref DlQueue);
                                Dispatcher.Invoke(new EmptyDelegate(() =>
                                {
                                    DlQueueList.ItemsSource = DlQueue;
                                    DlQueueList.Items.Refresh();
                                    QueueStatuslbl.Content = string.Format("There are currently {0} items in your queue.", DlQueue.Count);
                                    if (DlQueue.Count > 0)
                                        CallStartQueueProcess();
                                }));
                                Global.CleanupQueueDir();
                            }
                        });

                        var t2 = Task.Factory.StartNew(() =>
                        {
                            if (NetworkUtils.IsConnectedToInternet())
                            {
                                foreach (BookEntry be in Global.MangaSource.GetNewReleasesOfToday(3))
                                {

                                    Dispatcher.BeginInvoke(new EmptyDelegate(
                                        () =>
                                        {
                                            var bitmp = new BitmapImage();

                                            bitmp.BeginInit();
                                            bitmp.CacheOption = BitmapCacheOption.Default;


                                            bitmp.UriSource = new Uri(be.ParentManga.BookImageUrl);
                                            bitmp.EndInit();

                                            //bitmp.Freeze();
                                            var slide = new MetroBannerSlide();
                                            slide.Header = be.Name + " / " + be.ParentManga.MangaName;

                                            slide.Image = bitmp;

                                            slide.FontSize = 25;
                                            slide.Foreground = Brushes.Red;
                                            slide.FontStyle = FontStyles.Oblique;
                                            metroBanner.Slides.Add(slide);
                                        }));
                                }

                                Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                                    {
                                        metroBanner.Slide = metroBanner.Slides[0];
                                        metroBanner.Start();


                                        LatestReleaseGB.NotificationsCount = metroBanner.Slides.Count;
                                    }));
                            }
                        });

                        Dispatcher.BeginInvoke(
                            new EmptyDelegate(() =>
                                {
                                    try
                                    {
                                        if (NetworkUtils.IsConnectedToInternet())
                                            metroBanner.Visibility = System.Windows.Visibility.Visible;
                                        else
                                            metroBanner.Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }));
                        t1.Wait();
                        t2.Wait();
                        t1.Dispose();
                        t2.Dispose();
                    });

            System.Timers.Timer t = new System.Timers.Timer();
            System.Timers.ElapsedEventHandler h = null;
            h = new System.Timers.ElapsedEventHandler((a, b) =>
                 {
                     mainTsk.Wait();
                     mainTsk.Dispose();
                     t.Elapsed -= h;
                     t.Stop();
                     t.Dispose();

                     GC.KeepAlive(metroBanner);
                     GC.Collect();
                 }); ;
            t.Elapsed += h;
            t.Interval = 10000;
            t.Start();


            Application.Current.Exit += new ExitEventHandler((o, er) => { Global.Current_Exit(o, er); });
            this.Closing += new System.ComponentModel.CancelEventHandler((s, er) =>
            {
                Global.SaveQueue(this.DlQueue);

                metroBanner.Stop();
            });
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Global.Current_DispatcherUnhandledException);

        }
        private delegate void EmptyDelegate();
        private delegate void UpdateDelegate(int Current, int Total);
        private delegate void QueueUpDelegate(QueueItem q);
        private void viewInfoTile_Click(object sender, RoutedEventArgs e)
        {
            if (CatalogListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("You have to select a name before I can fetch information about it!");
            }
            else
            {
                if (NetworkUtils.IsConnectedToInternet())
                {
                    Manga info = null;
                    string name = CatalogListBox.SelectedItem.ToString();
                    var task = Task.Factory.StartNew(() =>
                        {
                            info = Global.GetMangaInfo(name);
                        }).ContinueWith((tsk) =>
                            {
                                if (tsk.Exception == null)
                                    Dispatcher.BeginInvoke(
                                        new EmptyDelegate(
                                            () =>
                                            {
                                                MetroTabItem mti = new MetroTabItem();
                                                mti.IsClosable = true;
                                                mti.Header = info.MangaName;
                                                mti.Content = new MangaInfoControl(info);

                                                metroTabControl1.Items.Add(mti);

                                                metroTabControl1.SelectedItem = mti;
                                            }));
                                else
                                    MessageBox.Show("There was an error grabbing information on that manga!" + Environment.NewLine + "Geeky error details: " + Environment.NewLine + Environment.NewLine + tsk.Exception.ToString() + Environment.NewLine + Environment.NewLine + "NOTE: You can press CTRL + C to copy the contents of this message!");
                                tsk.Dispose();
                            });
                }
                else
                {
                    MessageBox.Show("You are not connected to the internet! Connect and try again!");
                }
            }
        }

        private void CollectionListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CollectionListView.SelectedItem != null)
            {
                var mr = new MangaReaderWin();
                mr.DataContext = CollectionListView.SelectedItem;
                mr.Show();
            }
        }

        #region Queue Downloading
        public bool isQueueRunning = false;
        internal void AddToQueue(BookEntry bookEntry, MangaEplision.Base.Manga manga)
        {
            if (DlQueue != null)
            {
                QueueItem qi = new QueueItem(bookEntry, manga);
                if (!DlQueue.Contains(qi) && !Global.GetBookExist(qi.Manga, qi.Book))
                {
                    DlQueue.Add(qi);
                    DlQueueList.ItemsSource = DlQueue;
                    DlQueueList.Items.Refresh();

                    if (isQueueRunning == false)
                        QueueStatuslbl.Content = string.Format("There are currently {0} items in your queue.", DlQueue.Count);
                }
            }
        }
        internal void CallStartQueueProcess()
        {
            QueueRunning = true;
            var queueRunner = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                int i = 0;
                while (DlQueue.Count > 0)
                {
                    isQueueRunning = true;

                    QueueItem q = DlQueue[i];
                    if (Global.GetBookExist(q.Manga, q.Book))
                        return;
                    else
                    {
                        q.Downloading = true;
                        q.Status = QueueStatus.Downloading;
                        Dispatcher.Invoke(new QueueUpDelegate((qi) =>
                                        {
                                            QueueStatuslbl.Content = string.Format("Downloading {1}", DlQueue.Count, q.Name);
                                            DlQueueList.ItemsSource = DlQueue;
                                            DlQueueList.Items.Refresh();
                                        }), q);

                        Global.DownloadMangaBook(q.Manga, q.Book, (dlBook) =>
                        {
                            q.Downloading = false; q.Status = QueueStatus.Completed; DlQueue.Remove(q); Dispatcher.Invoke(new EmptyDelegate(() =>
                            {
                                DlQueueList.ItemsSource = DlQueue;
                                DlQueueList.Items.Refresh();
                                QueueStatuslbl.Content = string.Format("Done! {0} items left in queue.", DlQueue.Count);
                                Global.DisplayNotification(string.Format("{0} has downloaded.", q.Name), "Download Complete");
                                CurrProg.Value = 0;
                                Count.Content = string.Format("{0}%", 0);

                                Global.CollectionBooks.Add(dlBook);
                                Global.BookCollection = Global.CollectionBooks;

                                if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                                {
                                    //if Win7
                                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                                    this.TaskbarItemInfo.ProgressValue = 0;
                                }
                            }));
                        }, (curr, total) =>
                        {
                            Dispatcher.Invoke(new UpdateDelegate((cur, tot) =>
                            {
                                int percent = ((((cur < tot) ? cur + 1 : cur) * 100) / tot);
                                CurrProg.Value = percent;
                                Count.Content = string.Format("{0}%", percent);

                                if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                                {
                                    //if Win7
                                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                                    this.TaskbarItemInfo.ProgressValue = (double)percent / 100;
                                }
                            }), curr, total);
                        });
                        while (q.Downloading)
                            System.Threading.Thread.Sleep(30000);
                    }
                }
                QueueRunning = false;
                Global.DisplayNotification("Your queue has been emptied!", "Empty Queue!");
            });
        }
        #endregion

        private void searchTile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void searchTile_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        #region Maximized Window handling
        //http://blogs.msdn.com/b/llobo/archive/2006/08/01/maximizing-window-_2800_with-windowstyle_3d00_none_2900_-considering-taskbar.aspx
        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        private static System.IntPtr WindowProc(
              System.IntPtr hwnd,
              int msg,
              System.IntPtr wParam,
              System.IntPtr lParam,
              ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }
        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }
        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };
        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion
    }
}
