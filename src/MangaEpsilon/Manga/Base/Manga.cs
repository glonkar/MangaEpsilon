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

namespace MangaEpsilon.Manga.Base
{
    [DataContract(Name = "Manga")]
    public class Manga
    {
        public Manga()
        {
            Chapters = new Collection<ChapterEntry>();
        }
        [DataMember]
        public string BookImageUrl { get; internal set; }
        [DataMember]
        public Collection<ChapterEntry> Chapters { get; internal set; }
        [DataMember]
        public string MangaName { get; internal set; }
        [DataMember]
        public string Author { get; internal set; }
        [DataMember]
        public string Description { get; internal set; }
        [DataMember]
        public int StartRelease { get; internal set; }
        [DataMember]
        public string ID { get; internal set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            //Member2 = "This value went into the data file during serialization.";
        }
        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            //Member2 = "This value was reset after serialization.";
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            //Member3 = "This value was set during deserialization";
        }
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //Member4 = "This value was set after deserialization.";

            var x = context.Context;
        }
    }

    public abstract class ChapterBase
    {
        public string Name { get; set; }
        public Manga ParentManga { get; set; }
        public string ID { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int VolumeNumber { get; set; }
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
