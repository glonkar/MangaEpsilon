using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
#if !WINDOWS_PHONE
using System.Net.Http;
#else
using MangaEpsilonWP;
using MangaEpsilonWP.Reimps;
#endif
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MangaEpsilon.Manga.Base;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;

namespace MangaEpsilon.Manga.Sources.MangaEden
{
    public class MangaEdenSource : MangaEpsilon.Manga.Base.IMangaSource
    {
        //http://www.mangaeden.com/api/

        public async Task<Base.ChapterLight> GetChapterLight(Base.ChapterEntry chapter)
        {
            ChapterLight light = new ChapterLight(chapter.ParentManga);
            light.Name = chapter.Name;
            light.ID = chapter.ID;
            light.ChapterNumber = chapter.ChapterNumber;
            light.VolumeNumber = chapter.VolumeNumber;
            light.PagesUrls = new List<string>();

            if (light.ID == null)
            {
                var updatedParentManga = await GetMangaInfo(chapter.ParentManga.MangaName, false);

                var updatedChapter = updatedParentManga.Chapters.First(x => x.ChapterNumber == light.ChapterNumber);

                light.ID = updatedChapter.ID;
            }

            string json = string.Empty;
            using (var client = new HttpClient())
            {
                json = await client.GetStringAsync("http://www.mangaeden.com/api/chapter/" + light.ID + "/");
            }


            Dictionary<string, object> data = null;

            using (var sr = new StringReader(json))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    data = App.DefaultJsonSerializer.Deserialize<Dictionary<string, object>>(jtr);
                    jtr.Close();
                }
            }


            var pages = data["images"];
            foreach (IList page in (IEnumerable)pages)
            {
                light.PagesUrls.Add("http://cdn.mangaeden.com/mangasimg/" + page[1]);
            }

            light.PagesUrls.Reverse();

            light.TotalPages = light.PagesUrls.Count; //hmm...

