using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Messaging;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;

namespace MangaEpsilon.Services
{
    /// <summary>
    /// This class synchronizes with the MainWindowDownloadsViewModel
    /// </summary>
    public static class DownloadsService
    {
        internal static ObservableQueue<MangaChapterDownload> Downloads { get; set; }
        public static bool IsDownloading(ChapterEntry entry)
        {
            return Downloads.Any(x => x.Chapter.Name == entry.Name);
        }
        public static void AddDownload(ChapterEntry entry)
        {
            Messenger.PushMessage(typeof(DownloadsService), "MangaChapterDownload", entry);
        }

        internal static void RaiseDownloadCompleted(ChapterLight entry, MangaEpsilon.ViewModel.MainWindowDownloadsViewModel source)
        {
            if (DownloadCompleted != null)
                DownloadCompleted(entry);
        }

        public delegate void DownloadCompletedHandler(ChapterLight download);
        public static event DownloadCompletedHandler DownloadCompleted;
    }
}
