using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using MangaEpsilon.ViewModel;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;

namespace MangaEpsilonWP
{
    //[Crystal.Navigation.NavigationSetViewModel(typeof(MangaEpsilon.ViewModel.MainWindowTodaysReleasesViewModel))]
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

            var _progressIndicator = new ProgressIndicator();
            _progressIndicator.IsVisible = true;
            _progressIndicator.IsIndeterminate = true;
            _progressIndicator.Text = "Initializing manga components...";
            SystemTray.SetProgressIndicator(this, _progressIndicator);

            await App.MangaSourceInitializationTask;

            SystemTray.SetProgressIndicator(this, null);

            ContentPanel.Visibility = System.Windows.Visibility.Visible;

            //await TaskEx.Delay(10000);

            //MainWindowTodaysReleasesViewModel viewModel = ((MangaEpsilon.ViewModel.MainWindowTodaysReleasesViewModel)newReleasesPivot.DataContext);

            //await viewModel.TodaysReleasesViewModelInitializedTask;

            //newReleasesListBox.ItemsSource = ((MangaEpsilon.ViewModel.MainWindowTodaysReleasesViewModel)newReleasesListBox.DataContext).NewReleasesToday;
        }

        private void amryfavsListBox_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (amryfavsListBox.SelectedItem != null)
                ((MainWindowAmrykidsFavoritesViewModel)amryfavsListBox.DataContext).MangaClickCommand.Execute(amryfavsListBox.SelectedItem);
        }
    }
}