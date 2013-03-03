using MangaEpsilon.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MangaEpsilon.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MangaDetailPageViewModel))]
    public sealed partial class MangaDetailPage : LayoutAwarePage
    {
        public MangaDetailPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        public override void OnVisualStateChange(string newVisualState)
        {
            //layout aware page doesn't wanna work correctly... or its having trouble finding the views so im doing it manually
            //VerticalScrollMode="Disabled" HorizontalScrollMode="Enabled" VerticalScrollBarVisibility="Disabled" HorizontalScrollBarVisibility="Visible" x:Name="PageScrollViewer"
            switch (newVisualState)
            {
                case "Filled":
                case "FullScreenLandscape":
                case "FullScreenPortrait":
                    BodyStackPanel.Orientation = Orientation.Horizontal;
                    BodyStackPanel.SetValue(ScrollViewer.IsHorizontalRailEnabledProperty, true);
                    BodyStackPanel.SetValue(ScrollViewer.IsVerticalRailEnabledProperty, false);

                    PageScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                    PageScrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
                    PageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    PageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                    PageScrollViewer.Margin = new Thickness(0);

                    chaptersGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    ChaptersGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    //chaptersListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    CoverImage.Width = 450;
                    CoverImage.Height = 550;

                    MangaInfoGrid.Height = 600;
                    MangaInfoGrid.Width = 525;

                    DescriptionGrid.Width = 550;
                    DescriptionGrid.Height = 600;

                    DescriptionTextBlock.Width = 450;

                    pageTitle.Style = App.Current.Resources["PageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["BackButtonStyle"] as Style;
                    break;

                case "Snapped":
                    BodyStackPanel.Orientation = Orientation.Vertical;
                    BodyStackPanel.SetValue(ScrollViewer.IsHorizontalRailEnabledProperty, false);
                    BodyStackPanel.SetValue(ScrollViewer.IsVerticalRailEnabledProperty, true);

                    PageScrollViewer.VerticalScrollMode = ScrollMode.Auto;
                    PageScrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                    PageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    PageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    PageScrollViewer.Margin = new Thickness(-20, 0, 0, 0);

                    chaptersGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    ChaptersGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    //chaptersListView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    CoverImage.Width = 300;
                    CoverImage.Height = 400;

                    MangaInfoGrid.Height = 450;
                    MangaInfoGrid.Width = 375;

                    DescriptionGrid.Width = 300;
                    DescriptionGrid.Height = double.NaN;

                    DescriptionTextBlock.Width = 235;

                    pageTitle.Style = App.Current.Resources["SnappedPageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["SnappedBackButtonStyle"] as Style;
                    break;
            }
        }

        private void chaptersView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((MangaDetailPageViewModel)this.DataContext).OpenMangaChapterCommand.Execute(e.ClickedItem);
        }
    }
}
