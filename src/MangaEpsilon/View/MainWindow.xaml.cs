using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Crystal.Localization;
using MahApps.Metro.Controls;
using MangaEpsilon.Model;
using MangaEpsilon.ViewModel;

namespace MangaEpsilon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MainPageViewModel))]
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            VersionSpan.Inlines.Add(new Run("Version: " + typeof(MainWindow).Assembly.GetName().Version.ToString()));

            Notifications.NotificationsService.AddNotification("Moo", "moo");
        }

        private void itemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemListView.ContextMenu.DataContext = itemListView.SelectedItem;
        }

        private void thisWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.Windows.Count > 1)
            {
                e.Cancel = true;
                Application.Current.Windows[Application.Current.Windows.Count - 1].Focus();
                return;
            }

            if (App.DownloadsRunning)
            {
                if (MessageBox.Show(LocalizationManager.GetLocalizedValue("DownloadClosingWarningMsg"), LocalizationManager.GetLocalizedValue("DownloadClosingWarningTitle"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    e.Cancel = false;
                else
                    e.Cancel = true;
            }

            if (notifyIcon != null)
                notifyIcon.Visible = false;
        }

        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        private void thisWindow_StateChanged(object sender, EventArgs e)
        {
            if (App.CanMinimizeToTray)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    if (notifyIcon == null)
                    {
                        notifyIcon = new System.Windows.Forms.NotifyIcon();
                        notifyIcon.Text = this.Title;
                        notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                    }

                    notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;

                    notifyIcon.Visible = true;

                    this.Hide();
                }
                else
                    if (notifyIcon != null)
                    {
                        notifyIcon.MouseDoubleClick -= notifyIcon_MouseDoubleClick;
                        notifyIcon.Visible = false;
                    }
            }
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.Topmost = true;  // important
            this.Topmost = false; // important
            this.Focus();         // important
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void downloadsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItems == null)
                ((MainWindowDownloadsViewModel)downloadsListView.DataContext).SelectedItems = null;
            else
                ((MainWindowDownloadsViewModel)downloadsListView.DataContext).SelectedItems = (IList)((ListView)sender).SelectedItems;
        }

        private void LibraryTabItem_KeyDown(object sender, KeyEventArgs e)
        {
            UnifiedSearchBox.Focus();
        }

        private void CatalogTabItem_KeyDown(object sender, KeyEventArgs e)
        {
            UnifiedSearchBox.Focus();
        }

        private void UpperTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (UpperTabControl.SelectedIndex)
            {
                case 2:
                case 1:
                    {
                        UnifiedSearchBox.IsEnabled = true;
                        UnifiedSearchBox.Text = string.Empty; //clear before re-binding so the previous viewmodel will not retain the searchfilter

                        UnifiedSearchBox.SetBinding(TextBox.TextProperty, new Binding("SearchFilter")
                        {
                            Source = ((MetroTabItem)UpperTabControl.SelectedItem).DataContext,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Mode = BindingMode.TwoWay,
                            Delay = 500
                        });
                        break;
                    }
                default:
                    UnifiedSearchBox.IsEnabled = false;
                    UnifiedSearchBox.Text = string.Empty; //clear before re-binding so the previous viewmodel will not retain the searchfilter
                    break;
            }

            //Text="{Binding SearchFilter, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay, Delay=200}"
        }
    }
}
