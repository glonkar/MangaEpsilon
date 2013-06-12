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

        }

        private void thisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FlipViewer.Focus();
        }

        private void uiScaleSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            FlipViewer.Focus();
        }
    }
}
