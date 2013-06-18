using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MangaEpsilon.Converters
{
    public class ImageDownloadingConverter : IValueConverter, INotifyPropertyChanged
    {
        private static BitmapImage image = null;
        private static EventHandler<DownloadProgressEventArgs> progressHandler = null;
        private static EventHandler<System.Windows.Media.ExceptionEventArgs> errorHandler = null;
        private static EventHandler doneHandler = null;
        private List<Tuple<Uri, BitmapImage>> cachedImages = new List<Tuple<Uri, BitmapImage>>();


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            Uri url = null;

            if (value is string)
                url = new Uri((string)value);
            else if (value is Uri)
                url = (Uri)value;

            if (image != null)
            {
                if (image.UriSource == url) return null;

                if (IsDownloading)
                {
                    image.DownloadProgress -= progressHandler;
                    image.DownloadFailed -= errorHandler;
                    image.DownloadCompleted -= doneHandler;
                }


                IsDownloading = false;
                IsError = false;
                DownloadProgress = 0;
                DownloadProgressMax = 100;

                image = null;
            }

            if (cachedImages.Any(x => x.Item1 == url))
            {
                IsDownloading = false;
                IsError = false;
                DownloadProgress = 0;
                DownloadProgressMax = 100;

                return cachedImages.First(x => x.Item1 == url).Item2;
            }

            if (url.IsFile)
            {
                return value;
            }

            image = new BitmapImage();

            progressHandler = new EventHandler<DownloadProgressEventArgs>(image_DownloadProgress);
            errorHandler = new EventHandler<System.Windows.Media.ExceptionEventArgs>(image_DownloadFailed);
            doneHandler = new EventHandler(image_DownloadCompleted);

            image.DownloadProgress += progressHandler;
            image.DownloadFailed += errorHandler;
            image.DownloadCompleted += doneHandler;

            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    IsDownloading = true;
                });

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.UriSource = url;
            image.EndInit();

            cachedImages.Add(new Tuple<Uri, BitmapImage>(url, image));

            return image;
        }

        void image_DownloadCompleted(object sender, EventArgs e)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    IsDownloading = false;
                });
        }

        void image_DownloadFailed(object sender, System.Windows.Media.ExceptionEventArgs e)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    IsDownloading = false;
                    IsError = true;
                });
        }

        void image_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    IsDownloading = true;
                    DownloadProgress = e.Progress;
                    DownloadProgressMax = 100;
                });
        }

        private bool _downloading;
        public bool IsDownloading
        {
            get { return _downloading; }
            set { _downloading = value; OnPropertyChanged("IsDownloading"); }
        }

        private bool _error;
        public bool IsError
        {
            get { return _error; }
            set { _error = value; OnPropertyChanged("IsError"); }
        }

        private int _progress;
        public int DownloadProgress
        {
            get { return _progress; }
            set { _progress = value; OnPropertyChanged("DownloadProgress"); }
        }

        private int _progressMax;
        public int DownloadProgressMax
        {
            get { return _progressMax; }
            set { _progressMax = value; OnPropertyChanged("DownloadProgressMax"); }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
