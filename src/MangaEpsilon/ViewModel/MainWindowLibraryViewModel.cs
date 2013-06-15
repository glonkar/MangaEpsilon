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
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowLibraryViewModel: BaseViewModel
    {
        public MainWindowLibraryViewModel()
        {
            if (!LibraryService.IsInitialized)
                LibraryService.Initialize();

            LibraryService.LibraryItemAdded += LibraryService_LibraryItemAdded;

            LibraryItems = new ObservableCollection<Manga.Base.ChapterLight>(LibraryService.LibraryCollection.Select(x => x.Item1));

            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterLight)
                {
                    var chapter = ((ChapterLight)o);
                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                }
            }, (o) =>
                o != null && o is ChapterLight);
        }

        void LibraryService_LibraryItemAdded(Tuple<Manga.Base.ChapterLight, string> tuple)
        {
            
        }

        public ObservableCollection<Manga.Base.ChapterLight> LibraryItems
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Manga.Base.ChapterLight>>(x => this.LibraryItems); }
            set { SetProperty(x => this.LibraryItems, value); }
        }

        public CrystalProperCommand MangaClickCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
        }

        public ChapterLight SelectedEntry
        {
            get { return GetPropertyOrDefaultType<ChapterLight>(x => this.SelectedEntry); }
            set { SetProperty<ChapterLight>(x => this.SelectedEntry, value); }
        }

    }
}