            return light;
        }

        public Task<string> GetChapterPageImageUrl(Base.ChapterLight chapter, int pageIndex)
        {
#if !WINDOWS_PHONE
            return Task.FromResult(chapter.PagesUrls[pageIndex]);
#else
            return TaskEx.FromResult(chapter.PagesUrls[pageIndex]);
#endif
        }

        public async Task<Base.Manga> GetMangaInfo(string name, bool local = true)
        {
            //http://www.mangaeden.com/api/manga/[manga.id]/ 

#if !WINDOWS_PHONE
            var manga = AvailableManga.Find(x => x.MangaName == name);
#else
            var manga = AvailableManga.First(x => x.MangaName == name);
#endif

            if (local == false)
                try
                {
                    string json = string.Empty;
                    using (var client = new HttpClient())
                    {
                        json = await client.GetStringAsync("http://www.mangaeden.com/api/manga/" + manga.ID + "/");
                    }

                    var data = JSON.JSON.JsonDecode(json) as Dictionary<string, object>;

                    //Updates the existing entry for the manga for later.
                    var index = AvailableManga.IndexOf(manga);

                    //manga.MangaName = ((string)data["title"]).Trim();

                    if (manga.Author == null)
                        manga.Author = data["author"] as string;

                    manga.OnlineWebpage = new Uri("http://www.mangaeden.com/en-manga/" + (string)data["alias"] + "/");

#if !WINDOWS_PHONE
                    manga.Description = Regex.Replace(
                        WebUtility.HtmlDecode(
                            Regex.Replace(
                                (data["description"] as string), 
                                @"<br\s*(/)?>", 
                                Environment.NewLine, 
                                RegexOptions.Compiled | RegexOptions.Singleline)),
                        "<.+?>", 
                        "", 
                        RegexOptions.Singleline | RegexOptions.Compiled);
#else
                    manga.Description = Regex.Replace(
                            Regex.Replace(
                                (data["description"] as string),
                                @"<br\s*(/)?>",
                                Environment.NewLine,
                                RegexOptions.Compiled | RegexOptions.Singleline),
                        "<.+?>",
                        "",
                        RegexOptions.Singleline | RegexOptions.Compiled);
#endif

                    manga.LanguageByIetfTag = this.LanguageByIetfTag;

                    manga.SourceName = this.SourceName;

                    try
                    {
                        manga.Categories = data["categories"] as List<string>;
                    }
                    catch (Exception)
                    {
                    }

                    switch (int.Parse(data["status"].ToString()))
                    {
                        case 1: manga.Status = MangaStatus.Running;
                            break;
                        case 2: manga.Status = MangaStatus.Completed;
                            break;
                    }

                    //manga.Chapters.Clear();

#if !WINDOWS_PHONE
                    await Task.Run(() =>
#else
                    await TaskEx.Run(() =>
#endif
                        {
                            var chapters = data["chapters"] as List<object>;

                            foreach (List<object> chapter in chapters)
                            {
                                ChapterEntry entry = new ChapterEntry(manga);

                                var chapterNum = double.Parse(chapter[0].ToString());

                                var time = Sayuka.IRC.Utilities.UnixTimeUtil.UnixTimeToDateTime(chapter[1].ToString());

                                entry.Name = string.Format("{0} #{1}",
                                    manga.MangaName, chapterNum.ToString());

                                entry.ReleaseDate = time;

                                entry.ChapterNumber = double.Parse(chapterNum.ToString());

                                entry.Subtitle = chapter[2];

                                entry.ID = chapter[3] as string;


                                if (!manga.Chapters.Any(x => x.ChapterNumber == entry.ChapterNumber))
                                    manga.Chapters.Add(entry);
                            }

                            manga.Chapters = new System.Collections.ObjectModel.ObservableCollection<ChapterEntry>(manga.Chapters.OrderByDescending(x => x.ChapterNumber));
                        }).ConfigureAwait(false);

                    AvailableManga[index] = manga;
                }
                catch (Exception)
                {
                    //Probably off line. Should return whatever data we have.
                }

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
                var chapterNum = Regex.Replace(
                    Regex.Replace(
                        Regex.Match(
                            data[3].Value,
                            "<a.+?>.+?<span.+?>",
                            RegexOptions.Singleline | RegexOptions.Compiled).Value
                        , "\".+?\"",
                        "",
                        RegexOptions.Singleline | RegexOptions.Compiled)
                    , "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled).Trim();

                var date = Regex.Replace(
                    Regex.Match(data[3].Value, "<span.+?>.+?</span>", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                    "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled);
                if (date.StartsWith("on "))
                    date = date.Substring(3).Trim();
                date = date.Replace("Today", DateTime.Today.Date.ToShortDateString());
                date = date.Replace("Yesterday", DateTime.Today.Subtract(new TimeSpan(24, 0, 0)).ToShortDateString());

                ChapterEntry entry = new ChapterEntry(AvailableManga.First(x => x.MangaName == name));

                entry.Name = string.Format("{0} #{1}",
                    name, chapterNum);
                entry.ChapterNumber = double.Parse(chapterNum);

                entry.ReleaseDate = DateTime.Parse(date);

                entries.Add(entry);

                i++;
            }

            return entries.ToArray();
        }


        public void LoadAvilableMangaFromFile(string file)
        {
            //AvailableManga = JSON.JsonSerializer.Deserialize<List<Manga.Base.Manga>>(System.IO.File.ReadAllText(file));

#if !WINDOWS_PHONE
            using (var str = new StreamReader(File.OpenRead(file)))
#else
            using (var str = new StreamReader(IsolatedStorageFile.GetUserStoreForApplication().OpenFile(file, FileMode.Open)))
#endif
            {
                using (var jtr = new JsonTextReader(str))
                {
                    AvailableManga = App.DefaultJsonSerializer.Deserialize<List<Manga.Base.Manga>>(jtr);
                }
                str.Close();
            }
        }


        public async Task AcquireAvailableManga()
        {
            //http://www.mangaeden.com/api/list/0/ 

            var list = new List<Manga.Base.Manga>();

            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync("http://www.mangaeden.com/api/list/0/");
                var data = JSON.JSON.JsonDecode(json) as Dictionary<string, object>;

                var mangas = data["manga"] as List<object>;

                foreach (Dictionary<string, object> manga in mangas)
                {
                    Manga.Base.Manga mangaObj = new Base.Manga();
                    mangaObj.MangaName = manga["t"] as string;
                    mangaObj.ID = manga["i"] as string;
                    mangaObj.OnlineWebpage = new Uri("http://www.mangaeden.com/en-manga/" + (string)manga["a"] + "/");

                    if (manga["im"] != null)
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


        public string SourceName { get { return "MangaEden"; } }


        public string LanguageByIetfTag { get { return "en"; } }
    }
}
