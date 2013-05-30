using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MangaEpsilon.Manga.Extensions;
using System.Threading.Tasks;
using MangaEpsilon.Manga.Base;
using System.Net.Http;
using System.Windows.Threading;

namespace MangaEpsilon.Manga.Sources.MangaReader
{
    /// <summary>
    /// http://mangareader.net
    /// </summary>
    public class MangaReaderSource : IMangaSource
    {
        #region IMangaSource Members
        public Dictionary<string, string> mangas = null;
        public List<string> manganames = new List<string>();
        public MangaReaderSource(bool dontfetch = false)
        {
            Initialize(dontfetch);
        }

        private async void Initialize(bool dontfetch)
        {
            if (dontfetch == false)
                mangas = await GetAvailableManga();
            else
                return;
        }
        public MangaReaderSource(Dictionary<string, string> preloadeddict)
        {
            mangas = preloadeddict;
        }

        public async Task<ChapterLight> GetChapterLight(ChapterEntry chapter, Action<int, int> progressHandler = null)
        {
            ChapterLight b = new ChapterLight(chapter.ParentManga);
            b.Name = chapter.Name;
            b.ReleaseDate = chapter.ReleaseDate;
            b.PagesUrls = new List<string>();


            string url = chapter.Url; //first page.

            string firstpagehtml = await GetHtml(url);

            b.PagesUrls.Add(url);

            int maxpages = 0;
            Match pgcount = Regex.Match(firstpagehtml, "<div id=\"selectpage\">.+?</div>", RegexOptions.Singleline | RegexOptions.Compiled);
            string pgcountstr = Regex.Replace(pgcount.Value, "<option.+?>.+?</option>", "");
            pgcountstr = Regex.Replace(pgcountstr, "<.+?>", "");
            pgcountstr = pgcountstr.Replace("of", "").Replace(" ", "").Replace("\n", "");
            maxpages = int.Parse(pgcountstr);

            b.TotalPages = maxpages;

            string url_1 = null;
            string url_2 = null;
            if (url.EndsWith(".html"))
            {
                //old style urls.
                url_1 = url.Substring(0, url.NthIndexOf("-", 2) + 1);
                url_2 = url.Substring(url.NthIndexOf("/", 4));
            }
            else
            {
                url_1 = url + "/";
                url_2 = "";
            }
            for (int i = 2; i < maxpages + 1; i++)
            {
                if (progressHandler != null)
                    progressHandler(i, maxpages);

                string purl = url_1 + i + url_2;

                b.PagesUrls.Add(purl);
            }

            return b;
        }
        public async Task<Chapter> GetChapter(ChapterEntry chapter, Action<int, int> progressHandler = null)
        {
            return await GetChapter(chapter.ParentManga, chapter, progressHandler);
        }
        public async Task<Chapter> GetChapter(MangaEpsilon.Manga.Base.Manga mag, ChapterEntry chapter, Action<int, int> progressHandler = null)
        {
            Chapter b = new Chapter(mag);
            b.Name = chapter.Name;
            b.ReleaseDate = chapter.ReleaseDate;
            b.Pages = new System.Collections.ObjectModel.Collection<object>();
            b.PageOnlineUrls = new System.Collections.ObjectModel.Collection<Uri>();

            string url = chapter.Url; //first page.
            string firstpagehtml = await GetHtml(url);


            int maxpages = 0;
            Match pgcount = Regex.Match(firstpagehtml, "<div id=\"selectpage\">.+?</div>", RegexOptions.Singleline | RegexOptions.Compiled);
            string pgcountstr = Regex.Replace(pgcount.Value, "<option.+?>.+?</option>", "");
            pgcountstr = Regex.Replace(pgcountstr, "<.+?>", "");
            pgcountstr = pgcountstr.Replace("of", "").Replace(" ", "").Replace("\n", "");
            maxpages = int.Parse(pgcountstr);


            Match firstimg = Regex.Match(firstpagehtml, "<img id=\"img\".+?>", RegexOptions.Singleline | RegexOptions.Compiled);
            string firstimgurl = Regex.Match(firstimg.Value, "src=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            firstimgurl = Regex.Replace(firstimgurl, "(src=\"|\")", "");
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var uri = new Uri(firstimgurl);
                b.PageOnlineUrls.Add(uri);
            });

