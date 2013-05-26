using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MangaEpsilon.Manga.Extensions
{
    public static class ImageExt
    {
        public static async Task LoadUrl(this Image img, string url)
        {
            var bi = new BitmapImage();
            using (var http = new HttpClient())
            {
                var str = (await http.GetStreamAsync(url));
                bi.StreamSource = str;
            }
            img.Source = bi;
        }
    }
}
