using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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

            if (!Directory.Exists(AppDataDir))
                Directory.CreateDirectory(AppDataDir);

            ImageCacheDir = AppDataDir + "ImageCache\\";

            if (!Directory.Exists(ImageCacheDir))
                Directory.CreateDirectory(ImageCacheDir);

            base.PreStartup();
        }

        protected override void PostStartup()
        {
            MangaSourceInitializationTask = InitializeMangaComponents();

            base.PostStartup();
        }

        protected override void PreShutdown()
        {
            base.PreShutdown();
        }

        private static async Task InitializeMangaComponents()
        {
            App.MangaSource = new MangaEpsilon.Manga.Sources.MangaEden.MangaEdenSource();

            if (File.Exists(App.AppDataDir + "Manga.json"))
                App.MangaSource.LoadAvilableMangaFromFile(App.AppDataDir + "Manga.json");
            else
            {
                await App.MangaSource.AcquireAvailableManga();
                await SaveAvailableManga();
            }
        }

        private static async Task SaveAvailableManga()
        {
            var preloaded = App.MangaSource.AvailableManga;

            using (var fs = new FileStream(App.AppDataDir + "Manga.json", FileMode.OpenOrCreate))
            {
                var json = MangaEpsilon.JSON.JsonSerializer.Serialize(preloaded);

                using (var sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(json);
                    await sw.FlushAsync();
                }
            }
        }

        public static MangaEpsilon.Manga.Base.IMangaSource MangaSource { get; set; }

        public static readonly string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MangaEpsilon\\";
        public static string ImageCacheDir = null;
        internal static Task MangaSourceInitializationTask = null;
    }
}
