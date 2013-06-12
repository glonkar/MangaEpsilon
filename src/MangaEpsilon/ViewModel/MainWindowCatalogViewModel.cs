using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Core;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowCatalogViewModel: BaseViewModel
    {
        public MainWindowCatalogViewModel()
        {
            if (IsDesignMode) return;

            Initialize();
        }

        private async void Initialize()
        {
            IsBusy = true;
            await Task.WhenAny(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.

            AvailableMangas = new PaginatedObservableCollection<Manga.Base.Manga>(App.MangaSource.AvailableManga);
            AvailableMangas.PageSize = 30;

            IsBusy = false;
        }

        public PaginatedObservableCollection<Manga.Base.Manga> AvailableMangas { get; private set; }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
