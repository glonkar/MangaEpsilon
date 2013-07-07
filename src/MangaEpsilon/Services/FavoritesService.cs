using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaEpsilon.Manga.Base;
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
            FavoritesCollection = new ObservableCollection<Tuple<string, List<object>>>();

            await Task.Run(() =>
                {
                    if (File.Exists(FavoritesFile))
                    {
                        bool isOldFormat = false;
                        using (var sr = new StreamReader(FavoritesFile))
                        {
                            using (var jtr = new JsonTextReader(sr))
                            {
                                try
                                {
                                    //test if its the old format and if so, convert.

                                    var oldFormatCollection = App.DefaultJsonSerializer.Deserialize<ObservableCollection<string>>(jtr);
                                    jtr.Close();

                                    isOldFormat = true;

                                    if (FavoritesCollection != null)
                                        FavoritesCollection = new ObservableCollection<Tuple<string, List<object>>>();

                                    foreach (var manga in oldFormatCollection)
                                        if (!FavoritesCollection.Any(x => x.Item1 == manga))
                                            FavoritesCollection.Add(new Tuple<string, List<object>>(manga, new List<object>()));
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }

                        //JsonTextReader auto disposes of the upper stream. >_>
                        if (!isOldFormat)
                        {
                            using (var sr = new StreamReader(FavoritesFile))
                            {
                                using (var jtr = new JsonTextReader(sr))
                                {
                                    try
                                    {
                                        //is the new format, load it
                                        FavoritesCollection = App.DefaultJsonSerializer.Deserialize<ObservableCollection<Tuple<string, List<object>>>>(jtr);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                    }
                });

            if (FavoritesLoaded != null)
                FavoritesLoaded();
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

        internal static ObservableCollection<Tuple<string, List<object>>> FavoritesCollection = null;

        internal static string FavoritesFile { get; private set; }

        public static bool IsInitialized { get; private set; }

        internal static bool IsMangaFavorited(Manga.Base.Manga Manga)
        {
            return FavoritesCollection.Any(x => x.Item1 == Manga.MangaName);
        }

        internal static Task<bool> IsMangaFavoritedAsync(Manga.Base.Manga Manga)
        {
            return Task.FromResult(IsMangaFavorited(Manga));
        }

        internal static void AddManga(Manga.Base.Manga Manga)
        {
            if (!IsMangaFavorited(Manga))
            {
                FavoritesCollection.Add(new Tuple<string, List<object>>(Manga.MangaName, new List<object>()));
                //await SaveFavorites(false);

                if (ItemFavorited != null)
                    ItemFavorited(Manga);
            }
        }

        internal static void RemoveManga(Manga.Base.Manga Manga)
        {
            if (IsMangaFavorited(Manga))
            {
                FavoritesCollection.Remove(FavoritesCollection.First(x => x.Item1 == Manga.MangaName));
                //await SaveFavorites(true);

                if (ItemUnfavorited != null)
                    ItemUnfavorited(Manga);
            }
        }

        internal static double[] GetNoAutoDownloadChapters(Manga.Base.Manga manga)
        {
            return FavoritesCollection.First(x => x.Item1 == manga.MangaName).Item2.OfType<double>().ToArray();
        }
        internal static void AddNoAutoDownloadChapter(Manga.Base.Manga manga, ChapterEntry entry)
        {
            AddNoAutoDownloadChapter(manga, entry.ChapterNumber);
        }
        internal static void AddNoAutoDownloadChapter(Manga.Base.Manga manga, ChapterLight entry)
        {
            AddNoAutoDownloadChapter(manga, entry.ChapterNumber);
        }
        internal static void AddNoAutoDownloadChapter(Manga.Base.Manga manga, double entry)
        {
            int index = FavoritesCollection.IndexOf(FavoritesCollection.First(x => x.Item1 == manga.MangaName));

            FavoritesCollection[index].Item2.Add(entry);
        }

        public delegate void ItemFavoritedHandler(Manga.Base.Manga manga);
        public static event ItemFavoritedHandler ItemFavorited;

        public delegate void ItemUnfavoritedHandler(Manga.Base.Manga manga);
        public static event ItemUnfavoritedHandler ItemUnfavorited;

        public delegate void FavoritesLoadedHandler();
        public static event FavoritesLoadedHandler FavoritesLoaded;

        internal static void CheckAndDownload(Manga.Base.ChapterEntry chapter)
        {
            if (chapter == null) return;

            CheckAndDownload(chapter.ParentManga, chapter);
        }
        internal static void CheckAndDownload(Manga.Base.Manga manga, ChapterEntry chapter = null)
        {
            if (manga == null) return;

            if (chapter == null)
            {
                if (manga.Chapters.Count == 0)
                    return;
                chapter = manga.Chapters[0];
            }

            //If the manga is subscribed too (favorited), download the latest manga.
            if (FavoritesService.IsMangaFavorited(manga))
                if (!LibraryService.Contains(chapter) && !DownloadsService.IsDownloading(chapter) && !GetNoAutoDownloadChapters(manga).Contains(chapter.ChapterNumber))
                    DownloadsService.AddDownload(chapter);
        }
    }
}
