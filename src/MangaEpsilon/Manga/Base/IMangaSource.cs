using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaEpsilon.Manga.Base
{
    public interface IMangaSource
    {
        [Obsolete()]
        Task<Chapter> GetChapter(ChapterEntry chapter, Action<int, int> progressHandler = null);
        [Obsolete()]
        Task<Chapter> GetChapter(Manga mag, ChapterEntry chapter, Action<int,int> progressHandler = null);
        Task<ChapterLight> GetChapterLight(ChapterEntry chapter, Action<int, int> progressHandler = null);
        Task<string> GetChapterPageImageUrl(ChapterLight chapter, int pageIndex);
        Task<Dictionary<string,string>> GetAvailableManga();
        Task<Manga> GetMangaInfo(string name);
        Task<Manga> GetMangaInfoByUrl(string url);
        Task<ChapterEntry[]> GetNewReleasesOfToday(int amount = 5);
    }
}
