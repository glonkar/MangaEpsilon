﻿using Crystal.Command;
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
using Crystal.Services;
using Crystal.Localization;
using System.Windows.Data;

namespace MangaEpsilon.ViewModel
{
    public class MangaDetailPageViewModel : BaseViewModel
    {
        public override void CloseViewModel()
        {
            DownloadsService.DownloadCompleted -= DownloadsService_DownloadCompleted;

            base.CloseViewModel();
        }

        public override void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            if (IsDesignMode) return;

            SetManga(argument);
        }

        private async void SetManga(KeyValuePair<string, object>[] argument)
        {
            IsBusy = true;

            MangaEpsilon.Manga.Base.Manga selectedManga = (Manga.Base.Manga)argument[0].Value;

            DownloadsService.DownloadCompleted += DownloadsService_DownloadCompleted;

            //create a copy since directly binding to the Chapters collection slows the app down if there is 500+ entries.
            Manga = new Manga.Base.Manga();
            Manga.MangaName = selectedManga.MangaName;
            Manga.ID = selectedManga.ID;
            Manga.Description = selectedManga.Description;
            Manga.Author = selectedManga.Author;
            Manga.BookImageUrl = selectedManga.BookImageUrl;
            Manga.Status = selectedManga.Status;

            ViewTitle = Manga.MangaName + " - " + Crystal.Localization.LocalizationManager.GetLocalizedValue("MangaDetailsTitle") + " - " +
                Crystal.Localization.LocalizationManager.GetLocalizedValue("MainApplicationTitle");

            //var firstEntry = Yukihyo.MAL.MyAnimeListAPI.Search(Manga.MangaName, Yukihyo.MAL.MALSearchType.manga).First(x => x.Title.ToLower() == Manga.MangaName.ToLower());

            MangaChapters = new PaginatedObservableCollection<ChapterEntry>(selectedManga.Chapters);
            MangaChapters.PageSize = 30;
            if (selectedManga.Chapters.Count > MangaChapters.PageSize && MangaChapters.CanPageUp == false)
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            MangaIsFavorited = await FavoritesService.IsMangaFavoritedAsync(Manga);

            InitializeCommands();

            IsBusy = false;

            Manga.Categories = selectedManga.Categories;

            if (Yukihyo.MAL.Utils.NetworkUtils.IsConnectedToInternet())
            {
                if (Manga == null) return;

                await GetUpdatedInfo();

                if (MangaIsFavorited)
                    FavoritesService.CheckAndDownload(selectedManga);

                var reviews = GetReviews();
                var related = GetRelatedManga();

                //await Task.WhenAll(reviews, related).ConfigureAwait(false);
                await reviews.ConfigureAwait(false);
                await related.ConfigureAwait(false);
            }

        }

