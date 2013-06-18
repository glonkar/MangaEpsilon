using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Command;
using Crystal.Core;
using Crystal.Messaging;
using Crystal.Navigation;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowTodaysReleasesViewModel : BaseViewModel
    {
        public MainWindowTodaysReleasesViewModel()
        {
            if (IsDesignMode) return;

            Initialize();
        }

        private async void Initialize()
        {
            IsBusy = true;
            await Task.WhenAll(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.

            if (App.MangaSourceInitializationTask.IsCanceled) return; //MainWindowAmrykidsFavoritesViewModel will show a messagebox about this rare circumstance.


            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                }
            }, (o) =>
                o != null && o is ChapterEntry);
            MangaInfoCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    var manga = chapter.ParentManga;

                    Predicate<ICrystalViewModel> pred = (vm) => { return vm.ContainsProperty("Manga") && ((Manga.Base.Manga)vm.GetProperty("Manga")).MangaName == manga.MangaName; };
                    var win = ViewModelOperations.FindWindow(pred); //checks if its already open. probably not MVVM-ish.

                    if (win == null)
                        NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                    else
                        win.Focus();
                }
            }, (o) =>
                o != null && o is ChapterEntry);
            MangaDownloadCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    var manga = chapter.ParentManga;

                    Messenger.PushMessage(this, "MangaChapterDownload", chapter);
                }
            }, (o) => o != null && o is ChapterEntry && !LibraryService.Contains((ChapterEntry)o));

            RetryCommand = CommandManager.CreateProperCommand(async (o) => await GetNewReleases(), (o) => IsError);

            await GetNewReleases();

        }

        private async Task GetNewReleases()
        {
            try
            {
                IsError = false;

                if (!IsBusy)
                    IsBusy = true;

                var latestMangas = await App.MangaSource.GetNewReleasesOfToday(12);

                NewReleasesToday = new ObservableCollection<ChapterEntry>();

                foreach (var manga in latestMangas)
                {
                    //simulate real-time adding of items
                    await Task.Delay(100);
                    NewReleasesToday.Add(manga);
                }

                IsError = false;
            }
            catch (Exception)
            {
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ChapterEntry SelectedEntry
        {
            get { return GetPropertyOrDefaultType<ChapterEntry>(x => this.SelectedEntry); }
            set { SetProperty<ChapterEntry>(x => this.SelectedEntry, value); }
        }

        public ObservableCollection<ChapterEntry> NewReleasesToday
        {
            get
            {
                return GetPropertyOrDefaultType<ObservableCollection<ChapterEntry>>(x => this.NewReleasesToday);
            }
            set
            {
                SetProperty<object>(x => this.NewReleasesToday, value);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public CrystalProperCommand MangaClickCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
        }
        public CrystalProperCommand MangaDownloadCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaDownloadCommand); }
            set { SetProperty(x => this.MangaDownloadCommand, value); }
        }
        public CrystalProperCommand MangaInfoCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaInfoCommand); }
            set { SetProperty(x => this.MangaInfoCommand, value); }
        }

        public CrystalProperCommand RetryCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.RetryCommand); }
            set { SetProperty(x => this.RetryCommand, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }

        public bool IsError
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsError); }
            set { SetProperty<bool>(x => this.IsError, value); }
        }
    }
}
