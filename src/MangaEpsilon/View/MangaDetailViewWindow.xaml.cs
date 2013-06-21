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
using MahApps.Metro;
using MahApps.Metro.Controls;
using MangaEpsilon.ViewModel;

namespace MangaEpsilon.View
{
    /// <summary>
    /// Interaction logic for MangaDetailViewWindow.xaml
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MangaDetailPageViewModel))]
    public partial class MangaDetailViewWindow : MetroWindow
    {
        public MangaDetailViewWindow()
        {
            InitializeComponent();
        }

        private void thisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, (Accent)App.CurrentThemeAccent, (Theme)App.CurrentTheme);
        }

        private void chaptersGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //hack because SelectedItems cannot be bound to.
            if (chaptersGridView.SelectedItems != null)
                ((MangaDetailPageViewModel)this.DataContext).SelectedChapterItems = (MangaEpsilon.Manga.Base.ChapterEntry[])chaptersGridView.SelectedItems.Cast<MangaEpsilon.Manga.Base.ChapterEntry>().ToArray();
        }
    }
}
