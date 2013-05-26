using Crystal.Core;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;
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
                ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, CurrentPageIndex),
            });
            Pages[0].Image = LoadImgUrl(Pages[0].ImageUrl);

            IsBusy = false;

            for (int i = 1; i < chapter.TotalPages; i++)
            {
                var page = new MangaChapterPage(chapter);

                page.Index = i;

                Pages.Add(page);

                bool shouldBeLoaded = i <= Math.Min(chapter.TotalPages, 5);

                page.ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, i);

                if (shouldBeLoaded)
                    page.Image = LoadImgUrl(page.ImageUrl);
            }

            RaisePropertyChanged(x => this.Pages);
        }

        private async void GetNextBatchOfPages()
        {
            IsBusy = true;

            await Task.Delay(1);

            for (int i = CurrentPageIndex; i < Math.Min(chapter.TotalPages, CurrentPageIndex + 3); i++)
            {
                Pages[i].Image = LoadImgUrl(Pages[i].ImageUrl);
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
                if (Pages.Count > 0)
                    if (Pages[value].Image == null && IsBusy == false)
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

        private BitmapImage LoadImgUrl(string url)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url);
            bi.EndInit();

            return bi;
        }
    }
}
