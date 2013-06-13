using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Crystal.Core;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowCatalogViewModel : BaseViewModel
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

            AvailableMangas = new ObservableCollection<Manga.Base.Manga>(App.MangaSource.AvailableManga); // I hope virtualization is on.
            // new PaginatedObservableCollection<Manga.Base.Manga>(App.MangaSource.AvailableManga);
            //AvailableMangas.PageSize = 30;

            CollectionViewSource.GetDefaultView(AvailableMangas).SortDescriptions.Add(new System.ComponentModel.SortDescription("MangaName", System.ComponentModel.ListSortDirection.Ascending));

            IsBusy = false;
        }

        public string SearchFilter
        {
            get { return GetPropertyOrDefaultType<string>(x => this.SearchFilter); }
            set
            {
                SetProperty<string>(x => this.SearchFilter, value);

                if (AvailableMangas != null)
                {
                    var view = CollectionViewSource.GetDefaultView(AvailableMangas);

                    if (string.IsNullOrWhiteSpace(value))
                        view.Filter = null;
                    else
                    {
                        view.Filter = new Predicate<object>(x =>
                        {
                            var manga = (Manga.Base.Manga)x;

                            return manga.MangaName.ToLower().Contains(value.ToLower());
                        });
                    }
                }
            }
        }

        public ObservableCollection<Manga.Base.Manga> AvailableMangas { get; private set; }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
