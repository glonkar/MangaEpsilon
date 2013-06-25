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
    public static class LibraryService
    {
        public static async Task Initialize()
        {
            if (IsInitialized) return;

            LibraryDirectory = App.AppDataDir + "Library\\";
            LibraryFile = LibraryDirectory + "Library.jml";
            oldLibraryFile = LibraryDirectory + "Library.json"; /* got chapter number and volume number mixed up so I am changing the filename in order 
                                                               * to have a way to check for 'old library format' and 'new library format' which is really just an excuse to rebuild the library.
                                                               */

            await Task.Run(() =>
                {

                    if (!Directory.Exists(LibraryDirectory))
                        Directory.CreateDirectory(LibraryDirectory);

                    if (!File.Exists(LibraryFile))
                    {
                        LibraryCollection = new ObservableCollection<Tuple<Manga.Base.ChapterLight, string>>();

                        if (File.Exists(oldLibraryFile))
                        {
                            //as stated above, convert the old format to the new format.
                            using (var sr = new StreamReader(oldLibraryFile))
                            {
                                using (var jtr = new JsonTextReader(sr))
                                {
                                    ObservableCollection<Tuple<Manga.Base.ChapterLight, string>> oldLibraryItems = App.DefaultJsonSerializer.Deserialize<ObservableCollection<Tuple<Manga.Base.ChapterLight, string>>>(jtr);

                                    foreach (Tuple<Manga.Base.ChapterLight, string> item in oldLibraryItems)
                                    {
                                        item.Item1.ChapterNumber = item.Item1.VolumeNumber;
                                        item.Item1.VolumeNumber = 0.0;
                                        LibraryCollection.Add(item);
                                    }

                                    oldLibraryItems.Clear();
                                }
                            }

                            File.Delete(oldLibraryFile);
                        }
                    }
                    else
                        using (var sr = new StreamReader(LibraryFile))
                        {
                            using (var jtr = new JsonTextReader(sr))
                            {
                                LibraryCollection = App.DefaultJsonSerializer.Deserialize<ObservableCollection<Tuple<Manga.Base.ChapterLight, string>>>(jtr);
                                LibraryCollection = new ObservableCollection<Tuple<Manga.Base.ChapterLight, string>>(LibraryCollection.OrderBy(x => x.Item1.ChapterNumber));
                            }
                        }
                });

            IsInitialized = true;
        }

        public static async Task Deinitialize(bool async = false)
        {
            if (!IsInitialized) return;

            if (async)
                await SaveLibrary(async);
            else
                SaveLibrary(async).Wait();
        }

        private static async Task SaveLibrary(bool async = false)
        {
            using (var sw = new StreamWriter(LibraryFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    App.DefaultJsonSerializer.Serialize(jtw, LibraryCollection);

                    if (async)
                        await sw.FlushAsync();
                    else
                        sw.Flush();
                }
            }
        }

        internal static ObservableCollection<Tuple<Manga.Base.ChapterLight, string>> LibraryCollection = null;

        public static string LibraryDirectory { get; private set; }
        internal static string LibraryFile { get; private set; }
        private static string oldLibraryFile = null;

        public static bool IsInitialized { get; private set; }

        internal static bool Contains(Manga.Base.ChapterEntry chapter)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            return LibraryCollection.Any(x => x.Item1.Name == chapter.Name && x.Item1.ChapterNumber == chapter.ChapterNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName);
        }

        internal static bool Contains(Manga.Base.ChapterLight chapter)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            return LibraryCollection.Any(x => x.Item1.Name == chapter.Name && x.Item1.ChapterNumber == chapter.ChapterNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName);
        }

        internal static async void AddLibraryItem(Tuple<Manga.Base.ChapterLight, string> tuple)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            if (!Contains(tuple.Item1))
            {
                LibraryCollection.Add(tuple);
                await SaveLibrary();

                if (LibraryItemAdded != null)
                    LibraryItemAdded(tuple);
            }
        }

        internal static async void RemoveLibraryItem(Tuple<Manga.Base.ChapterLight, string> tuple, bool deleteData = false)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            if (Contains(tuple.Item1))
            {
                LibraryCollection.Remove(LibraryCollection.First(x => x.Item1.Name == tuple.Item1.Name && x.Item2 == tuple.Item2));
                await SaveLibrary();

                if (deleteData)
                {
                    if (Directory.Exists(tuple.Item2))
                        Directory.Delete(tuple.Item2, true);
                }

                if (LibraryItemRemoved != null)
                    LibraryItemRemoved(tuple);
            }
        }

        internal static Manga.Base.ChapterLight GetDownloadedChapterLightFromEntry(Manga.Base.ChapterEntry chapter)
        {
            return LibraryCollection.First(x => x.Item1.Name == chapter.Name && x.Item1.ChapterNumber == chapter.ChapterNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName).Item1;
        }
        internal static string GetDownloadedChapterLightPathFromEntry(Manga.Base.ChapterEntry chapter)
        {
            return LibraryCollection.First(x => x.Item1.Name == chapter.Name && x.Item1.ChapterNumber == chapter.ChapterNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName).Item2;
        }

        internal static string GetPath(Manga.Base.ChapterLight chapter)
        {
            return LibraryCollection.First(x => x.Item1.Name == chapter.Name && x.Item1.ChapterNumber == chapter.ChapterNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName).Item2;
        }

        public delegate void LibraryItemAddedHandler(Tuple<Manga.Base.ChapterLight, string> tuple);
        public delegate void LibraryItemRemovedHandler(Tuple<Manga.Base.ChapterLight, string> tuple);

        public static event LibraryItemAddedHandler LibraryItemAdded;
        public static event LibraryItemRemovedHandler LibraryItemRemoved;
    }
}
