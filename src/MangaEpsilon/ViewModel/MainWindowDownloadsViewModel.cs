﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Crystal.Core;
using Crystal.Messaging;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowDownloadsViewModel : BaseViewModel
    {
        public MainWindowDownloadsViewModel()
        {
            Downloads = new ObservableQueue<MangaChapterDownload>();
            RegisterForMessages("MangaChapterDownload");
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

            if (!LibraryService.Contains(chap))
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


                    var downloadPath = LibraryService.LibraryDirectory + download.Chapter.ParentManga.MangaName + "\\" + download.Chapter.VolumeNumber.ToString() + "\\";

                    if (!Directory.Exists(downloadPath))
                        Directory.CreateDirectory(downloadPath);

                    download.MaxProgress = download.Chapter.PagesUrls.Count;

                    bool error = false;

                    using (WebClient wc = new WebClient())
                    {

                        foreach (var pageUrl in download.Chapter.PagesUrls)
                        {
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
                                await Task.Delay(500);

                                download.Progress++;

                                Messenger.PushMessage(this, "UpdateMainWindowProgress", (Convert.ToDouble(download.Progress) / Convert.ToDouble(download.MaxProgress)));
                            }
                            else
                            {
                                Messenger.PushMessage(this, "UpdateMainWindowState", System.Windows.Shell.TaskbarItemProgressState.None);
                                Notifications.NotificationsService.AddNotification("Download Failed!", download.Chapter.Name + " has failed to download.");
                                return;
                            }
                        }
                    }

                    if (!error)
                    {
                        LibraryService.AddLibraryItem(new Tuple<ChapterLight, string>(download.Chapter, downloadPath));                    

                        Notifications.NotificationsService.AddNotification("Download Completed!", download.Chapter.Name + " Downloaded");
                    }
                    
                    Downloads.Dequeue();
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

    }
}
