using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MangaEpsilon.Manga.Base
{
    public class Manga
    {
        public Manga()
        {
            Chapters = new Collection<ChapterEntry>();
        }
        public string BookImageUrl { get; set; }
        private ImageSource _imgcache = null;
        public object BookImageFld = null;
        public object BookImage
        {
            get
            {
                if (BookImageFld != null)
                    FetchImage();

                return BookImageFld;
            }
        }
        internal async Task FetchImage()
        {
            if (BookImageUrl != null)
            {
                if (_imgcache == null)
                {
                    try
                    {
                        BitmapImage bi = new BitmapImage(new Uri(BookImageUrl));

                        _imgcache = bi;
                        BookImageFld = bi;
                    }
                    catch (Exception)
                    {
                        BookImageFld = null;
                    }
                }
                else
                    BookImageFld = _imgcache;
            }
        }
        public Collection<ChapterEntry> Chapters { get; set; }
        public string MangaName { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int StartRelease { get; set; }

        public bool IsBookImageCached { get; set; }
    }
    public abstract class ChapterBase
    {
        public string Name { get; set; }
        public Manga ParentManga { get; set; }
        public int ID { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
    public class ChapterLight : ChapterBase
    {
        public ChapterLight(Manga m)
        {
            ParentManga = m;   
        }

        public int TotalPages { get; set; }

        public List<string> PagesUrls { get; set; }
    }
    public class Chapter: ChapterBase
    {
        public Chapter(Manga m)
        {
            ParentManga = m;   
        }
        public Collection<object> Pages { get; set; }
        public Collection<Uri> PageOnlineUrls { get; set; }
        public Collection<string> PageLocalUrls { get; set; }
        public string Filename { get; set; }
    }
    public class ChapterEntry: ChapterBase
    {
        public string Url { get; set; }
        public ChapterEntry(Manga m)
        {
            ParentManga = m;   
        }
    }
}
