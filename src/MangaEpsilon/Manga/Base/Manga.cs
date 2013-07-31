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
using System.Runtime.Serialization;
using Crystal.Core;
using System.Collections;

namespace MangaEpsilon.Manga.Base
{
    [DataContract(Name = "Manga")]
    public class Manga: BaseModel
    {
        public Manga()
        {
            Chapters = new ObservableCollection<ChapterEntry>();
        }
        [DataMember]
        public string BookImageUrl { get { return GetPropertyOrDefaultType<string>("BookImageUrl"); } internal set { SetProperty("BookImageUrl", value); } }
        [DataMember]
        public ObservableCollection<ChapterEntry> Chapters { get { return GetPropertyOrDefaultType<ObservableCollection<ChapterEntry>>(x => this.Chapters); } internal set { SetProperty(x => this.Chapters, value); } }
        [DataMember]
        public string MangaName { get; internal set; }
        [DataMember]
        public string Author { get { return (string)GetProperty("Author"); } internal set { SetProperty("Author", value); } }
        [DataMember]
        public string Description { get { return (string)GetProperty("Description"); } internal set { SetProperty("Description", value); } }
        [DataMember]
        public int StartRelease { get; internal set; }
        [DataMember]
        public string ID { get; internal set; }
        [DataMember]
        public List<object> Categories { get { return (List<object>)GetProperty("Categories"); } internal set { SetProperty("Categories", value); } }
        [DataMember]
        public MangaStatus Status { get { return GetPropertyOrDefaultType<MangaStatus>("Status"); } internal set { SetProperty("Status", value); } }
        [DataMember]
        public Uri OnlineWebpage { get; internal set; }
        [DataMember]
        public string SourceName { get; internal set; }
        [DataMember]
        public string LanguageByIetfTag { get; internal set; }
        [DataMember]
        public List<string> AlternateNames { get; internal set; }
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
