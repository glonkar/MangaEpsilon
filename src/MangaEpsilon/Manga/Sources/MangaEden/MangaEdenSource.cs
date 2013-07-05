using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MangaEpsilon.Manga.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                var updatedParentManga = await GetMangaInfo(chapter.ParentManga.MangaName, false).ConfigureAwait(false);

                var updatedChapter = updatedParentManga.Chapters.First(x => x.ChapterNumber == light.ChapterNumber);

                light.ID = updatedChapter.ID;
            }


            string json = string.Empty;
            using (var client = new HttpClient())
            {
                json = await client.GetStringAsync("http://www.mangaeden.com/api/chapter/" + light.ID + "/").ConfigureAwait(false);
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
            return Task.FromResult(chapter.PagesUrls[pageIndex]);
        }

        public async Task<Base.Manga> GetMangaInfo(string name, bool local = true)
        {
            //http://www.mangaeden.com/api/manga/[manga.id]/ 

            var manga = AvailableManga.Find(x => x.MangaName == name);

            if (local == false)
            {
                try
                {
                    string json = string.Empty;
                    using (var client = new HttpClient())
                    {
                        json = await client.GetStringAsync("http://www.mangaeden.com/api/manga/" + manga.ID + "/").ConfigureAwait(false);
                    }

                    Dictionary<string, object> data = null;
                    data = await Task.Run(() =>
                    {
                        Dictionary<string, object> result = null;
                        using (StringReader sr = new StringReader(json))
                        {
                            using (JsonTextReader jtr = new JsonTextReader(sr))
                            {
                                result = App.DefaultJsonSerializer.Deserialize<Dictionary<string, object>>(jtr);
                                jtr.Close();
                            }
                        }
                        return result;
                    }).ConfigureAwait(false);

                    //Updates the existing entry for the manga for later.
                    var index = AvailableManga.IndexOf(manga);

                    //manga.MangaName = ((string)data["title"]).Trim();

                    if (manga.Author == null)
                        manga.Author = data["author"] as string;

                    manga.OnlineWebpage = new Uri("http://www.mangaeden.com/en-manga/" + (string)data["alias"] + "/");

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

                    manga.LanguageByIetfTag = this.LanguageByIetfTag;

                    manga.SourceName = this.SourceName;

                    try
                    {
                        manga.Categories = new List<object>(((JArray)data["categories"]).Values());
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

                    await Task.Run(() =>
                        {
                            var chapters = data["chapters"] as IList;

                            var chapterList = new ChapterEntry[int.Parse(data["chapters_len"].ToString())];

                            Parallel.ForEach(chapters.Cast<IList>(), (IList chapter, ParallelLoopState loopState, long loopIndex) =>
                            {
                                var chapterNum = double.Parse(chapter[0].ToString());

                                if (!manga.Chapters.Any(x => x.ChapterNumber == chapterNum))
                                {

                                    ChapterEntry entry = new ChapterEntry(manga);

                                    var time = Sayuka.IRC.Utilities.UnixTimeUtil.UnixTimeToDateTime(chapter[1].ToString());

                                    entry.Name = string.Format("{0} #{1}",
                                        manga.MangaName, chapterNum.ToString());

                                    entry.ReleaseDate = time;

                                    entry.ChapterNumber = chapterNum;

                                    entry.Subtitle = chapter[2];

                                    entry.ID = chapter[3] as string;

                                    chapterList[loopIndex] = entry;
                                }
                                else
                                    chapterList[loopIndex] = manga.Chapters[(int)loopIndex];
                            });

                            manga.Chapters = new System.Collections.ObjectModel.ObservableCollection<ChapterEntry>(chapterList.OrderByDescending(x => x.ChapterNumber));
                        });

                    AvailableManga[index] = manga;
                }
                catch (Exception)
                {
                    //Probably off line. Should return whatever data we have.
                }
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
                html = await client.GetStringAsync("http://www.mangaeden.com/en-directory/?order=3").ConfigureAwait(false);
            }

            var mangaListTable = Regex.Match(html, "<table id=\"mangaList\">.+?</table>", RegexOptions.Singleline | RegexOptions.Compiled);

            var mangaItems = await Task.Run(() => Regex.Matches(mangaListTable.Value, "<tr>.+?</tr>", RegexOptions.Singleline | RegexOptions.Compiled)).ConfigureAwait(false);

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

                ChapterEntry entry = new ChapterEntry(AvailableManga.Find(x => x.MangaName == name));

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
            using (var str = new StreamReader(File.OpenRead(file)))
            {
                using (var jtr = new JsonTextReader(str))
                {
                    AvailableManga = App.DefaultJsonSerializer.Deserialize<List<Manga.Base.Manga>>(jtr);

                    jtr.Close();
                }
            }
        }


        public async Task AcquireAvailableManga()
        {
            //http://www.mangaeden.com/api/list/0/ 

            AvailableManga = AvailableManga == null ? new List<Manga.Base.Manga>() : AvailableManga;

            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync("http://www.mangaeden.com/api/list/0/").ConfigureAwait(false);
                Dictionary<string, object> data = null;

                using (StringReader sr = new StringReader(json))
                {
                    using (JsonTextReader jtr = new JsonTextReader(sr))
                    {
                        data = App.DefaultJsonSerializer.Deserialize<Dictionary<string, object>>(jtr);

                        jtr.Close();
                    }
                }

                JArray mangas = (JArray)data.First().Value;

                foreach (JToken manga in (IEnumerable)mangas)
                {
                    string mangaName = manga["t"].Value<string>();

                    if (AvailableManga.Any(x => x.MangaName == mangaName))
                    {
                        //do something with an existing entry?
                    }
                    else
                    {
                        Manga.Base.Manga mangaObj = new Base.Manga();
                        mangaObj.MangaName = mangaName;
                        mangaObj.ID = manga["i"].Value<string>();
                        mangaObj.OnlineWebpage = new Uri("http://www.mangaeden.com/en-manga/" + (string)manga["a"] + "/");

                        if (manga["im"] != null)
                            mangaObj.BookImageUrl = "http://cdn.mangaeden.com/mangasimg/" + manga["im"] as string;

                        AvailableManga.Add(mangaObj);
                    }

                }

            }
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
