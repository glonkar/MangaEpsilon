using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Crystal.Core;
using Crystal.Navigation;
using MahApps.Metro.Controls;

namespace MangaEpsilon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseCrystalApplication
    {
        protected override void PreStartup()
        {
            this.EnableCrystalLocalization = true;
            this.EnableDeepReflectionCaching = true;
            this.EnableSelfAssemblyResolution = true;

            base.PreStartup();
        }

        public static MangaEpsilon.Manga.Base.IMangaSource MangaSource { get; set; }

        public static ProgressRing ProgressIndicator
        {
            get
            {
                if (Application.Current.MainWindow.Content == null) return null;

                return ((MainWindow)Application.Current.MainWindow).MainProgressIndictator;
            }
        }

    }
}
