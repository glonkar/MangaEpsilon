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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MangaEpsilon.ViewModel;

namespace MangaEpsilon.View
{
    /// <summary>
    /// Interaction logic for MangaChapterViewPage.xaml
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MangaChapterViewPageViewModel))]
    public partial class MangaChapterViewPage : MetroWindow
    {
        public MangaChapterViewPage()
        {
            InitializeComponent();
        }

        private void FlipViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FlipViewer.IsLoaded)
            {
                ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(FlipViewer);

                var scroller = contentPresenter.ContentTemplate.FindName("Scroller", contentPresenter) as ScrollViewer;

                uiScaleSlider.Value = 1;
            }
        }

        private void thisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FlipViewer.Focus();
        }

        private void uiScaleSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            FlipViewer.Focus();
        }

        // zoom support is a combo of: http://blogs.msdn.com/b/ivo_manolov/archive/2007/10/05/ui-scaling-ui-zooming-with-wpf.aspx 
        // http://msdn.microsoft.com/en-us/library/bb613579.aspx 
        // http://stackoverflow.com/questions/10372560/zooming-to-mouse-point-with-scrollview-and-viewbox-in-wpf
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
            {
                uiScaleSlider.Value += (args.Delta > 0) ? 0.1 : -0.1;

                ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(FlipViewer);

                var scroller = contentPresenter.ContentTemplate.FindName("Scroller", contentPresenter) as ScrollViewer;

                var mousePosition = args.GetPosition(this);
                mousePosition = MainGrid.TransformToVisual(scroller).Transform(mousePosition);

                scroller.ScrollToHorizontalOffset(mousePosition.X);
                scroller.ScrollToVerticalOffset(mousePosition.Y * 2);

                args.Handled = true;
            }
            else
                base.OnPreviewMouseWheel(args);
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
