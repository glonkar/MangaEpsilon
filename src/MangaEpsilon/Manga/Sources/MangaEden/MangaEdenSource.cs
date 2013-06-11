using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MangaEpsilon.Manga.Base;

namespace MangaEpsilon.Manga.Sources.MangaEden
{
    public class MangaEdenSource : MangaEpsilon.Manga.Base.IMangaSource
    {
        //http://www.mangaeden.com/api/

        public Task<Base.ChapterLight> GetChapterLight(Base.ChapterEntry chapter)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetChapterPageImageUrl(Base.ChapterLight chapter, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public async Task<Base.Manga> GetMangaInfo(string name)
        {
            //http://www.mangaeden.com/api/manga/[manga.id]/ 

            var manga = AvailableManga.Find(x => x.MangaName == name);

            string json = string.Empty;
            using (var client = new HttpClient())
            {
                json = await client.GetStringAsync("http://www.mangaeden.com/api/manga/" + manga.ID + "/");
            }

            var data = JSON.JSON.JsonDecode(json) as Hashtable;

            //Updates the existing entry for the manga for later.
            var index = AvailableManga.IndexOf(manga);

            if (manga.Author != null)
                manga.Author = data["author"] as string;

            manga.Description = WebUtility.HtmlDecode(data["description"] as string);

            manga.Chapters.Clear();

            var chapters = data["chapters"] as ArrayList;

            foreach (ArrayList chapter in chapters)
            {
                ChapterEntry entry = new ChapterEntry(manga);

                var chapterNum = Convert.ToInt32(double.Parse(chapter[0].ToString())).ToString();

                var time = Sayuka.IRC.Utilities.UnixTimeUtil.UnixTimeToDateTime(chapter[1].ToString());

                entry.Name = string.Format("{0} #{1}",
                    manga.MangaName, chapterNum);

                entry.ReleaseDate = time;

                entry.VolumeNumber = int.Parse(chapterNum.ToString());

                entry.Subtitle = chapter[2];

                entry.ID = chapter[3] as string;

                manga.Chapters.Add(entry);
            }

            AvailableManga[index] = manga;

            return manga;
        }

        public Task<Base.Manga> GetMangaInfoByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<Base.ChapterEntry[]> GetNewReleasesOfToday(int amount = 5)
        {
            //http://www.mangaeden.com/en-directory/?order=3

            string html = string.Empty;
            List<ChapterEntry> entries = new List<ChapterEntry>();

            using (var client = new HttpClient())
            {
                html = await client.GetStringAsync("http://www.mangaeden.com/en-directory/?order=3");
            }

            var mangaListTable = Regex.Match(html, "<table id=\"mangaList\">.+?</table>", RegexOptions.Singleline | RegexOptions.Compiled);

            var mangaItems = Regex.Matches(mangaListTable.Value, "<tr>.+?</tr>", RegexOptions.Singleline | RegexOptions.Compiled);

            int i = 0;

            foreach (Match mangaItem in mangaItems)
            {
                if (i == amount) break;

                var data = Regex.Matches(mangaItem.Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled);

                var name = Regex.Replace(data[0].Value, "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled).Trim();
                var chapterNum = Regex.Replace(data[2].Value, "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled);

                var date = Regex.Replace(
                    Regex.Match(data[3].Value, "<span.+?>.+?</span>", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                    "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled);
                if (date.StartsWith("on "))
                    date = date.Substring(3).Trim();
                date = date.Replace("Today", DateTime.Today.Date.ToShortDateString());
                date = date.Replace("Yesterday", DateTime.Today.Subtract(new TimeSpan(24, 0, 0)).ToShortDateString());

                ChapterEntry entry = new ChapterEntry(AvailableManga.Find(x => x.MangaName == name));

                entry.Name = string.Format("{0} #{1}",
                    name, chapterNum);
                entry.VolumeNumber = int.Parse(chapterNum);

                entry.ReleaseDate = DateTime.Parse(date);

                entries.Add(entry);

                i++;
            }

            return entries.ToArray();
        }


        public void LoadAvilableMangaFromFile(string file)
        {
            AvailableManga = JSON.JsonSerializer.Deserialize<List<Manga.Base.Manga>>(System.IO.File.ReadAllText(file));
        }


        public async Task AcquireAvailableManga()
        {
            //http://www.mangaeden.com/api/list/0/ 

            var list = new List<Manga.Base.Manga>();

            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync("http://www.mangaeden.com/api/list/0/");
                var data = JSON.JSON.JsonDecode(json) as Hashtable;

                var mangas = data["manga"] as ArrayList;

                foreach (Hashtable manga in mangas)
                {
                    Manga.Base.Manga mangaObj = new Base.Manga();
                    mangaObj.MangaName = manga["t"] as string;
                    mangaObj.ID = manga["i"] as string;
                    mangaObj.BookImageUrl = "http://cdn.mangaeden.com/mangasimg/" + manga["im"] as string;

                    list.Add(mangaObj);
                }

            }

            AvailableManga = list;
        }

        public List<Manga.Base.Manga> AvailableManga
        {
            get;
            private set;
        }
    }
}
