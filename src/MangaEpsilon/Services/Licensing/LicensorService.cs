using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MangaEpsilon.Services.Licensing;
using Newtonsoft.Json;

namespace MangaEpsilon.Services
{
    public static class LicensorService
    {
        private static Regex GetSeriesLinkRegex = new Regex("<td class='text pad'  bgcolor='' ><a href='(?<url>http://www\\.mangaupdates\\.com/series\\.html\\?id=\\d+)' title='Series Info'>(?<name>.+?)</a>",
            RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex IsLicensedRegex = new Regex("<div class=\"sCat\"><b>Licensed \\(in English\\)</b></div>(\n)?<div class=\"sContent\"(\\W)*>(?<licensed>.+?).+?</div>",
            RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex GetLicensorsRegex = new Regex("<a(\\W)href='.+?'(\\W)title='Publisher Info'><u>(?<name>.+?)</u></a>(?<status>.+?)(<br />|</div>)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static List<ILicenseProvider> providers = new List<ILicenseProvider>();

        public static async Task Initialize()
        {
            if (IsInitialized) return;

            LicensorFile = App.AppDataDir + "Licensor.json";

            //normally, I'd use MEF for this.
            providers.Add(new VizMediaLicenseProvider());

            foreach (ILicenseProvider provider in providers)
                provider.LoadLicensedMangas();

            await LoadLicensors();

            IsInitialized = true;
        }

        internal static string LicensorFile { get; private set; }

        public static bool IsInitialized { get; private set; }
        private static List<Tuple<string, bool, string>> Licensors { get; set; }

        public static void Deinitialize()
        {
            if (!IsInitialized) return;

            SaveLicensors();
        }

        private static async Task LoadLicensors()
        {
            Licensors = new List<Tuple<string, bool, string>>();

            await Task.Run(() =>
            {
                if (File.Exists(LicensorFile))
                {
                    using (var sr = new StreamReader(LicensorFile))
                    {
                        using (var jtr = new JsonTextReader(sr))
                        {
                            try
                            {
                                Licensors = App.DefaultJsonSerializer.Deserialize<List<Tuple<string, bool, string>>>(jtr);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            });
        }

        private static void SaveLicensors()
        {
            using (var sw = new StreamWriter(LicensorFile, false))
            {
                using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    jtw.Formatting = Formatting.Indented;

                    App.DefaultJsonSerializer.Serialize(jtw, Licensors);

                    sw.Flush();
                }
            }
        }

        public static async Task<string> GetLicensor(Manga.Base.Manga Manga)
        {
            if (Licensors.Any(x => x.Item1 == Manga.MangaName))
                return Licensors.First(x => x.Item1 == Manga.MangaName).Item3.ToString();
            else
            {
                ILicenseProvider licensor = null;
                if ((licensor = providers.FirstOrDefault(x => x.IsMangaLicensedFromProvider(Manga))) != null)
                    return licensor.Name;
                else
                {
                    string html = string.Empty;
                    using (var http = new HttpClient(new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip }))
                    {
                        html = await (await http.PostAsync("http://www.mangaupdates.com/search.html", new StringContent("search=" + Manga.MangaName + "&x=" + new Random().Next(-50, 100) + "&y=" + new Random().Next(-50, 100), ASCIIEncoding.ASCII, "application/x-www-form-urlencoded"))).Content.ReadAsStringAsync();

                        string url = string.Empty;

                        foreach (Match match in GetSeriesLinkRegex.Matches(html))
                        {
                            if (match.Groups["name"].Value.ToLower().Trim() == Manga.MangaName.ToLower().Trim())
                            {
                                url = match.Groups["url"].Value;
                                break;
                            }
                        }

                        html = await http.GetStringAsync(url);

                        if (IsLicensedRegex.IsMatch(html))
                        {
                            var licenseData = GetLicensorsRegex.Matches(html);

                            foreach (Match match in licenseData)
                            {
                                if (match.Groups["status"].Value.Contains("Ongoing"))
                                {
                                    var licensorName = match.Groups["name"].Value;

                                    Licensors.Add(new Tuple<string, bool, string>(Manga.MangaName, true, licensorName));

                                    return licensorName;
                                }
                            }
                        }
                    }

                    return null;
                }
            }
        }

        internal static async Task<string> GetLicensorBuyLink(Manga.Base.Manga Manga)
        {
            string licensor = await GetLicensor(Manga);

            ILicenseProvider provider = providers.ToArray().FirstOrDefault(x => x.Name.ToLower() == licensor.ToLower());

            if (provider != null)
                return provider.GetShopLink(Manga);

            return null;
        }

        internal static string GetLicensorFriendlyName(string Licensor)
        {
            ILicenseProvider provider = providers.ToArray().FirstOrDefault(x => x.Name.ToLower() == Licensor.ToLower());
            if (provider != null)
                return provider.FriendlyName;
            return Licensor;
        }

        internal static bool IsLicensed(Manga.Base.Manga Manga)
        {
            return Licensors.Any(x => x.Item1 == Manga.MangaName) || providers.Any(x => x.IsMangaLicensedFromProvider(Manga));
        }
    }
}
