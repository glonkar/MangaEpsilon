using System;
using System.Collections.Generic;
using System.Linq;
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
using MahApps.Metro.Controls;
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
                if (MessageBox.Show("Downloads are in progress. Exiting now will stop them. You would to manually select the items for download again. Are you sure you want to exit?", "Downloads Running", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    e.Cancel = false;
                else
                    e.Cancel = true;
            }
        }
    }
}
