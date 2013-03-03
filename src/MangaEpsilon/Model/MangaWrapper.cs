using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaEpsilon.Model
{
    public class MangaWrapper : BaseModel
    {
        internal MangaWrapper(Manga.Base.Manga manga)
        {
            if (manga == null) throw new ArgumentNullException("manga");

            Title = manga.MangaName;

            Crystal.Dispatcher.DispatcherService.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(delegate
            {
                ImageUrl = manga.BookImageUrl;
            }));

            WrappedObject = manga;
        }
        public Manga.Base.Manga WrappedObject { get; set; }

        public string Title { get; set; }
        public string ImageUrl { get { return (string)GetProperty("ImageUrl"); } set { SetProperty("ImageUrl", value); RaisePropertyChanged("Image"); } }

        private ImageSource _image = null;
        public ImageSource Image { get { if (ImageUrl == null) return null; else return new BitmapImage(new Uri(ImageUrl)); } }
        public string Subtitle { get; set; }
    }
}
