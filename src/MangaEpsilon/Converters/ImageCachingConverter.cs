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
    //[ValueConversion(typeof(string), typeof(string))]
    public class ImageCachingConverter : IValueConverter
    {
        private List<string> BadUrls = null;
        public ImageCachingConverter()
        {
            BadUrls = new List<string>();
        }

        public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            if (BadUrls.Contains(value.ToString())) return null;

            var url = new Uri(value.ToString());

            var filename = url.Segments.Last();

            if (!File.Exists(App.ImageCacheDir + filename))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = url;
                image.EndInit();

                image.DownloadFailed += image_DownloadFailed;
                image.DownloadCompleted += image_DownloadCompleted;

                return image;
            }
            else
                return App.ImageCacheDir + filename;
        }

        void image_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            var image = ((BitmapImage)sender);
            image.DownloadFailed -= image_DownloadFailed;

            BadUrls.Add(image.UriSource.ToString());
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
    public class ThumbnailImageCachingConverter : ImageCachingConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            var url = new Uri(value.ToString());

            var filename = url.Segments.Last();

            if (File.Exists(App.ImageCacheDir + filename))
            {
                return CreateThumbnail(new Uri(App.ImageCacheDir + filename), int.Parse(parameter.ToString()));
            }
            else
                return base.Convert(value, targetType, parameter, culture);
        }

        private BitmapImage CreateThumbnail(Uri Source, int PreferredWidth)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = PreferredWidth;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = Source;
            bi.EndInit();
            return bi;
        }
    }
}
