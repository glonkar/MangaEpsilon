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
        }

        public static async Task Deinitialize(bool async = false)
        {
            if (!IsInitialized) return;

            if (async)
                await SaveFavorites(async);
            else
                SaveFavorites(async).Wait();
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

        private static async Task SaveFavorites(bool async = false)
        {
            using (var sw = new StreamWriter(FavoritesFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    App.DefaultJsonSerializer.Serialize(jtw, FavoritesCollection);
                    if (async)
                        await sw.FlushAsync();
                    else
                        sw.Flush();
                }
            }
        }

        internal static ObservableCollection<string> FavoritesCollection = null;

        internal static string FavoritesFile { get; private set; }

        public static bool IsInitialized { get; private set; }
    }
}
