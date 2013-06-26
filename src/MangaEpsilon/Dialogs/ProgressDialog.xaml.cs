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

namespace MangaEpsilon.Dialogs
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : MetroWindow
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsCancel = true;
            CancelButton.IsEnabled = false;
        }

        public void UpdateText(string title, string description)
        {
            this.Title = title;
            NameLabel.Content = description;
        }

        public void UpdateProgress(int max, int value)
        {
            PBar.Maximum = max;
            PBar.Value = value;
        }

        public bool IsCancel { get; private set; }
    }
}
