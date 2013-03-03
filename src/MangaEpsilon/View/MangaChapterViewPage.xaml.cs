using MangaEpsilon.Extensions;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaEpsilon.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MangaChapterViewPageViewModel))]
    public sealed partial class MangaChapterViewPage : LayoutAwarePage
    {
        public MangaChapterViewPage()
        {
            this.InitializeComponent();

            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void FlipViewer_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

            // http://www.wiredprairie.us/blog/index.php/archives/1730

            for (int i = 0; i < FlipViewer.Items.Count; i++)
            {
                var element = FlipViewer.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (element != null)
                {
                    var sv = element.FindDescendantByName("PageScrollViewer") as ScrollViewer;
                    if (sv != null)
                    {
                        if (sv.ZoomFactor != ZoomFactor)
                            ZoomFactor = sv.ZoomFactor;

                        sv.ZoomToFactor(ZoomFactor);
                    }
                }
            }
        }

        public float ZoomFactor { get; set; }
    }
}
