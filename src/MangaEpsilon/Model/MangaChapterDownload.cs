using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using MangaEpsilon.Manga.Base;

namespace MangaEpsilon.Model
{
    public class MangaChapterDownload: BaseModel
    {
        internal MangaChapterDownload(ChapterLight entry)
        {
            Chapter = entry;
            Status = MangaChapterDownloadStatus.None;
        }

        public ChapterLight Chapter
        {
            get { return GetPropertyOrDefaultType<ChapterLight>(x => this.Chapter); }
            set { SetProperty(x => this.Chapter, value); }
        }

        public MangaChapterDownloadStatus Status
        {
            get { return GetPropertyOrDefaultType<MangaChapterDownloadStatus>(x => this.Status); }
            set { SetProperty(x => this.Status, value); }
        }

        public int MaxProgress
        {
            get { return GetPropertyOrDefaultType<int>(x => this.MaxProgress); }
            set { SetProperty(x => this.MaxProgress, value); }
        }
        public int Progress
        {
            get { return GetPropertyOrDefaultType<int>(x => this.Progress); }
            set { SetProperty(x => this.Progress, value); }
        }
    }
    public enum MangaChapterDownloadStatus
    {
        None = 0,
        Queued = 1,
        Downloading = 2
    }
}
