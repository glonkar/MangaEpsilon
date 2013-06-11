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
    public class MainWindowTodaysReleasesViewModel: BaseViewModel
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

            var latestMangas = await App.MangaSource.GetNewReleasesOfToday(8);

            IsBusy = false;

            NewReleasesToday = new ObservableCollection<ChapterEntry>();

            foreach (var manga in latestMangas)
            {
                //simulate real-time adding of items
                await Task.Delay(200);
                NewReleasesToday.Add(manga);
            }

            MangaClickCommand = CommandManager.CreateCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                }
            });
        }

        public ObservableCollection<ChapterEntry> NewReleasesToday
        {
            get
            {
                return GetPropertyOrDefaultType<ObservableCollection<ChapterEntry>>(x => this.NewReleasesToday);
            }
            set { SetProperty<object>(x => this.NewReleasesToday, value); }
        }

        public CrystalCommand MangaClickCommand { get; set; }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
