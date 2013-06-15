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
        public static async void Initialize()
        {
            if (IsInitialized) return;

            LibraryDirectory = App.AppDataDir + "Library\\";
            LibraryFile = LibraryDirectory + "Library.json";

            await Task.Run(() =>
                {

                    if (!Directory.Exists(LibraryDirectory))
                        Directory.CreateDirectory(LibraryDirectory);

                    if (!File.Exists(LibraryFile))
                        LibraryCollection = new Collection<Tuple<Manga.Base.ChapterLight, string>>();
                    else
                        using (var sr = new StreamReader(LibraryFile))
                        {
                            using (var jtr = new JsonTextReader(sr))
                            {
                                LibraryCollection = App.DefaultJsonSerializer.Deserialize<Collection<Tuple<Manga.Base.ChapterLight, string>>>(jtr);
                            }
                        }
                });

            IsInitialized = true;
        }

        public static async void Deinitialize()
        {
            if (!IsInitialized) return;

            using (var sw = new StreamWriter(LibraryFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    App.DefaultJsonSerializer.Serialize(jtw, LibraryCollection);
                    await sw.FlushAsync();
                }
            }
        }

        private static Collection<Tuple<Manga.Base.ChapterLight, string>> LibraryCollection = null;

        public static string LibraryDirectory { get; private set; }
        internal static string LibraryFile { get; private set; }

        public static bool IsInitialized { get; private set; }

        internal static bool Contains(Manga.Base.ChapterEntry chapter)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            return LibraryCollection.Any(x => x.Item1.Name == chapter.Name && x.Item1.VolumeNumber == chapter.VolumeNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName);
        }

        internal static bool Contains(Manga.Base.ChapterLight chapter)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            return LibraryCollection.Any(x => x.Item1.Name == chapter.Name && x.Item1.VolumeNumber == chapter.VolumeNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName);
        }

        internal static void AddLibraryItem(Tuple<Manga.Base.ChapterLight, string> tuple)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            if (!Contains(tuple.Item1))
                LibraryCollection.Add(tuple);
        }

        internal static Manga.Base.ChapterLight GetDownloadedChapterLightFromEntry(Manga.Base.ChapterEntry chapter)
        {
            return LibraryCollection.First(x => x.Item1.Name == chapter.Name && x.Item1.VolumeNumber == chapter.VolumeNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName).Item1;
        }
        internal static string GetDownloadedChapterLightPathFromEntry(Manga.Base.ChapterEntry chapter)
        {
            return LibraryCollection.First(x => x.Item1.Name == chapter.Name && x.Item1.VolumeNumber == chapter.VolumeNumber && x.Item1.ParentManga.MangaName == chapter.ParentManga.MangaName).Item2;
        }
    }
}
