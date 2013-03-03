using Crystal.Core;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            App.ProgressIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            base.OnNavigatedFrom();
        }

        private async void GetMangaPages(ChapterEntry entry)
        {
            ChapterLight chapter = await App.MangaSource.GetChapterLight(entry, null);

            Pages = new ObservableCollection<MangaChapterPage>();

            Pages.Add(new MangaChapterPage(chapter)
            {
                Index = 0,
                ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, CurrentPageIndex)
            });

            for (int i = 1; i < chapter.TotalPages; i++)
            {
                var page = new MangaChapterPage(chapter);

                page.Index = i;
                //page.ImageUrl = await App.MangaSource.GetChapterPageImageUrl(chapter, CurrentPageIndex + 1);

                Pages.Add(page);
            }
        }

        public string ChapterName
        {
            get { return GetPropertyOrDefaultType<string>(x => this.ChapterName); }
            set { SetProperty(x => this.ChapterName, value); }
        }

        public int CurrentPageIndex
        {
            get { return GetPropertyOrDefaultType<int>(x => this.CurrentPageIndex); }
            set { SetProperty(x => this.CurrentPageIndex, value); }
        }

        public ObservableCollection<MangaChapterPage> Pages
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<MangaChapterPage>>(x => this.Pages); }
            set { SetProperty(x => this.Pages, value); }
        }
    }
}
