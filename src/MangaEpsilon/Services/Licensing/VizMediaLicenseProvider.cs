using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MangaEpsilon.Services.Licensing
{
    [Export(typeof(ILicenseProvider))]
    internal class VizMediaLicenseProvider : ILicenseProvider
    {
        private List<Tuple<string, string>> licensedTitles = null;

        private static Regex LicensedMangaRegex = new Regex("<a class=\"track(\\W*showlink_mature)?\" href=\"(?<url>/.+?)\".+?>(?<name>.+?)</a>",
            RegexOptions.Compiled | RegexOptions.Singleline);

        internal VizMediaLicenseProvider()
        {
            ProviderLicensedTitlesFile = App.AppDataDir + "VizMedia.LicensedTitles.json";
            licensedTitles = new List<Tuple<string, string>>();
        }

        public string Name
        {
            get { return "Viz"; }
        }

        public string FriendlyName
        {
            get { return "Viz Media"; }
        }

        public string GetShopLink(Manga.Base.Manga Manga)
        {
            if (IsMangaLicensedFromProvider(Manga))
                return "http://www.vizmanga.com" + GetMangaShopLink(Manga).Item2;

                        //educated guess.
            return "http://www.vizmanga.com/" + Manga.MangaName.ToLower().Replace(" ", "-").Replace("&", "and").Replace("!", "").Replace(".", "").Replace(":", "");
        }

        public async Task LoadLicensedMangas()
        {
            if (File.Exists(ProviderLicensedTitlesFile))
            {
                using (var sr = new StreamReader(ProviderLicensedTitlesFile))
                {
                    using (var jtr = new JsonTextReader(sr))
                    {
                        try
                        {
                            licensedTitles = App.DefaultJsonSerializer.Deserialize<List<Tuple<string, string>>>(jtr);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            else
            {
                string html = string.Empty;
                using (var http = new HttpClient(new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip }))
                {
                    html = await http.GetStringAsync("http://www.vizmanga.com/").ConfigureAwait(false);
                }

                foreach (Match match in LicensedMangaRegex.Matches(html))
                {
                    if (!licensedTitles.Any(x => x.Item1 == match.Groups["name"].Value))
                        licensedTitles.Add(new Tuple<string, string>(match.Groups["name"].Value, match.Groups["url"].Value));
                }

                using (var sw = new StreamWriter(ProviderLicensedTitlesFile, false))
                {
                    using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                    {
                        jtw.Formatting = Formatting.Indented;

                        App.DefaultJsonSerializer.Serialize(jtw, licensedTitles);

                        sw.Flush();
                    }
                }
            }

        }

        public System.Globalization.RegionInfo LicenseRegion
        {
            get { return new System.Globalization.RegionInfo("US"); }
        }


        public string ProviderLicensedTitlesFile { get; private set; }


        public bool IsMangaLicensedFromProvider(Manga.Base.Manga Manga)
        {
            return licensedTitles.Any(x =>
            {
                if (x.Item1.ToLower() == Manga.MangaName.ToLower())
                    return true;

                if (Manga.AlternateNames != null && Manga.AlternateNames.Count > 0)
                    foreach (string altName in Manga.AlternateNames)
                        if (altName.ToLower() == x.Item1.ToLower())
                            return true;

                return false;
            });
        }

        private Tuple<string,string> GetMangaShopLink(Manga.Base.Manga Manga)
        {
            return licensedTitles.First(x =>
            {
                if (x.Item1.ToLower() == Manga.MangaName.ToLower())
                    return true;

                if (Manga.AlternateNames != null && Manga.AlternateNames.Count > 0)
                    foreach (string altName in Manga.AlternateNames)
                        if (altName.ToLower() == x.Item1.ToLower())
                            return true;

                return false;
            });
        }
    }
}
