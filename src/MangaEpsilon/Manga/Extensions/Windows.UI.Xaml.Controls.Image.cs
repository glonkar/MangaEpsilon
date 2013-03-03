using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaEpsilon.Manga.Extensions
{
    public static class ImageExt
    {
        public static async Task LoadUrl(this Image img, string url)
        {
            var bi = new BitmapImage();
            using (var http = new HttpClient())
            {
                var str = (await http.GetStreamAsync(url)).AsInputStream();
                await bi.SetSourceAsync((IRandomAccessStream)str);
            }
            img.Source = bi;
        }
    }
}
