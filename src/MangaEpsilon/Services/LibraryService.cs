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
        }

        private static Collection<Tuple<Manga.Base.ChapterLight, string>> LibraryCollection = null;

        public static string LibraryDirectory { get; private set; }
        internal static string LibraryFile { get; private set; }
    }
}