        private void InitializeCommands()
        {
            MangaAddFavoriteCommand = CommandManager.CreateProperCommand(async (o) =>
            {
                if (MangaFavoritedCommandIsBusy) return;
                Task.Run(() =>
                {
                    MangaFavoritedCommandIsBusy = true;
                    FavoritesService.AddManga(Manga);

                    MangaIsFavorited = true;
                    MangaFavoritedCommandIsBusy = false;
                });
            }, (o) =>
            {
                if (Manga != null)
                    return !FavoritesService.IsMangaFavorited(Manga) && !MangaFavoritedCommandIsBusy;
                else
                    return false;
            });
            MangaRemoveFavoriteCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (MangaFavoritedCommandIsBusy) return;
                Task.Run(() =>
                    {
                        MangaFavoritedCommandIsBusy = true;
                        FavoritesService.RemoveManga(Manga);

                        MangaIsFavorited = false;
                        MangaFavoritedCommandIsBusy = false;
                    });
            }, (o) =>
            {
                if (Manga != null)
                    return FavoritesService.IsMangaFavorited(Manga) && !MangaFavoritedCommandIsBusy;
                else
                    return false;
            });

            OpenMangaChapterCommand = CommandManager.CreateProperCommand(x =>
            {
                ChapterEntry selectedChapter = x as ChapterEntry;

                if ((!LibraryService.Contains(selectedChapter) && Yukihyo.MAL.Utils.NetworkUtils.IsConnectedToInternet())
                    || LibraryService.Contains(selectedChapter))
                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", selectedChapter));
                else
                    ServiceManager.Resolve<IMessageBoxService>().ShowMessage(LocalizationManager.GetLocalizedValue("NoInternetConnectionTitle"),
                        LocalizationManager.GetLocalizedValue("NoInternetConnectionMsg"));
            }, (o) => o != null && o is ChapterBase);

            MangaDownloadCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    var manga = chapter.ParentManga;


                    if (SelectedChapterItems.Length == 1 && SelectedChapterItems[0] == chapter)
                        Messenger.PushMessage(this, "MangaChapterDownload", chapter);
                    else
                        foreach (ChapterEntry chap in SelectedChapterItems)
                            Messenger.PushMessage(this, "MangaChapterDownload", chap);
                }
            }, (o) =>
                o != null && o is ChapterEntry && !LibraryService.Contains((ChapterEntry)o) && SelectedChapterItems != null && Yukihyo.MAL.Utils.NetworkUtils.IsConnectedToInternet());

            BeginningChapterPageCommand = CommandManager.CreateProperCommand((o) =>
            {
                MangaChapters.CurrentPage = 0;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }, (o) =>
            {
                if (MangaChapters != null)
                    return this.MangaChapters.CanPageDown;
                else
                    return false;
            });
            EndingChapterPageCommand = CommandManager.CreateProperCommand((o) =>
            {
                MangaChapters.CurrentPage = MangaChapters.MaxPageIndex;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }, (o) =>
            {
                if (MangaChapters != null)
                    return this.MangaChapters.CanPageUp;
                else
                    return false;
            });
            NextChapterPageCommand = CommandManager.CreateProperCommand((o) =>
            {
                MangaChapters.CurrentPage++;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }, (o) =>
            {
                if (MangaChapters != null)
                    return MangaChapters.CanPageUp;
                else
                    return false;
            });
            PreviousChapterPageCommand = CommandManager.CreateProperCommand((o) =>
            {
                MangaChapters.CurrentPage--;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }, (o) =>
            {
                if (MangaChapters != null)
                    return MangaChapters.CanPageDown;
                else
                    return false;
            });
            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                var manga = ((Manga.Base.Manga)o);

                var win = ViewModelOperations.FindWindow((vm) => vm.ContainsProperty("Manga") &&
                    ((Manga.Base.Manga)vm.GetProperty("Manga")).MangaName == manga.MangaName); //checks if its already open. probably not MVVM-ish.

                if (win == null)
                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                else
                    win.Focus();

            }, (o) => o != null && o is Manga.Base.Manga);
        }

        async void DownloadsService_DownloadCompleted(ChapterLight download)
        {
            if (download.ParentManga.MangaName == Manga.MangaName)
            {
                var selected = SelectedChapterItem;
                await UIDispatcher.InvokeAsync(() =>
                    {
                        MangaChapters.Refresh();
                        //CollectionViewSource.GetDefaultView(MangaChapters).Refresh();

                        SelectedChapterItem = selected; //just to be safe, do this on the dispatcher thread
                    });
            }
        }

        private async Task GetReviews()
        {
            if (Manga == null) return;
            try
            {
                IsError_Reviews = false;
                IsBusy_Reviews = true;
                Reviews = await MAL.MALReviewGrabber.GetReviews(Manga.MangaName).ConfigureAwait(false);
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
                    if (Manga == null) return;

                    var results = Yukihyo.MAL.MyAnimeListAPI.Search(Manga.MangaName, Yukihyo.MAL.MALSearchType.manga);
                    var results2 = results.Where(x =>
                            x.Title.ToLower().Trim() != Manga.MangaName.ToLower().Trim() &&
                            App.MangaSource.AvailableManga.Any(y =>
                                LevenshteinDistance.Compute(y.MangaName.ToLower().Trim(), x.Title.ToLower().Trim()) <= 2)).ToArray();


                    RelatedManga = new ObservableCollection<Manga.Base.Manga>(
                        results2.Select(x =>
                            App.MangaSource.AvailableManga.Find(y =>
                                LevenshteinDistance.Compute(y.MangaName, x.Title) <= 2 && y.MangaName != Manga.MangaName))
                            .Where(x => x != null).Distinct());

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

            if (Manga == null) return;

            Manga.Description = newManga.Description;
            Manga.Author = newManga.Author;
            Manga.Status = newManga.Status;
            Manga.Categories = newManga.Categories;

            UIDispatcher.BeginInvoke(new EmptyDelegate(() =>
            {
                if (MangaChapters.Count <= newManga.Chapters.Count)
                    MangaChapters = new PaginatedObservableCollection<ChapterEntry>(newManga.Chapters, 40);
            }));
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

        public CrystalProperCommand BeginningChapterPageCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.BeginningChapterPageCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.BeginningChapterPageCommand, value); }
        }
        public CrystalProperCommand EndingChapterPageCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.EndingChapterPageCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.EndingChapterPageCommand, value); }
        }
        public CrystalProperCommand PreviousChapterPageCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.PreviousChapterPageCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.PreviousChapterPageCommand, value); }
        }
        public CrystalProperCommand NextChapterPageCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.NextChapterPageCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.NextChapterPageCommand, value); }
        }
        public CrystalProperCommand OpenMangaChapterCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.OpenMangaChapterCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.OpenMangaChapterCommand, value); }
        }
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

                if (value != null)
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public ChapterEntry[] SelectedChapterItems
        {
            get { return (ChapterEntry[])GetProperty(x => this.SelectedChapterItems); }
            set
            {
                SetProperty(x => this.SelectedChapterItems, value);

                if (value != null)
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

        public bool MangaIsFavorited
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.MangaIsFavorited); }
            set { SetProperty<bool>(x => this.MangaIsFavorited, value); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
        }
        public bool MangaFavoritedCommandIsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.MangaFavoritedCommandIsBusy); }
            set { SetProperty<bool>(x => this.MangaFavoritedCommandIsBusy, value); }
        }
        public CrystalProperCommand MangaAddFavoriteCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.MangaAddFavoriteCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.MangaAddFavoriteCommand, value); }
        }
        public CrystalProperCommand MangaRemoveFavoriteCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.MangaRemoveFavoriteCommand); }
            set { SetProperty<CrystalProperCommand>(x => this.MangaRemoveFavoriteCommand, value); }
        }
    }
}
