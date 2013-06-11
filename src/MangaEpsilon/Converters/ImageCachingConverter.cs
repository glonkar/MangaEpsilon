using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MangaEpsilon.Extensions;

namespace MangaEpsilon.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ImageCachingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var url = new Uri(value.ToString());

            var filename = url.Segments.Last();

            if (!File.Exists(App.ImageCacheDir + filename))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = url;
                image.EndInit();

                image.DownloadCompleted += image_DownloadCompleted;

                return image;
            }
            else
                return App.ImageCacheDir + filename;
        }

        void image_DownloadCompleted(object sender, EventArgs e)
        {
            var image = ((BitmapImage)sender);
            try
            {
                var filename = image.UriSource.Segments.Last();

                BitmapEncoder encoder = null;

                if (filename.ToLower().EndsWith(".jpg") || filename.ToLower().EndsWith(".jepg"))
                    encoder = new JpegBitmapEncoder();
                else if (filename.ToLower().EndsWith(".png"))
                    encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                else if (filename.ToLower().EndsWith(".gif"))
                    encoder = new System.Windows.Media.Imaging.GifBitmapEncoder();
                else
                    encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(image));

                using (var filestream = new FileStream(App.ImageCacheDir + filename, FileMode.Create))
                    encoder.Save(filestream);
            }
            catch (Exception)
            {
            }
            image.DownloadCompleted -= image_DownloadCompleted;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