            string url_1 = null;
            string url_2 = null;
            if (url.EndsWith(".html"))
            {
                //old style urls.
                url_1 = url.Substring(0, url.NthIndexOf("-", 2) + 1);
                url_2 = url.Substring(url.NthIndexOf("/", 4));
            }
            else
            {
                url_1 = url + "/";
                url_2 = "";
            }

            for (int i = 2; i < maxpages + 1; i++)
            {
                //if (progressHandler != null)
                //    progressHandler(i, maxpages);

                string purl = url_1 + i + url_2;
                string imgurl = await _GetMangaImageFromUrl(purl);
                await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    var uri = new Uri(imgurl);
                    b.PageOnlineUrls.Add(uri);
                });

                //await Task.Delay(50); //Decreased from 100 to 50, better download time,   // //Prevent simulating a DDOS.
            }

            return b;
        }

        private async Task<string> _GetMangaImageFromUrl(string purl)
        {
            string html = await GetHtml(purl);

            Match img = Regex.Match(html, "<img id=\"img\".+?>", RegexOptions.Singleline | RegexOptions.Compiled);
            string imgurl = Regex.Match(img.Value, "src=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            imgurl = Regex.Replace(imgurl, "(src=\"|\")", "");
            return imgurl;
        }
        private delegate void EmptyDelegate();
        private async Task<string> GetHtml(string url)
        {
            string html = null;

            using (var http = new HttpClient())
            {
                html = await http.GetStringAsync(url);
            }

            return html;
        }
        public async Task<Dictionary<string, string>> GetAvailableManga()
        {
            if (mangas != null && mangas.Keys.Count > 0)
                return mangas;

            Dictionary<string, string> dict = new Dictionary<string, string>();


            string html = await GetHtml("http://www.mangareader.net/alphabetical");
            string important = "";
            important = html.Substring(Regex.Match(html, "<div class=\"content_bloc2\">", RegexOptions.Singleline | RegexOptions.Compiled).Index);
            important = important.Substring(Regex.Match(important, "<div class=\"series_col\">", RegexOptions.Singleline | RegexOptions.Compiled).Index);
            important = important.Substring(0, Regex.Match(important, "<div id=\"adfooter\">", RegexOptions.Singleline | RegexOptions.Compiled).Index);

            foreach (Match m in Regex.Matches(important, "<li>.+?</li>"))
            {
                MatchCollection tagrm = Regex.Matches(m.Value, "<.+?>.+?</.+?>");
                string name = Regex.Replace(tagrm[0].Value, "<.+?>", "");
                string url = "http://www.mangareader.net";
                string urlbit = Regex.Match(tagrm[0].Value, "\".+?\"").Value;
                urlbit = urlbit.Replace("\"", "");
                url += urlbit;


                if (!dict.ContainsKey(name))
                {
                    dict.Add(name, url);

                    manganames.Add(name);
                }
 
            }

            return dict;
        }

        public async Task<MangaEpsilon.Manga.Base.Manga> GetMangaInfoByUrl(string url)
        {
            MangaEpsilon.Manga.Base.Manga m = new MangaEpsilon.Manga.Base.Manga();
            string html = await GetHtml(url);
            string summuaryarea = Regex.Match(html, "<div id=\"readmangasum\">.+?</div>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.Compiled).Value;
            summuaryarea = Regex.Match(summuaryarea, "<p>.+?</p>", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            string sum = Regex.Replace(summuaryarea, "<(/p|p)>", "");

            m.Description = sum;


            string imagearea = Regex.Match(html, "<div id=\"mangaimg\">.+?</div>", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            imagearea = Regex.Match(imagearea, "src=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            string img = Regex.Replace(imagearea, "(src=\"|\")", "");

            m.IsBookImageCached = false;
            m.BookImageUrl = img;

            var name = Regex.Match(html, "<h2 class=\"aname\">.+?</h2>", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            name = Regex.Replace(name, "<.+?>", "");
            m.MangaName = name;


            string chaptersarea = Regex.Match(html, "<div id=\"chapterlist\">.+?</div>.+?</table>", RegexOptions.Singleline | RegexOptions.Compiled).Value;

            foreach (Match chp in Regex.Matches(chaptersarea, "<tr>.+?</tr>", RegexOptions.Singleline | RegexOptions.Compiled))
            {
                MatchCollection split = Regex.Matches(chp.Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled);
                ChapterEntry be = new ChapterEntry(m);
                string datestr = split[1].Value.Replace("</td>", "").Replace("<td>", "");
                DateTime tmp;
                be.ReleaseDate = (DateTime.TryParse(datestr, out tmp)) ? DateTime.Parse(datestr) : DateTime.Now;
                string chpurl = "";
                chpurl = Regex.Match(split[0].Value, "href=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value;
                chpurl = Regex.Replace(chpurl, "(href=\"|\")", "");
                chpurl = "http://www.mangareader.net" + chpurl;
                be.Url = chpurl;

                string nm = Regex.Replace(split[0].Value.Replace("</td>", "").Replace("<td>", ""), "<.+?>", "");
                //nm = nm.Substring(3);
                be.Name = nm.Replace("\n", "");
                be.ID = m.Chapters.Count + 1;
                m.Chapters.Add(be);
            }

            string authorarea = Regex.Match(html, "<td class=\"propertytitle\">Author:.+?</tr>", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            MatchCollection authorsplt = Regex.Matches(authorarea, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled);
            try
            {
                m.Author = Regex.Replace(authorsplt[0].Value, "<.+?>", "");
            }
            catch (Exception) { m.Author = "Unknown"; }

            await m.FetchImage();

            return m;
        }

        public async Task<MangaEpsilon.Manga.Base.Manga> GetMangaInfo(string name)
        {
            if (mangas == null || mangas.Keys.Count == 0)
                mangas = await GetAvailableManga();

            string url = mangas[name];
            return await GetMangaInfoByUrl(url);
        }

        #endregion


        public async Task<ChapterEntry[]> GetNewReleasesOfToday(int amount = 5)
        {
            var html = await GetHtml("http://mangareader.net/");

            var manga_updates_area = Regex.Match(html, "<div id=\"latestchapters\">.+?<h3>Yesterday's Manga</h3>", RegexOptions.Singleline | RegexOptions.Compiled);
            var matchedmangas = Regex.Matches(manga_updates_area.Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled);

            List<ChapterEntry> entries = new List<ChapterEntry>();

            int i = 0;
            foreach (Match match in matchedmangas)
            {
                if (i == amount)
                    break;

                ChapterEntry be = null;

                var manganame = Regex.Match(match.Value,
                    "<a class=\"chapter\".+?>.+?</a>",
                    RegexOptions.Singleline | RegexOptions.Compiled);
                manganame = Regex.Matches(manganame.Value, "\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled)[1];

                var manganamestr = manganame.Value;
                manganamestr = (manganamestr.StartsWith("\"") ? manganamestr.Substring(1) : manganamestr);
                manganamestr = (manganamestr.EndsWith("\"") ? manganamestr.Remove(manganamestr.Length - 1) : manganamestr);

                var mangaurl = "http://mangareader.net" + manganamestr;
                be = new ChapterEntry(await GetMangaInfoByUrl(mangaurl)); //Should implement a cached version for Global



                var chapter = Regex.Match(match.Value,
                    "<a class=\"chaptersrec\".+?>.+?</a>",
                    RegexOptions.Singleline | RegexOptions.Compiled);

                var chpname = Regex.Replace(
                    Regex.Match(chapter.Value, ">.+?<", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                    "(>|<)",
                "");
                be.Name = chpname;

                if (Regex.IsMatch(match.Value, "\"chaptersrec\""))
                {
                    var submatches = Regex.Matches(match.Value, "\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled);
                    var chpurl = submatches[submatches.Count - 1].Value;
                    chpurl = (chpurl.StartsWith("\"") ? chpurl.Substring(1) : chpurl);
                    chpurl = (chpurl.EndsWith("\"") ? chpurl.Remove(chpurl.Length - 1) : chpurl);
                    be.Url = "http://mangareader.net" + chpurl;

                    entries.Add(be);
                    i++;
                }
                else
                    continue;
            }

            return entries.ToArray();
        }


        public async Task<string> GetChapterPageImageUrl(ChapterLight chapter, int pageIndex)
        {
            var nextPage = chapter.PagesUrls[pageIndex];

            string imgurl = await _GetMangaImageFromUrl(nextPage);

            return imgurl;
        }
    }
}
