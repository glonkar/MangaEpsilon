using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaEpsilon.Manga.Base
{
    public interface IMangaSource
    {
        Task<ChapterLight> GetChapterLight(ChapterEntry chapter);
        Task<string> GetChapterPageImageUrl(ChapterLight chapter, int pageIndex);
        Task AcquireAvailableManga();
        List<Manga> AvailableManga { get; }
        void LoadAvilableMangaFromFile(string file);
        Task<Manga> GetMangaInfo(string name, bool local = true);
        Task<Manga> GetMangaInfoByUrl(string url);
        Task<ChapterEntry[]> GetNewReleasesOfToday(int amount = 5);
    }
}
