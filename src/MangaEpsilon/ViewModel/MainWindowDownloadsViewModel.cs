using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Crystal.Command;
using Crystal.Core;
using Crystal.Messaging;
using Crystal.Navigation;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowDownloadsViewModel : BaseViewModel
    {
        public MainWindowDownloadsViewModel()
        {
            if (IsDesignMode) return;

            Downloads = new ObservableQueue<MangaChapterDownload>();
            RegisterForMessages("MangaChapterDownload");

            CancelDownloadCommand = CommandManager.CreateProperCommand((o) =>
            {
                var download = (MangaChapterDownload)o;

                if (download.Status == MangaChapterDownloadStatus.Downloading && Downloads.Peek() == download)
                {
                    download = Downloads.Peek(); //set a reference.
                    download.Status = MangaChapterDownloadStatus.Canceled;
                }
                else
                {
                    List<MangaChapterDownload> downloadsToRequeue = new List<MangaChapterDownload>();

                    while (Downloads.Count > 0)
                    {
                        var mangaChapter = Downloads.Dequeue();
                        if (mangaChapter != o)
                            downloadsToRequeue.Add(mangaChapter);
                    }

                    foreach (var d in downloadsToRequeue)
                        Downloads.Enqueue(d);
                }
            },
            (o) =>
            {
                return o != null && o is MangaChapterDownload;
            });
        }

        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            switch (message.MessageString)
            {
                case "MangaChapterDownload":
                    DownloadChapter(message);
                    return true;
                default:
                    return base.ReceiveMessage(source, message);
            }
        }

        private async void DownloadChapter(Crystal.Messaging.Message message)
        {
            ChapterEntry chapter = (ChapterEntry)message.Data;

            var chap = await App.MangaSource.GetChapterLight(chapter);

            if (!LibraryService.Contains(chap) && !Downloads.Any(x => x.Chapter.Name == chap.Name))
            {
                Downloads.Enqueue(new MangaChapterDownload(chap) { MaxProgress = chap.PagesUrls.Count, Status = MangaChapterDownloadStatus.Queued });

                RaisePropertyChanged(x => this.Downloads);

                if (!App.DownloadsRunning)
                    DownloadAllQueuedChapters();
            }
        }

        private async void DownloadAllQueuedChapters()
        {
            await Task.Run(async () =>
            {
                App.DownloadsRunning = true;

                Messenger.PushMessage(this, "UpdateMainWindowState", System.Windows.Shell.TaskbarItemProgressState.Normal);

                while (Downloads.Count > 0)
                {
                    var download = Downloads.Peek();

                    download.Status = MangaChapterDownloadStatus.Downloading;


                    var downloadPath = LibraryService.LibraryDirectory + download.Chapter.ParentManga.MangaName + "\\" + download.Chapter.ChapterNumber.ToString() + "\\";

                    if (!Directory.Exists(downloadPath))
                        Directory.CreateDirectory(downloadPath);

                    download.MaxProgress = download.Chapter.PagesUrls.Count;

                    bool error = false;

                    using (WebClient wc = new WebClient())
                    {
                        download.TotalFilesToDownload = download.Chapter.PagesUrls.Count;
                        foreach (var pageUrl in download.Chapter.PagesUrls)
                        {
                            if (download.Status == MangaChapterDownloadStatus.Canceled)
                                break;

                            var url = new Uri(pageUrl.ToString());

                            var filename = url.Segments.Last();

                            for (int i = 0; i < 3; i++)
                            {
                                //attempts to download the file a maximum of 3 times, in case the download fails.
                                try
                                {
                                    await wc.DownloadFileTaskAsync(pageUrl, downloadPath + filename);
                                    error = false;
                                    break;
                                }
                                catch (Exception)
                                {
                                    error = true;
                                }
                            }
                            if (!error)
                            {
                                download.Progress++;
                                download.TotalFilesDownloaded++;

                                Messenger.PushMessage(this, "UpdateMainWindowProgress", (Convert.ToDouble(download.Progress) / Convert.ToDouble(download.MaxProgress)));

                                download.ProgressStr = Math.Round(((Convert.ToDouble(download.Progress) / Convert.ToDouble(download.MaxProgress)) * 100.0), 2).ToString() + "%";


                                await Task.Delay((download.Chapter.PagesUrls.Count - download.Progress) * 2 + 500);
                            }
                            else
                            {
                                Messenger.PushMessage(this, "UpdateMainWindowState", System.Windows.Shell.TaskbarItemProgressState.None);
                                Notifications.NotificationsService.AddNotification("Download Failed!", download.Chapter.Name + " has failed to download.");
                            }
                        }

                        if (download.Status == MangaChapterDownloadStatus.Downloading && !error)
                            download.Status = MangaChapterDownloadStatus.Completed;
                    }

                    if (download.Status == MangaChapterDownloadStatus.Completed)
                    {
                        if (!error)
                        {
                            LibraryService.AddLibraryItem(new Tuple<ChapterLight, string>(download.Chapter, downloadPath));
                        }
                    }

                    Downloads.Dequeue();

                    await Task.Delay(1000);
                }

                Messenger.PushMessage(this, "UpdateMainWindowState", System.Windows.Shell.TaskbarItemProgressState.None);

                App.DownloadsRunning = false;
            });
        }

        public ObservableQueue<MangaChapterDownload> Downloads
        {
            get { return GetPropertyOrDefaultType<ObservableQueue<MangaChapterDownload>>(x => this.Downloads); }
            set { SetProperty<ObservableQueue<MangaChapterDownload>>(x => this.Downloads, value); }
        }

        public MangaChapterDownload SelectedItem
        {
            get { return GetPropertyOrDefaultType<MangaChapterDownload>(x => this.SelectedItem); }
            set { SetProperty(x => this.SelectedItem, value); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
        }

        public CrystalProperCommand CancelDownloadCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.CancelDownloadCommand); }
            set { SetProperty(x => this.CancelDownloadCommand, value); }
        }

    }
}
