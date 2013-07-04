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

namespace MangaEpsilonWP.View
{
    [Crystal.Navigation.NavigationSetViewModel(typeof(MangaEpsilon.ViewModel.MangaDetailPageViewModel))]
    public partial class MangaDetailsPage : PhoneApplicationPage
    {
        public MangaDetailsPage()
        {
            InitializeComponent();
        }

        private void chaptersListBox_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (chaptersListBox.SelectedItem != null)
                ((MangaEpsilon.ViewModel.MangaDetailPageViewModel)this.DataContext).OpenMangaChapterCommand.Execute(chaptersListBox.SelectedItem);
        }
    }
}