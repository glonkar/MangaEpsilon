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
using Crystal.Localization;
using Crystal.Navigation;
using MahApps.Metro.Controls;
using MangaEpsilon.Services;
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
            this.LocalizationFallbackBehavior = Crystal.Localization.LocalizationFallbackBehavior.Fallback;
            this.FallbackLocale = new System.Globalization.CultureInfo("en-US");

            if (!Directory.Exists(AppDataDir))
                Directory.CreateDirectory(AppDataDir);

            CatalogFile = App.AppDataDir + "Manga.jmc";
            oldCatalogFile = App.AppDataDir + "Manga.json";

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

            SoundManager.Initialize();

            LibraryInitializationTask = LibraryService.Initialize();
            FavoritesInitializationTask = FavoritesService.Initialize();

            base.PostStartup();
        }

        protected override void PreShutdown()
        {
            SaveAvailableManga();
            FavoritesService.Deinitialize().Wait();
            LibraryService.Deinitialize(false).Wait();
            base.PreShutdown();
        }

        private static async Task InitializeMangaComponents()
        {
            App.MangaSource = new MangaEpsilon.Manga.Sources.MangaEden.MangaEdenSource();
            App.AggregateMangaSource = new MangaEpsilon.Manga.Sources.MangaFox.MangaFoxSource();

            if (File.Exists(CatalogFile))
            {
                App.MangaSource.LoadAvilableMangaFromFile(CatalogFile);

                if (App.MangaSource.AvailableManga == null)
                {
                    //corruption.
                }
            }
            else
            {
                if (File.Exists(oldCatalogFile))
                {
                    // see LibraryService.cs, line 20 for reasoning as to why im converting...well...in this case..deleting things.
                    File.Delete(oldCatalogFile);
                }

                await App.MangaSource.AcquireAvailableManga();
                await SaveAvailableManga(true);
                await Task.Delay(500);

            }
        }

        private static async Task SaveAvailableManga(bool async = false)
        {
            var preloaded = App.MangaSource.AvailableManga;

            using (var sw = new StreamWriter(CatalogFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    DefaultJsonSerializer.Serialize(jtw, preloaded);

                    if (async)
                        await sw.FlushAsync();
                    else
                        sw.Flush();
                }
            }
        }

        public static MangaEpsilon.Manga.Base.IMangaSource MangaSource { get; private set; }
        public static MangaEpsilon.Manga.Base.IMangaSource AggregateMangaSource { get; private set; }

        public static readonly string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MangaEpsilon\\";
        internal static string CatalogFile = null;
        internal static string oldCatalogFile = null;
        public static string ImageCacheDir = null;
        internal static Task MangaSourceInitializationTask = null;
        internal static Task LibraryInitializationTask = null;
        internal static Task FavoritesInitializationTask = null;
        internal static JsonSerializer DefaultJsonSerializer = null;

        public static volatile bool DownloadsRunning = false;

        internal static object CurrentTheme = null;
        internal static object CurrentThemeAccent = null;

        internal static bool CanMinimizeToTray = false;
        public static bool SaveZoomPosition = false;
    }
}
