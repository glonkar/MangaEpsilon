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
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowLibraryViewModel: BaseViewModel
    {
        public MainWindowLibraryViewModel()
        {
            if (IsDesignMode) return;

            Initialize();
        }

        private async void Initialize()
        {
            if (!LibraryService.IsInitialized && App.LibraryInitializationTask == null)
                await LibraryService.Initialize();
            else
                await Task.WhenAll(App.LibraryInitializationTask);

            LibraryService.LibraryItemAdded += LibraryService_LibraryItemAdded;
            LibraryService.LibraryItemRemoved += LibraryService_LibraryItemRemoved;

            LibraryItems = new ObservableCollection<Manga.Base.ChapterLight>(LibraryService.LibraryCollection.Select(x => x.Item1));

            var libraryItemsView = CollectionViewSource.GetDefaultView(LibraryItems);
            libraryItemsView.GroupDescriptions.Add(new PropertyGroupDescription("ParentManga.MangaName"));
            libraryItemsView.SortDescriptions.Add(new System.ComponentModel.SortDescription("ParentManga.MangaName", System.ComponentModel.ListSortDirection.Ascending));
            libraryItemsView.SortDescriptions.Add(new System.ComponentModel.SortDescription("ChapterNumber", System.ComponentModel.ListSortDirection.Ascending));

            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterLight)
                {
                    var chapter = ((ChapterLight)o);
                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                }
            }, (o) =>
                o != null && o is ChapterLight);

            MangaInfoCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterLight)
                {
                    var chapter = ((ChapterLight)o);
                    var manga = chapter.ParentManga;

                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                }
            }, (o) =>
                o != null && o is ChapterLight);

            DeleteMangaCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterLight)
                {
                    var chapter = ((ChapterLight)o);
                    LibraryService.RemoveLibraryItem(new Tuple<ChapterLight, string>(chapter, LibraryService.GetPath(chapter)), true);
                }
            }, (o) =>
                o != null && o is ChapterLight);
        }

        async void LibraryService_LibraryItemRemoved(Tuple<ChapterLight, string> tuple)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                LibraryItems.Remove(tuple.Item1);

                var libraryItemsView = CollectionViewSource.GetDefaultView(LibraryItems);

                libraryItemsView.Refresh();
            });
        }

        async void LibraryService_LibraryItemAdded(Tuple<Manga.Base.ChapterLight, string> tuple)
        {
            await Dispatcher.InvokeAsync(() =>
                {
                    LibraryItems.Add(tuple.Item1);

                    var libraryItemsView = CollectionViewSource.GetDefaultView(LibraryItems);

                    libraryItemsView.Refresh();
                });
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

        public CrystalProperCommand MangaInfoCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaInfoCommand); }
            set { SetProperty(x => this.MangaInfoCommand, value); }
        }
        public CrystalProperCommand DeleteMangaCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.DeleteMangaCommand); }
            set { SetProperty(x => this.DeleteMangaCommand, value); }
        }

        public ChapterLight SelectedEntry
        {
            get { return GetPropertyOrDefaultType<ChapterLight>(x => this.SelectedEntry); }
            set { SetProperty<ChapterLight>(x => this.SelectedEntry, value); }
        }

        public string SearchFilter
        {
            get { return GetPropertyOrDefaultType<string>(x => this.SearchFilter); }
            set
            {
                SetProperty<string>(x => this.SearchFilter, value);

                if (LibraryItems != null)
                {
                    var view = CollectionViewSource.GetDefaultView(LibraryItems);

                    if (string.IsNullOrWhiteSpace(value))
                        view.Filter = null;
                    else
                    {
                        view.Filter = new Predicate<object>(x =>
                        {
                            var manga = (ChapterLight)x;

                            return manga.Name.ToLower().Contains(value.ToLower()) || manga.ParentManga.MangaName.ToLower().Contains(value.ToLower());
                        });
                    }
                }
            }
        }
    }
}
