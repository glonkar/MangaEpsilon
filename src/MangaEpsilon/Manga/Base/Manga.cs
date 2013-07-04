using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;
using Crystal.Core;
using System.Collections;

namespace MangaEpsilon.Manga.Base
{
#if !WINDOWS_PHONE
    using System.Net.Http;
#endif

    [DataContract(Name = "Manga")]
    public class Manga: BaseModel
    {
        public Manga()
        {
            Chapters = new ObservableCollection<ChapterEntry>();
        }
        [DataMember]
        public string BookImageUrl { get { return GetPropertyOrDefaultType<string>("BookImageUrl"); } set { SetProperty("BookImageUrl", value); } }
        [DataMember]
        public ObservableCollection<ChapterEntry> Chapters { get { return GetPropertyOrDefaultType<ObservableCollection<ChapterEntry>>(x => this.Chapters); } set { SetProperty(x => this.Chapters, value); } }
        [DataMember]
        public string MangaName { get; set; }
        [DataMember]
        public string Author { get { return (string)GetProperty("Author"); } set { SetProperty("Author", value); } }
        [DataMember]
        public string Description { get { return (string)GetProperty("Description"); } set { SetProperty("Description", value); } }
        [DataMember]
        public int StartRelease { get; set; }
        [DataMember]
        public string ID { get; set; }
        [DataMember]
#if !WINDOWS_PHONE
        public ArrayList Categories { get { return (ArrayList)GetProperty("Categories"); } internal set { SetProperty("Categories", value); } }
#else
        public List<string> Categories { get { return (List<string>)GetProperty("Categories"); } set { SetProperty("Categories", value); } }
#endif
        [DataMember]
        public MangaStatus Status { get { return GetPropertyOrDefaultType<MangaStatus>("Status"); } set { SetProperty("Status", value); } }
        [DataMember]
        public Uri OnlineWebpage { get; set; }
        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public string LanguageByIetfTag { get; set; }
    }

    public enum MangaStatus
    {
        None = -1,
        Unknown = 0,
        YetToBegin = 1,
        Running = 2,
        Completed = 3
    }
    
    public abstract class ChapterBase
    {
        public string Name { get; set; }
        public Manga ParentManga { get; set; }
        public string ID { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double ChapterNumber { get; set; }
        public double VolumeNumber { get; set; }
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

    public class Chapter : ChapterBase
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

    public class ChapterEntry : ChapterBase
    {
        public string Url { get; set; }
        public ChapterEntry(Manga m)
        {
            ParentManga = m;
        }

        public object Subtitle { get; set; }
    }
}
