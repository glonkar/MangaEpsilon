using Crystal.Core;
using MangaEpsilon.Manga.Base;
//using MangaEpsilon.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Crystal.Localization;
using Crystal.Messaging;
using Crystal.Command;

namespace MangaEpsilon.ViewModel
{
#if !WINDOWS_PHONE
    using MangaEpsilon.Services;
    using MangaEpsilon.Extensions;
#else
    using MangaEpsilonWP;
    using MangaEpsilonWP.Reimps;
    using Microsoft.Phone;
#endif
    public class MangaChapterViewPageViewModel : BaseViewModel
    {
#if WINDOWS_PHONE
        public override void OnNavigatedTo(KeyValuePair<string, string>[] argument = null)
        {
            Phone_Initialize(argument);

            base.OnNavigatedTo(argument);
        }

        private async void Phone_Initialize(KeyValuePair<string, string>[] argument)
        {
            IsBusy = true;

            ChapterBase entry = (await App.MangaSource.GetMangaInfo(argument[0].Value)).Chapters.First(x => x.Name == argument[1].Value);

            ChapterName = entry.Name;

            GetMangaPages(entry);
        }
#else
        public override void OnNavigatedTo(params KeyValuePair<string, object>[] argument)
        {
            ChapterBase entry = (ChapterBase)argument[0].Value;

            ChapterName = entry.Name;

            SaveZoomPosition = App.SaveZoomPosition;

            GetMangaPages(entry);

            base.OnNavigatedTo(argument);
        }
#endif

        private ChapterLight chapter = null;
        private async void GetMangaPages(ChapterBase entry)
        {
            IsBusy = true;

            ChapterName = entry.Name;

#if !WINDOWS_PHONE
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
#endif
            if (entry.ParentManga.Chapters.Count == 0)
                entry.ParentManga = await App.MangaSource.GetMangaInfo(entry.ParentManga.MangaName, false);

            if (entry is ChapterEntry)
                chapter = await App.MangaSource.GetChapterLight((ChapterEntry)entry);
            else if (entry is ChapterLight)
                chapter = (ChapterLight)entry;
            else
                throw new Exception();

            //Pages.Add(new Uri(await App.MangaSource.GetChapterPageImageUrl(chapter, CurrentPageIndex)));


#if !WINDOWS_PHONE
                for (int i = 0; i < chapter.TotalPages; i++)
                {
                    Pages.Add(new Uri(await App.MangaSource.GetChapterPageImageUrl(chapter, i)));
                }
#else
            object[] pageArray = new object[chapter.TotalPages];

            for (int i = 0; i < chapter.TotalPages; i++)
            {
                var url = await App.MangaSource.GetChapterPageImageUrl(chapter, i).ConfigureAwait(false);
                using (HttpClient http = new HttpClient())
                {
                    var stream = await http.GetStreamAsync(url).ConfigureAwait(false);
                    await Dispatcher.InvokeAsync(() =>
                        {
                            pageArray[i] = PictureDecoder.DecodeJpeg(stream, 470, 500);
                        });
                    stream.Dispose();
                }
            }

            Pages = new ObservableCollection<object>(pageArray);
#endif

            RaisePropertyChanged(x => this.Pages);
#if !WINDOWS_PHONE    
            }
#endif

            CurrentPageIndex = 0;

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

#if !WINDOWS_PHONE
                if (chapter != null)
                    if (!LibraryService.Contains(chapter))
                        if (Pages != null)
                            if (Pages.Count > 0)
                                if (Pages.Count > value + 1)
                                    if (Pages[value + 1] == null && IsBusy == false)
                                        GetNextBatchOfPages();

                CurrentPageLabelString = String.Format(LocalizationManager.GetLocalizedValue("MangaChapterViewCurrentPageLabelFormatString"),
                    CurrentPage.ToString(), Pages.Count.ToString());
#endif
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

#if !WINDOWS_PHONE
        public ObservableCollection<Uri> Pages
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Uri>>(x => this.Pages); }
            set { SetProperty(x => this.Pages, value); }
        }
#else
        public ObservableCollection<object> Pages
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<object>>(x => this.Pages); }
            set { SetProperty(x => this.Pages, value); }
        }
#endif

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }

        public string CurrentPageLabelString
        {
            get { return GetPropertyOrDefaultType<string>(x => this.CurrentPageLabelString); }
            set { SetProperty(x => this.CurrentPageLabelString, value); }
        }

        public bool? SaveZoomPosition
        {
            get { return GetPropertyOrDefaultType<bool?>(x => this.SaveZoomPosition); }
            set { SetProperty<bool?>(x => this.SaveZoomPosition, value); Messenger.PushMessage(this, "MangaViewerSaveZoomPosition", SaveZoomPosition); }
        }

#if !WINDOWS_PHONE
        public CrystalProperCommand DownloadChapterCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.DownloadChapterCommand); }
            set { SetProperty(x => this.DownloadChapterCommand, value); }
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
#endif

        public override void CloseViewModel()
        {
            base.CloseViewModel();
        }
    }
}
