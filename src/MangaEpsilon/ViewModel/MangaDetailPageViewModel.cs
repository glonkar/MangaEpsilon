using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;
using MangaEpsilon.Manga.Base;
//using MangaEpsilon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Crystal.Messaging;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MangaDetailPageViewModel : BaseViewModel
    {
        public override void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            if (IsDesignMode) return;

            SetManga(argument);
        }

        private void SetManga(KeyValuePair<string, object>[] argument)
        {
            IsBusy = true;

            var selectedManga = (Manga.Base.Manga)argument[0].Value;

            //create a copy since directly binding to the Chapters collection slows the app down if there is 500+ entries.
            Manga = new Manga.Base.Manga();
            Manga.MangaName = selectedManga.MangaName;
            Manga.ID = selectedManga.ID;
            Manga.Description = selectedManga.Description;
            Manga.Author = selectedManga.Author;
            Manga.BookImageUrl = selectedManga.BookImageUrl;

            ViewTitle = Manga.MangaName + " - " + Crystal.Localization.LocalizationManager.GetLocalizedValue("MangaDetailsTitle") + " - " + Crystal.Localization.LocalizationManager.GetLocalizedValue("MainApplicationTitle");

            //var firstEntry = Yukihyo.MAL.MyAnimeListAPI.Search(Manga.MangaName, Yukihyo.MAL.MALSearchType.manga).First(x => x.Title.ToLower() == Manga.MangaName.ToLower());

            OpenMangaChapterCommand = CommandManager.CreateCommand(x =>
            {
                ChapterEntry selectedChapter = x as ChapterEntry;

                NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", selectedChapter));
            });

            MangaDownloadCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    var manga = chapter.ParentManga;

                    Messenger.PushMessage(this, "MangaChapterDownload", chapter);
                }
            }, (o) =>
                o != null && o is ChapterEntry && !LibraryService.Contains((ChapterEntry)o));

            MangaChapters = new PaginatedObservableCollection<ChapterEntry>(selectedManga.Chapters);
            MangaChapters.PageSize = 40;

            NextChapterPageCommand = CommandManager.CreateProperCommand((o) =>
            {
                MangaChapters.CurrentPage++;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }, (o) =>
            {
                return MangaChapters.CanPageUp;
            });
            PreviousChapterPageCommand = CommandManager.CreateProperCommand((o) =>
            {
                MangaChapters.CurrentPage--;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }, (o) =>
            {
                return MangaChapters.CanPageDown;
            });

            IsBusy = false;

            GetUpdatedInfo();

        }

        private async void GetUpdatedInfo()
        {
            var newManga = await App.MangaSource.GetMangaInfo(Manga.MangaName, false); //Get fresh, updated information.
            Manga.Description = newManga.Description;
            Manga.Author = newManga.Author;

            if (MangaChapters.Count != newManga.Chapters.Count)
                MangaChapters = new PaginatedObservableCollection<ChapterEntry>(newManga.Chapters, 40);
        }

        public Manga.Base.Manga Manga
        {
            get { return GetPropertyOrDefaultType<Manga.Base.Manga>(x => this.Manga); }
            set { SetProperty(x => this.Manga, value); }
        }

        public PaginatedObservableCollection<Manga.Base.ChapterEntry> MangaChapters
        {
            get { return GetPropertyOrDefaultType<PaginatedObservableCollection<Manga.Base.ChapterEntry>>(x => this.MangaChapters); }
            set { SetProperty(x => this.MangaChapters, value); }
        }

        public CrystalProperCommand PreviousChapterPageCommand { get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.PreviousChapterPageCommand); } set { SetProperty<CrystalProperCommand>(x => this.PreviousChapterPageCommand, value); } }
        public CrystalProperCommand NextChapterPageCommand { get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.NextChapterPageCommand); } set { SetProperty<CrystalProperCommand>(x => this.NextChapterPageCommand, value); } }
        public CrystalCommand OpenMangaChapterCommand { get; set; }
        public CrystalProperCommand MangaDownloadCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaDownloadCommand); }
            set { SetProperty(x => this.MangaDownloadCommand, value); }
        }

        public ChapterEntry SelectedChapterItem
        {
            get { return (ChapterEntry)GetProperty(x => this.SelectedChapterItem); }
            set
            {
                SetProperty(x => this.SelectedChapterItem, value);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ViewTitle
        {
            get { return GetPropertyOrDefaultType<string>(x => this.ViewTitle); }
            set { SetProperty(x => this.ViewTitle, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
