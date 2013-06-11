using Crystal.Core;
using MangaEpsilon.Manga.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;

namespace MangaEpsilon.Model
{
    public class ChapterEntryWrapper : BaseModel
    {
        internal ChapterEntryWrapper(ChapterEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            Title = entry.Name + " / " + entry.ParentManga.MangaName;

            App.MangaSource.GetMangaInfo(entry.ParentManga.MangaName).ContinueWith(x =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                        ImageUrl = x.Result.BookImageUrl);
                });

            WrappedObject = entry;

            Id = WrappedObject.ID;
        }

        public ChapterEntry WrappedObject { get; set; }

        public string Title { get; set; }
        public string ImageUrl { get { return (string)GetProperty("ImageUrl"); } set { SetProperty("ImageUrl", value); GetImage(); } }

        private ImageSource _image = null;
        public ImageSource Image
        {
            get
            {
                return _image;
            }
        }
        public string Subtitle { get; set; }

        public int Id { get; set; }

        private async void GetImage()
        {

            var bookImageUri = new Uri(ImageUrl);
            if (!File.Exists(App.ImageCacheDir + bookImageUri.Segments.Last()))
            {
                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(WrappedObject.ParentManga.BookImageUrl, App.ImageCacheDir + bookImageUri.Segments.Last()).ContinueWith(x =>
                        {
                            _image = new BitmapImage(new Uri(App.ImageCacheDir + bookImageUri.Segments.Last()));
                        });
                }
            }
            
            _image = new BitmapImage(new Uri(App.ImageCacheDir + bookImageUri.Segments.Last()));

            RaisePropertyChanged("Image");
        }
    }
}
