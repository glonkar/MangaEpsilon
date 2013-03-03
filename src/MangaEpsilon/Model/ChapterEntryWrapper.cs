using Crystal.Core;
using MangaEpsilon.Manga.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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
                    Crystal.Dispatcher.DispatcherService.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(delegate
                        {
                            ImageUrl = x.Result.BookImageUrl;
                        }));
                });

            WrappedObject = entry;

            Id = WrappedObject.ID;
        }

        public ChapterEntry WrappedObject { get; set; }

        public string Title { get; set; }
        public string ImageUrl { get { return (string)GetProperty("ImageUrl"); } set { SetProperty("ImageUrl", value); RaisePropertyChanged("Image"); } }

        private ImageSource _image = null;
        public ImageSource Image { get { if (ImageUrl == null) return null; else return new BitmapImage(new Uri(ImageUrl)); } }
        public string Subtitle { get; set; }

        public int Id { get; set; }
    }
}
