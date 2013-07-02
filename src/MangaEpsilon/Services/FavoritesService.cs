using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MangaEpsilon.Services
{
    public static class FavoritesService
    {
        public static async Task Initialize()
        {
            if (IsInitialized) return;

            FavoritesFile = App.AppDataDir + "Favorites.json";

            await LoadFavorites();

            IsInitialized = true;
        }

        public static void Deinitialize()
        {
            if (!IsInitialized) return;

            SaveFavorites();
        }
        public static async Task DeinitializeAsync()
        {
            if (!IsInitialized) return;

            await SaveFavoritesAsync();
        }

        private static async Task LoadFavorites()
        {
            await Task.Run(() =>
                {
                    if (!File.Exists(FavoritesFile))
                    {
                        FavoritesCollection = new ObservableCollection<string>();
                    }
                    else
                        using (var sr = new StreamReader(FavoritesFile))
                        {
                            using (var jtr = new JsonTextReader(sr))
                            {
                                FavoritesCollection = App.DefaultJsonSerializer.Deserialize<ObservableCollection<string>>(jtr);
                            }
                        }
                });
        }

        private static void SaveFavorites()
        {
            using (var sw = new StreamWriter(FavoritesFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    App.DefaultJsonSerializer.Serialize(jtw, FavoritesCollection);

                    sw.Flush();
                }
            }
        }
        private static async Task SaveFavoritesAsync()
        {
            using (var sw = new StreamWriter(FavoritesFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    App.DefaultJsonSerializer.Serialize(jtw, FavoritesCollection);

                    await sw.FlushAsync();
                }
            }
        }

        internal static ObservableCollection<string> FavoritesCollection = null;

        internal static string FavoritesFile { get; private set; }

        public static bool IsInitialized { get; private set; }

        internal static bool IsMangaFavorited(Manga.Base.Manga Manga)
        {
            return FavoritesCollection.Contains(Manga.MangaName);
        }

        internal static Task<bool> IsMangaFavoritedAsync(Manga.Base.Manga Manga)
        {
            return Task.Run<bool>(() => FavoritesCollection.Contains(Manga.MangaName));
        }

        internal static async void AddManga(Manga.Base.Manga Manga)
        {
            if (!FavoritesCollection.Contains(Manga.MangaName))
            {
                FavoritesCollection.Add(Manga.MangaName);
                //await SaveFavorites(false);

                if (ItemFavorited != null)
                    ItemFavorited(Manga);
            }
        }

        internal static async void RemoveManga(Manga.Base.Manga Manga)
        {
            if (FavoritesCollection.Contains(Manga.MangaName))
            {
                FavoritesCollection.Remove(Manga.MangaName);
                //await SaveFavorites(true);

                if (ItemUnfavorited != null)
                    ItemUnfavorited(Manga);
            }
        }

        public delegate void ItemFavoritedHandler(Manga.Base.Manga manga);
        public static event ItemFavoritedHandler ItemFavorited;

        public delegate void ItemUnfavoritedHandler(Manga.Base.Manga manga);
        public static event ItemUnfavoritedHandler ItemUnfavorited;
    }
}
