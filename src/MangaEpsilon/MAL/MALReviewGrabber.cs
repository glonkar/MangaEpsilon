using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yukihyo.MAL;

namespace MangaEpsilon.MAL
{
    public static class MALReviewGrabber
    {
        private static Regex ReviewAreaMatchingRegex = 
            new Regex(@"<div class=\""borderDark\"" style=\""padding: 14px 13px;\"">.+?<h2>.+?(W*)?User Recommendations</h2>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex IndividualReviewsRegex =
            new Regex("<div class=\"borderLight reviewDetails\">.+?</div>.+?<div class=\"spaceit textReadability\">.+?</span>.+?</div>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex ReviewTextRegex =
            new Regex(@"<td class=""borderClass"" style=""border-width: 0;"">(\d{1,3})?</td>\s+</tr>\s+(</tbody></table>|</table>)\s*</div>.+?</span>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex ReviewAuthorRegex =
            new Regex(@"</div>(\s+)<a href=\"".+?/profile/.+?\"">.*?</a><br />", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex ReviewAuthorAvatarRegex =
            new Regex(@"<img src=""(?<url>.+?)"" border=""0"">", RegexOptions.Singleline | RegexOptions.Compiled);

        private static Regex NoReviewAreaMatchingRegex = 
            new Regex(@"<div style=\""margin: 10px 0px;\"">There have been no reviews submitted for this manga yet\..+?</div>", RegexOptions.Compiled | RegexOptions.Singleline);

        public static async Task<List<MALReview>> GetReviews(string manga)
        {
            return await Task<Func<List<MALReview>>>.Run(new Func<Task<List<MALReview>>>(async () =>
                {
                    Yukihyo.MAL.MALSearchResult mangaResult = null;
                    foreach (var searchResult in Yukihyo.MAL.MyAnimeListAPI.Search(manga, Yukihyo.MAL.MALSearchType.manga))
                    {
                        if (searchResult.Title == manga)
                        {
                            mangaResult = searchResult;
                            break;
                        }
                    }

                    if (mangaResult == null) return null;

                    var url = "http://myanimelist.net/manga/" + mangaResult.ID.ToString();
                    //var manga = MyAnimeListAPI.GetMangaInfo(mangaResult);

                    string html = string.Empty;
                    using (var http = new HttpClient(new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip }))
                    {
                        html = await http.GetStringAsync(url);
                    }


                    if (NoReviewAreaMatchingRegex.IsMatch(html))
                    {
                        //no reviews have been written.
                        return null;
                    }
                    else if (ReviewAreaMatchingRegex.IsMatch(html))
                    {
                        //reviews have been written.

                        List<MALReview> reviews = new List<MALReview>();

                        var reviewArea = ReviewAreaMatchingRegex.Match(html).Value; //this may be redundant. will remove when code is refactored later.

                        foreach(Match review in IndividualReviewsRegex.Matches(reviewArea))
                        {
                            var textMatch = ReviewTextRegex.Match(review.Value);
                            var text = Regex.Replace(
                                textMatch.Value,
                                "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline);
                            var score = int.Parse(text.Split(new char[] { '\n' }, 2)[0]);
                            text = text.Split(new char[] { '\n' }, 2)[1].Trim();
                            var author = Regex.Replace(
                                ReviewAuthorRegex.Match(review.Value).Value,
                                "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline).Replace("_", " ").Trim();

                            var avatarUrl = ReviewAuthorAvatarRegex.Match(review.Value).Groups["url"].Value;

                            var reviewObj = new MALReview();

                            reviewObj.MangaName = manga;
                            reviewObj.ReviewAuthor = author;
                            reviewObj.ReviewText = text;
                            reviewObj.Overall_Rating = score;
                            reviewObj.ReviewAuthorAvatar = new Uri(avatarUrl);

                            reviews.Add(reviewObj);
                        }

                        return reviews;
                    }

                    return null; //what?
                }));
        }
    }

    public class MALReview
    {
        internal MALReview()
        {
        }

        public string MangaName { get; internal set; }
        public string ReviewAuthor { get; internal set; }
        public Uri ReviewAuthorAvatar { get; internal set; }
        public string ReviewText { get; internal set; }
        public int Overall_Rating { get; internal set; }
    }
}
