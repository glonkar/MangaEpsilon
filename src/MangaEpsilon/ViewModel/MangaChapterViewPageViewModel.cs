using Crystal.Core;
using MangaEpsilon.Manga.Base;
//using MangaEpsilon.Model;
using MangaEpsilon.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MangaEpsilon.Services;
using Crystal.Localization;
using Crystal.Messaging;
using Crystal.Command;

namespace MangaEpsilon.ViewModel
{
    public class MangaChapterViewPageViewModel : BaseViewModel
    {
        public override void OnNavigatedTo(params KeyValuePair<string, object>[] argument)
        {
            ChapterBase entry = (ChapterBase)argument[0].Value;

            ChapterName = entry.Name;

            SaveZoomPosition = App.SaveZoomPosition;

            GetMangaPages(entry);

            base.OnNavigatedTo(argument);
        }

        private ChapterLight chapter = null;
        private async void GetMangaPages(ChapterBase entry)
        {
            IsBusy = true;
            Pages = new ObservableCollection<Uri>();

            DownloadChapterCommand = CommandManager.CreateProperCommand((o) =>
            {
                Messenger.PushMessage(this, "MangaChapterDownload", (ChapterEntry)entry);
            }, (o) => !LibraryService.Contains((dynamic)entry));

            if (LibraryService.Contains((dynamic)entry))
            {
                string path = string.Empty;
                if (entry is ChapterEntry)
                {
                    chapter = LibraryService.GetDownloadedChapterLightFromEntry((ChapterEntry)entry);
                    path = LibraryService.GetDownloadedChapterLightPathFromEntry((ChapterEntry)entry);
                }
                else
                {
                    chapter = (ChapterLight)entry;
                    path = LibraryService.GetPath((ChapterLight)entry);
                }

                foreach (var page in chapter.PagesUrls)
                {
                    var url = new Uri(page.ToString());

                    var filename = url.Segments.Last();

                    Pages.Add(new Uri(path + filename));
                }
            }
            else
            {
                if (entry.ParentManga.Chapters.Count == 0)
                    entry.ParentManga = await App.MangaSource.GetMangaInfo(entry.ParentManga.MangaName, false);

                if (entry is ChapterEntry)
                    chapter = await App.MangaSource.GetChapterLight((ChapterEntry)entry);
                else if (entry is ChapterLight)
                    chapter = (ChapterLight)entry;
                else
                    throw new Exception();

                //Pages.Add(new Uri(await App.MangaSource.GetChapterPageImageUrl(chapter, CurrentPageIndex)));


                for (int i = 0; i < chapter.TotalPages; i++)
                {
                    Pages.Add(new Uri(await App.MangaSource.GetChapterPageImageUrl(chapter, i)));
                }

                RaisePropertyChanged(x => this.Pages);
            }

            CurrentPageIndex = 0;

            IsBusy = false;
        }

        private async void GetNextBatchOfPages()
        {
            IsBusy = true;

            await Task.Delay(1);

            for (int i = CurrentPageIndex; i < Math.Min(chapter.TotalPages, CurrentPageIndex + 3); i++)
            {
                if (Pages[i] == null)
                    Pages[i] = new Uri(await App.MangaSource.GetChapterPageImageUrl(chapter, i));
            }

            await Task.Delay(1000);

            IsBusy = false;
        }

        public string ChapterName
        {
            get { return GetPropertyOrDefaultType<string>(x => this.ChapterName); }
            set { SetProperty(x => this.ChapterName, value); }
        }

        public int CurrentPageIndex
        {
            get { return GetPropertyOrDefaultType<int>(x => this.CurrentPageIndex); }
            set
            {
                SetProperty(x => this.CurrentPageIndex, value);

                CurrentPage = CurrentPageIndex + 1;

                if (chapter != null)
                    if (!LibraryService.Contains(chapter))
                        if (Pages != null)
                            if (Pages.Count > 0)
                                if (Pages.Count > value + 1)
                                    if (Pages[value + 1] == null && IsBusy == false)
                                        GetNextBatchOfPages();

                CurrentPageLabelString = String.Format(LocalizationManager.GetLocalizedValue("MangaChapterViewCurrentPageLabelFormatString"),
                    CurrentPage.ToString(), Pages.Count.ToString());
            }
        }
        public int CurrentPage
        {
            get { return GetPropertyOrDefaultType<int>(x => this.CurrentPage); }
            set
            {
                SetProperty(x => this.CurrentPage, value);
            }
        }

        public ObservableCollection<Uri> Pages
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Uri>>(x => this.Pages); }
            set { SetProperty(x => this.Pages, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }

        public string CurrentPageLabelString
        {
            get{ return GetPropertyOrDefaultType<string>(x => this.CurrentPageLabelString);}
            set { SetProperty(x => this.CurrentPageLabelString, value);}
        }

        public bool? SaveZoomPosition
        {
            get { return GetPropertyOrDefaultType<bool?>(x => this.SaveZoomPosition); }
            set { SetProperty<bool?>(x => this.SaveZoomPosition, value); Messenger.PushMessage(this, "MangaViewerSaveZoomPosition", SaveZoomPosition); }
        }

        public CrystalProperCommand DownloadChapterCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.DownloadChapterCommand); }
            set { SetProperty(x => this.DownloadChapterCommand, value); }
        }

        public override void CloseViewModel()
        {
            base.CloseViewModel();
        }

        private async Task<BitmapImage> LoadImgUrl(string url, bool block = false)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url);
            bi.EndInit();

            if (block)
                await bi.WaitForDownloadCompletion();

            return bi;
        }
    }
}
