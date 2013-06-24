using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaEpsilon.Manga.Sources.MangaFox
{
    public class MangaFoxSource : Manga.Base.IMangaSource
    {
        private static Regex MangaEntryRegex =
            new Regex(@"<li><a href=""(?<url>https?://([-\w\.]+)+(:\d)?(\\-)?(/?([\w\S/_\.]*(\?\S+)?)?))"" rel=""(?<id>\d+)"" class=""series_preview manga_(close|open)"">(?<name>.+?)</a></li>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaAuthorRegex =
            new Regex(@"<a href=""/search/author/(?<alias>.+?)/"" style=""color:.+?"">(?<name>.+?)</a>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaArtistRegex =
            new Regex(@"<a href=""/search/artist/(?<alias>.+?)/"" style=""color:.+?"">(?<name>.+?)</a>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaGenreRegex = 
            new Regex(@"<a href=""(http://mangafox.me)?/search/genres/(?<alias>.+?)/"">(?<name>.+?)</a>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaCoverImageRegex =
            new Regex(@"<div class=""cover"">\s*<img.+?src=""(?<url>.+?)"" alt=""(?<name>.+?)"">\s*</div>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaStatusRegex =
            new Regex(@"<div class=""data"">\s*<h5>Status:</h5>\s*<span>\s*(?<status>\w+),?\s*.+?</div>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaDescriptionRegex =
            new Regex(@"<p class=""summary( less)?"">\s*(?<text>.+?)\s*</p>",
                RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex MangaLicensedRegex =
            new Regex(@"<div class=""warning"">\s*The series (?<name>.+?) has been licensed, it is not available in Manga Fox.\s*</div>",
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

            List<Manga.Base.Manga> list = new List<Manga.Base.Manga>();
            string html = await GetHtmlFromUrl("http://mangafox.me/manga/");

            foreach(Match match in await Task<Match>.Run(() => MangaEntryRegex.Matches(html)))
            {
                var mangaObj = new Manga.Base.Manga();

                mangaObj.MangaName = match.Groups["name"].Value;
                mangaObj.ID = match.Groups["id"].Value;
                mangaObj.OnlineWebpage = new Uri(match.Groups["url"].Value);

                list.Add(mangaObj);
            }

            AvailableManga = list;
        }

        private static async Task<string> GetHtmlFromUrl(string url)
        {
            string html = string.Empty;
            using (var client = new HttpClient())
            {
                //mangafox uses some encoding trickery

                HttpResponseMessage response = await client.GetAsync(url);

                byte[] bytes = await response.Content.ReadAsByteArrayAsync();

                html = System.Text.Encoding.UTF8.GetString(bytes);
            }

            return html;
        }

        public List<Base.Manga> AvailableManga { get; private set; }
        public void LoadAvilableMangaFromFile(string file)
        {
            throw new NotImplementedException();
        }

        public async Task<Base.Manga> GetMangaInfo(string name, bool local = true)
        {
            Manga.Base.Manga manga = AvailableManga.Find(x => x.MangaName == name);

            int index = AvailableManga.FindIndex(x => x.MangaName == name);

            manga.SourceName = this.SourceName;

            manga.LanguageByIetfTag = this.LanguageByIetfTag;

            AvailableManga[index] = manga;

            if (local) return manga;

            return await GetMangaInfoByUrl(manga.OnlineWebpage.ToString());
        }

        public async Task<Base.Manga> GetMangaInfoByUrl(string url)
        {
            string html = await GetHtmlFromUrl(url);

            Manga.Base.Manga manga = AvailableManga.Find(x => x.OnlineWebpage.ToString() == url);

            int index = AvailableManga.FindIndex(x => x.OnlineWebpage.ToString() == url);

            string author = MangaAuthorRegex.Match(html).Groups["name"].Value;
            author = string.Join(" ", author.Split(' ').Reverse());

            manga.Author = author;

            //string artist = MangaArtistRegex.Match(html).Groups["name"].Value;
            //artist = string.Join(" ", artist.Split(' ').Reverse());

            //manga.Artist = artist;

            manga.Categories = new System.Collections.ArrayList((System.Collections.ICollection)MangaGenreRegex.Matches(html).OfType<Match>().Select(x => x.Groups["name"].Value).ToArray());

            manga.BookImageUrl = MangaCoverImageRegex.Match(html).Groups["url"].Value;

            var status = MangaStatusRegex.Match(html).Groups["status"].Value;

            switch (status)
            {
                case "Ongoing":
                    manga.Status = Base.MangaStatus.Running;
                    break;
                case "Completed":
                    manga.Status = Base.MangaStatus.Completed;
                    break;
                default:
                    manga.Status = Base.MangaStatus.Unknown;
                    break;
            }

            manga.Description = Regex.Replace(MangaDescriptionRegex.Match(html).Groups["text"].Value,
                @"<br\s*(/)?>", Environment.NewLine, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!MangaLicensedRegex.IsMatch(html))
            {
                //Since the manga hasn't been licensed, the chapters will be available on MangaFox.
            }

            AvailableManga[index] = manga;

            return manga;
        }

        public Task<Base.ChapterEntry[]> GetNewReleasesOfToday(int amount = 5)
        {
            throw new NotImplementedException();
        }


        public string SourceName { get { return "MangaFox"; } }


        public string LanguageByIetfTag { get { return "en"; } }
    }
}
