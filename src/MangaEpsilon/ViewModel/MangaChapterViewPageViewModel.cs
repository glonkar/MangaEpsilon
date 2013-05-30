using Crystal.Core;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;
using MangaEpsilon.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MangaEpsilon.ViewModel
{
    public class MangaChapterViewPageViewModel : BaseViewModel
    {
        public override void OnNavigatedTo(params KeyValuePair<string, object>[] argument)
        {
            ChapterEntry entry = (ChapterEntry)argument[0].Value;

            ChapterName = entry.Name;

            GetMangaPages(entry);

            base.OnNavigatedTo(argument);
        }

        public override void OnNavigatedFrom()
        {
            //App.ProgressIndicator.Visibility = Visibility.Collapsed;

            base.OnNavigatedFrom();
        }

        private ChapterLight chapter = null;
        private async void GetMangaPages(ChapterEntry entry)
        {
            IsBusy = true;

            chapter = await App.MangaSource.GetChapterLight(entry, null);

            Pages = new ObservableCollection<MangaChapterPage>();

            Pages.Add(new MangaChapterPage(chapter)
            {
                Index = 0,
            });
            Pages[0].ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, CurrentPageIndex);

            await Task.Yield();

            Pages[0].Image = await LoadImgUrl(Pages[0].ImageUrl, true);

            CurrentPageIndex = 0;

            IsBusy = false;

            for (int i = 1; i < chapter.TotalPages; i++)
            {
                var page = new MangaChapterPage(chapter);

                page.Index = i;

                Pages.Add(page);

                bool shouldBeLoaded = i <= Math.Min(chapter.TotalPages, 5);

                page.ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, i);

                if (shouldBeLoaded)
                    page.Image = await LoadImgUrl(page.ImageUrl, true);
            }

            RaisePropertyChanged(x => this.Pages);
        }

        private async void GetNextBatchOfPages()
        {
            IsBusy = true;

            await Task.Delay(1);

            for (int i = CurrentPageIndex; i < Math.Min(chapter.TotalPages, CurrentPageIndex + 3); i++)
            {
                if (Pages[i].ImageUrl == null)
                    Pages[i].ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, i);

                Pages[i].Image = await LoadImgUrl(Pages[i].ImageUrl, true);
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

                if (Pages != null)
                    if (Pages.Count > 0)
                        if (Pages.Count > value + 1)
                            if (Pages[value + 1].Image == null && IsBusy == false)
                                GetNextBatchOfPages();
            }
        }

        public ObservableCollection<MangaChapterPage> Pages
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<MangaChapterPage>>(x => this.Pages); }
            set { SetProperty(x => this.Pages, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
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
