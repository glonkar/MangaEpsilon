using MangaEpsilon.ViewModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaEpsilon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MainPageViewModel))]
    public sealed partial class MainPage : LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public override void OnVisualStateChange(string newVisualState)
        {
            //layout aware page doesn't wanna work correctly... or its having trouble finding the views so im doing it manually

            switch (newVisualState)
            {
                case "Filled":
                case "FullScreenLandscape":
                case "FullScreenPortrait":
                    itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    itemListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    //nowPlayingPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    backButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    itemListView.Margin = new Thickness(0, 0, 0, 0);

                    pageTitle.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
                    pageTitle.Margin = new Thickness(0, 0, 0, 0);

                    ViewSelection.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                    foreach (Control ui in ((StackPanel)ViewSelection.Children[0]).Children)
                        ui.FontSize = 22;

                    HeaderRow.Height = new GridLength(HeaderRow.MaxHeight);

                    pageTitle.Style = App.Current.Resources["PageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["BackButtonStyle"] as Style;
                    break;

                case "Snapped":
                    itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    itemListView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    //nowPlayingPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    backButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    itemListView.Margin = new Thickness(0, 0, 0, 20);

                    pageTitle.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                    pageTitle.Margin = new Thickness(100, 0, 0, 0);

                    ViewSelection.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                    foreach (Control ui in ((StackPanel)ViewSelection.Children[0]).Children)
                        ui.FontSize = 15;

                    HeaderRow.Height = new GridLength(70);

                    pageTitle.Style = App.Current.Resources["SnappedPageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["SnappedBackButtonStyle"] as Style;
                    break;
            }
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((MainPageViewModel)this.DataContext).MangaClickCommand.Execute(e.ClickedItem);
        }

        private async void ItemView_ItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var pos = e.GetPosition(this);

            var elements = VisualTreeHelper.FindElementsInHostCoordinates(pos, ((UIElement)sender));

            FrameworkElement selectedElement = null;
            //Station selectedStation = null;

            //I could refactor this
            if (sender is GridView)
            {
                selectedElement = (FrameworkElement)elements.FirstOrDefault(x => x.GetType() == typeof(GridViewItem));
            }
            else if (sender is ListView)
            {
                selectedElement = (FrameworkElement)elements.FirstOrDefault(x => x.GetType() == typeof(ListViewItem));
            }

            if (selectedElement != null)
            {
                e.Handled = true;

                //selectedStation = (Station)selectedElement.DataContext;

                //PopupMenu menu = new PopupMenu();
                //menu.Commands.Add(new UICommand(LocalizationManager.GetLocalizedValue("GotoHomepageMenu"), (command) =>
                //{
                //    Windows.System.Launcher.LaunchUriAsync(selectedStation.HomepageUrl);
                //}));

                //this.BottomAppBar.IsSticky = true;
                //this.TopAppBar.IsSticky = true;

                //var chosenCommand = await menu.ShowForSelectionAsync(new Rect(pos, selectedElement.RenderSize));

                //this.BottomAppBar.IsSticky = false;
                //this.TopAppBar.IsSticky = false;

            }
        }
    }
}
