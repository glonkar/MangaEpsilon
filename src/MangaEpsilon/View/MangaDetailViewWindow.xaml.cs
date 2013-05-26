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
    /// Interaction logic for MangaDetailViewWindow.xaml
    /// </summary>
    [Crystal.Navigation.NavigationSetViewModel(typeof(MangaDetailPageViewModel))]
    public partial class MangaDetailViewWindow : MetroWindow
    {
        public MangaDetailViewWindow()
        {
            InitializeComponent();
        }

        private void chaptersGridView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((MangaDetailPageViewModel)this.DataContext).OpenMangaChapterCommand.Execute(chaptersGridView.SelectedItem);
        }
    }
}
