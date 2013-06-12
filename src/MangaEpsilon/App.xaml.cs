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
using Newtonsoft.Json;

namespace MangaEpsilon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseCrystalApplication
    {
        protected override void PreStartup()
        {
            DefaultJsonSerializer = Newtonsoft.Json.JsonSerializer.Create(
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All
                });

            this.EnableCrystalLocalization = true;
            this.EnableDeepReflectionCaching = true;
            this.EnableSelfAssemblyResolution = true;

            if (!Directory.Exists(AppDataDir))
                Directory.CreateDirectory(AppDataDir);

            ImageCacheDir = AppDataDir + "ImageCache\\";

            if (!Directory.Exists(ImageCacheDir))
                Directory.CreateDirectory(ImageCacheDir);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.PreStartup();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        protected override void PostStartup()
        {
            MangaSourceInitializationTask = InitializeMangaComponents();

            base.PostStartup();
        }

        protected override void PreShutdown()
        {
            SaveAvailableManga();
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

            using (var sw = new StreamWriter(App.AppDataDir + "Manga.json", false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    DefaultJsonSerializer.Serialize(jtw, preloaded);
                    await sw.FlushAsync();
                }
            }
        }

        public static MangaEpsilon.Manga.Base.IMangaSource MangaSource { get; set; }

        public static readonly string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MangaEpsilon\\";
        public static string ImageCacheDir = null;
        internal static Task MangaSourceInitializationTask = null;
        internal static JsonSerializer DefaultJsonSerializer = null;
    }
}
