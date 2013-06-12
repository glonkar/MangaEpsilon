﻿using System;
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

        private void itemListView_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void amrykidsFavoritesListView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((amrykidsFavoritesListView).SelectedItem == null) return;

            if (e.ChangedButton == MouseButton.Left)
                ((MainWindowAmrykidsFavoritesViewModel)amrykidsFavoritesListView.DataContext).MangaClickCommand.Execute((amrykidsFavoritesListView).SelectedItem);
        }

        private void itemListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((itemListView).SelectedItem == null) return;

            if (e.ChangedButton == MouseButton.Left)
                ((MainWindowTodaysReleasesViewModel)itemListView.DataContext).MangaClickCommand.Execute((itemListView).SelectedItem);
        }

        private void itemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemListView.ContextMenu.DataContext = itemListView.SelectedItem;
        }
    }
}
