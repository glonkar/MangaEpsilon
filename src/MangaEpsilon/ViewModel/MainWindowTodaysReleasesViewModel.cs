using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;
using MangaEpsilon.Manga.Base;

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
            await Task.WhenAny(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.

            var latestMangas = await App.MangaSource.GetNewReleasesOfToday(12);

            IsBusy = false;

            NewReleasesToday = new ObservableCollection<ChapterEntry>();

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

                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                }
            }, (o) =>
                o != null && o is ChapterEntry);

            foreach (var manga in latestMangas)
            {
                //simulate real-time adding of items
                await Task.Delay(100);
                NewReleasesToday.Add(manga);
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
        public CrystalProperCommand MangaInfoCommand { get; set; }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
