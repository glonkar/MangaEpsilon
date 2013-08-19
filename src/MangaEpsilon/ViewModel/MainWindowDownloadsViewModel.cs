using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Crystal.Command;
using Crystal.Core;
using Crystal.Localization;
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

            downloadsCollectionView = CollectionViewSource.GetDefaultView(Downloads);
            downloadsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Status"));

            CancelDownloadCommand = CommandManager.CreateProperCommand(async (o) =>
            {
                MangaChapterDownload[] downloads = ((IList)o).OfType<MangaChapterDownload>().ToArray();

                for (int i = 0; i < downloads.Length; ++i)
                {
                    MangaChapterDownload download = downloads[i];

                    if (download.Status == MangaChapterDownloadStatus.Downloading && Downloads.Peek() == download)
                    {
                        download = Downloads.Peek(); //set a reference.
                        download.Status = MangaChapterDownloadStatus.Canceled;
                        Downloads.Dequeue();
                    }
                    else
                    {
                        List<MangaChapterDownload> downloadsToRequeue = new List<MangaChapterDownload>();

                        while (Downloads.Count > 0)
                        {
                            var mangaChapter = Downloads.Dequeue();
                            if (mangaChapter != download)
                                downloadsToRequeue.Add(mangaChapter);
                        }

                        foreach (var d in downloadsToRequeue)
                            Downloads.Enqueue(d);
                    }
                }

                await Dispatcher.InvokeAsync(() =>
                    {
                        downloadsCollectionView.Refresh();
                    });
            },
            (o) =>
            {
                return o != null && o is IList;
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

        private ICollectionView downloadsCollectionView = null;

        private async void DownloadChapter(Crystal.Messaging.Message message)
        {
            ChapterEntry chapter = (ChapterEntry)message.Data;

            var chap = await App.MangaSource.GetChapterLight(chapter);

            if (!LibraryService.Contains(chap) && !Downloads.Any(x => x.Chapter.Name == chap.Name))
            {
                Downloads.Enqueue(new MangaChapterDownload(chap) { MaxProgress = chap.PagesUrls.Count, TotalFilesToDownload = chap.PagesUrls.Count, Status = MangaChapterDownloadStatus.Queued });

                RaisePropertyChanged(x => this.Downloads);

                try
                {
                    await UIDispatcher.InvokeAsync(() =>
                    {
                        downloadsCollectionView.Refresh();
                    });
                }
                catch (Exception)
                {
                }

                if (!App.DownloadsRunning)
                    DownloadAllQueuedChapters();
            }

            if (Downloads.Count == 1 && !firstSwitchTab)
            {
                Messenger.PushMessage(this, "SwitchTab", 3);
                firstSwitchTab = true;
            }
        }
        private bool firstSwitchTab = false;


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


                                if (download.Progress % 10 == 0)
                                    await Task.Delay(2000); //better throttle
                            }
                            else
                            {
                                Messenger.PushMessage(this, "UpdateMainWindowState", System.Windows.Shell.TaskbarItemProgressState.None);
                                Notifications.NotificationsService.AddNotification(LocalizationManager.GetLocalizedValue("DownloadFailedTitle"), string.Format(LocalizationManager.GetLocalizedValue("DownloadFailedMsg"), download.Chapter.Name));
                                break;
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

                            Notifications.NotificationsService.AddNotification(
                                LocalizationManager.GetLocalizedValue("DownloadCompletedTitle"),
                                string.Format(
                                    LocalizationManager.GetLocalizedValue("DownloadCompletedMsg"),
                                    download.Chapter.Name),
                                download.Chapter.ParentManga.BookImageUrl,
                                3000,
                                false,
                                Notifications.NotificationType.Information,
                                (x) =>
                                {
                                    var chapter = ((ChapterLight)download.Chapter);
                                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                                });
                            DownloadsService.RaiseDownloadCompleted(download.Chapter, this);
                        }
                    }
                    else if (download.Status == MangaChapterDownloadStatus.Canceled)
                    {
                        if (Directory.Exists(downloadPath))
                            Directory.Delete(downloadPath, true);
                    }

                    if (Downloads.Count > 0)
                        if (Downloads.Peek() == download)
                            Downloads.Dequeue();

                    DownloadsService.Downloads = Downloads;

                    await Task.Delay(1000);

                    UIDispatcher.BeginInvoke(new EmptyDelegate(() =>
                    {
                        downloadsCollectionView.Refresh();
                    }));
                }

                Messenger.PushMessage(this, "UpdateMainWindowState", System.Windows.Shell.TaskbarItemProgressState.None);

                DownloadsService.Downloads = Downloads;

                App.DownloadsRunning = false;
            });
        }

        public ObservableQueue<MangaChapterDownload> Downloads
        {
            get { return GetPropertyOrDefaultType<ObservableQueue<MangaChapterDownload>>(x => this.Downloads); }
            set { SetProperty<ObservableQueue<MangaChapterDownload>>(x => this.Downloads, value); DownloadsService.Downloads = Downloads; }
        }

        public MangaChapterDownload SelectedItem
        {
            get { return GetPropertyOrDefaultType<MangaChapterDownload>(x => this.SelectedItem); }
            set { SetProperty(x => this.SelectedItem, value); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
        }
        public IList SelectedItems
        {
            get { return GetPropertyOrDefaultType<IList>(x => this.SelectedItems); }
            set
            {
                SetProperty(x => this.SelectedItems, value);
                if (value.Count > 1)
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public CrystalProperCommand CancelDownloadCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.CancelDownloadCommand); }
            set { SetProperty(x => this.CancelDownloadCommand, value); }
        }

    }
}
