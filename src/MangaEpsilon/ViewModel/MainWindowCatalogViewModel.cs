﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;

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
            await Task.WhenAll(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.

            if (App.MangaSourceInitializationTask.IsCanceled) return; //MainWindowAmrykidsFavoritesViewModel will show a messagebox about this rare circumstance.

            AvailableMangas = new ObservableCollection<Manga.Base.Manga>(App.MangaSource.AvailableManga); // I hope virtualization is on.
            // new PaginatedObservableCollection<Manga.Base.Manga>(App.MangaSource.AvailableManga);
            //AvailableMangas.PageSize = 30;

            CollectionViewSource.GetDefaultView(AvailableMangas).SortDescriptions.Add(new System.ComponentModel.SortDescription("MangaName", System.ComponentModel.ListSortDirection.Ascending));

            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                var manga = ((Manga.Base.Manga)o);

                var win = ViewModelOperations.FindWindow((vm) =>  vm.ContainsProperty("Manga") && ((Manga.Base.Manga)vm.GetProperty("Manga")).MangaName == manga.MangaName); //checks if its already open. probably not MVVM-ish.

                if (win == null)
                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                else
                    win.Focus();

            }, (o) => o != null && o is Manga.Base.Manga);

            IsBusy = false;
        }

        public CrystalProperCommand MangaClickCommand
        {
            get { return GetPropertyOrDefaultType<CrystalProperCommand>(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
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

        public ObservableCollection<Manga.Base.Manga> AvailableMangas
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Manga.Base.Manga>>(x => this.AvailableMangas); }
            set { SetProperty(x => this.AvailableMangas, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
