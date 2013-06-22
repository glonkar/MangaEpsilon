using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaEpsilon.Manga.Sources.MangaFox
{
    public class MangaFoxSource: Manga.Base.IMangaSource
    {
        private static Regex MangaEntryRegex =
            new Regex(@"<li><a href=""(?<url>https?://([-\w\.]+)+(:\d)?(\\-)?(/?([\w\S/_\.]*(\?\S+)?)?))"" rel=""(?<id>\d+)"" class=""series_preview manga_(close|open)"">(?<name>.+?)</a></li>",
                RegexOptions.Compiled | RegexOptions.Singleline);

        public Task<Base.ChapterLight> GetChapterLight(Base.ChapterEntry chapter)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetChapterPageImageUrl(Base.ChapterLight chapter, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public async Task AcquireAvailableManga()
        {
            //http://mangafox.me/manga/

            var list = new List<Manga.Base.Manga>();
            string html = string.Empty;

            using (var client = new HttpClient())
            {
                //mangafox uses some encoding trickery

                HttpResponseMessage response = await client.GetAsync("http://mangafox.me/manga/");

                var bytes = await response.Content.ReadAsByteArrayAsync();

                html = System.Text.Encoding.UTF8.GetString(bytes);
            }

            int totalIndex = 0;
            int index = 0;
            string indexHtml = html;
            while(totalIndex < indexHtml.Length)
            {
                var match = await Task<Match>.Run(() => MangaEntryRegex.Match(indexHtml));

                var mangaObj = new Manga.Base.Manga();

                mangaObj.MangaName = match.Groups["name"].Value;
                mangaObj.ID = match.Groups["id"].Value;
                mangaObj.OnlineWebpage = new Uri(match.Groups["url"].Value);

                list.Add(mangaObj);

                index = match.Index;
                indexHtml = indexHtml.Substring(index);
                totalIndex += match.Length;
            }

            AvailableManga = list;
        }

        public List<Base.Manga> AvailableManga { get; private set; }
        public void LoadAvilableMangaFromFile(string file)
        {
            throw new NotImplementedException();
        }

        public Task<Base.Manga> GetMangaInfo(string name, bool local = true)
        {
            throw new NotImplementedException();
        }

        public Task<Base.Manga> GetMangaInfoByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public Task<Base.ChapterEntry[]> GetNewReleasesOfToday(int amount = 5)
        {
            throw new NotImplementedException();
        }
    }
}
