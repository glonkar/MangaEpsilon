using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaFoxSourceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            FetchManga().Wait();

            Console.ReadLine();
        }

        private static async Task FetchManga()
        {
            var mangaFox = new MangaEpsilon.Manga.Sources.MangaFox.MangaFoxSource();
            await mangaFox.AcquireAvailableManga();

            var manga = mangaFox.AvailableManga;
        }
    }
}
