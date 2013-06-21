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
using System.Collections.ObjectModel;

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

            MangaEpsilon.Manga.Base.Manga selectedManga = (Manga.Base.Manga)argument[0].Value;

            //create a copy since directly binding to the Chapters collection slows the app down if there is 500+ entries.
            Manga = new Manga.Base.Manga();
            Manga.MangaName = selectedManga.MangaName;
            Manga.ID = selectedManga.ID;
            Manga.Description = selectedManga.Description;
            Manga.Author = selectedManga.Author;
            Manga.BookImageUrl = selectedManga.BookImageUrl;
            Manga.Status = selectedManga.Status;

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


                    if (SelectedChapterItems.Length == 1 && SelectedChapterItems[0] == chapter)
                        Messenger.PushMessage(this, "MangaChapterDownload", chapter);
                    else
                        foreach(ChapterEntry chap in SelectedChapterItems)
                            Messenger.PushMessage(this, "MangaChapterDownload", chap);
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
            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                var manga = ((Manga.Base.Manga)o);

                var win = ViewModelOperations.FindWindow((vm) => vm.ContainsProperty("Manga") && ((Manga.Base.Manga)vm.GetProperty("Manga")).MangaName == manga.MangaName); //checks if its already open. probably not MVVM-ish.

                if (win == null)
                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                else
                    win.Focus();

            }, (o) => o != null && o is Manga.Base.Manga);

            IsBusy = false;

            Task.Run(async () =>
                {
                    Manga.Categories = selectedManga.Categories;

                    if (Yukihyo.MAL.Utils.NetworkUtils.IsConnectedToInternet())
                    {
                        await GetUpdatedInfo();

                        GetReviews();
                        GetRelatedManga();
                    }
                });

        }

        private async Task GetReviews()
        {
            try
            {
                IsError_Reviews = false;
                IsBusy_Reviews = true;
                Reviews = await MAL.MALReviewGrabber.GetReviews(Manga.MangaName);
                IsBusy_Reviews = false;
            }
            catch (Exception)
            {
                IsBusy_Reviews = false;
                IsError_Reviews = true;
            }
        }

        private async Task GetRelatedManga()
        {
            try
            {
                IsError_RelatedManga = false;
                IsBusy_RelatedManga = true;

                await Task.Run(() =>
                {
                    var results = Yukihyo.MAL.MyAnimeListAPI.Search(Manga.MangaName, Yukihyo.MAL.MALSearchType.manga);
                    var results2 = results.Where(x =>
                            x.Title.ToLower().Trim() != Manga.MangaName.ToLower().Trim() &&
                            App.MangaSource.AvailableManga.Any(y =>
                                LevenshteinDistance.Compute(y.MangaName.ToLower().Trim(), x.Title.ToLower().Trim()) <= 2)).ToArray();


                    RelatedManga = new ObservableCollection<Manga.Base.Manga>(
                        results2.Select(x =>
                            App.MangaSource.AvailableManga.Find(y =>
                                LevenshteinDistance.Compute(y.MangaName, x.Title) <= 2 && y.MangaName != Manga.MangaName))
                            .Where(x => x != null));

                });
                IsBusy_RelatedManga = false;
            }
            catch (Exception)
            {
                IsBusy_RelatedManga = false;
                IsError_RelatedManga = true;
            }
        }

        private async Task GetUpdatedInfo()
        {
            var newManga = await App.MangaSource.GetMangaInfo(Manga.MangaName, false); //Get fresh, updated information.
            Manga.Description = newManga.Description;
            Manga.Author = newManga.Author;
            Manga.Status = newManga.Status;
            Manga.Categories = newManga.Categories;

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

        public CrystalProperCommand MangaClickCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
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

        public ChapterEntry[] SelectedChapterItems
        {
            get { return (ChapterEntry[])GetProperty(x => this.SelectedChapterItems); }
            set
            {
                SetProperty(x => this.SelectedChapterItems, value);
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

        public bool IsBusy_Reviews
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy_Reviews); }
            set { SetProperty<bool>(x => this.IsBusy_Reviews, value); }
        }
        public bool IsError_Reviews
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsError_Reviews); }
            set { SetProperty<bool>(x => this.IsError_Reviews, value); }
        }

        public List<MAL.MALReview> Reviews
        {
            get { return GetPropertyOrDefaultType<List<MAL.MALReview>>(x => this.Reviews); }
            set { SetProperty(x => this.Reviews, value); }
        }

        public bool IsBusy_RelatedManga
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy_RelatedManga); }
            set { SetProperty<bool>(x => this.IsBusy_RelatedManga, value); }
        }
        public bool IsError_RelatedManga
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsError_RelatedManga); }
            set { SetProperty<bool>(x => this.IsError_RelatedManga, value); }
        }

        public ObservableCollection<Manga.Base.Manga> RelatedManga
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Manga.Base.Manga>>(x => this.RelatedManga); }
            set { SetProperty(x => this.RelatedManga, value); }
        }
    }
}
