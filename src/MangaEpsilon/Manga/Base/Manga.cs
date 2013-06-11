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
    [Serializable]
    public class Manga
    {
        public Manga()
        {
            Chapters = new Collection<ChapterEntry>();
        }
        public string BookImageUrl { get; internal set; }
        public Collection<ChapterEntry> Chapters { get; internal set; }
        public string MangaName { get; internal set; }
        public string Author { get; internal set; }
        public string Description { get; internal set; }
        public int StartRelease { get; internal set; }
        public string ID { get; internal set; }
    }
    [Serializable]
    public abstract class ChapterBase
    {
        public string Name { get; set; }
        public Manga ParentManga { get; set; }
        public string ID { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int VolumeNumber { get; set; }
    }
    [Serializable]
    public class ChapterLight : ChapterBase
    {
        public ChapterLight(Manga m)
        {
            ParentManga = m;   
        }

        public int TotalPages { get; set; }

        public List<string> PagesUrls { get; set; }
    }
    [Serializable]
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
    [Serializable]
    public class ChapterEntry: ChapterBase
    {
        public string Url { get; set; }
        public ChapterEntry(Manga m)
        {
            ParentManga = m;   
        }

        public object Subtitle { get; set; }
    }
}
