using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Crystal.Command;
using Crystal.Core;
using Crystal.Localization;
using Crystal.Navigation;
using Ionic.Zip;
using MangaEpsilon.Dialogs;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowLibraryViewModel : BaseViewModel
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

            FavoritesService.ItemFavorited += FavoritesService_ItemFavoriteStatusChanged;
            FavoritesService.ItemUnfavorited += FavoritesService_ItemFavoriteStatusChanged;

            LibraryItems = new ObservableCollection<Manga.Base.ChapterLight>(LibraryService.LibraryCollection.Select(x => x.Item1));

            libraryItemsView = (System.Windows.Data.ListCollectionView)CollectionViewSource.GetDefaultView(LibraryItems);
            libraryItemsView.GroupDescriptions.Add(new PropertyGroupDescription("ParentManga.MangaName"));
            libraryItemsView.SortDescriptions.Add(new System.ComponentModel.SortDescription("ParentManga.MangaName", System.ComponentModel.ListSortDirection.Ascending));
            libraryItemsView.SortDescriptions.Add(new System.ComponentModel.SortDescription("ChapterNumber", System.ComponentModel.ListSortDirection.Ascending));

            InitializeCommands();
        }

        System.Windows.Data.ListCollectionView libraryItemsView = null;

        void FavoritesService_ItemFavoriteStatusChanged(Manga.Base.Manga manga)
        {
            //CollectionViewGroup group = (CollectionViewGroup)libraryItemsView.Groups.First(x => 
            //    ((ChapterLight)((CollectionViewGroup)x).Items[0]).ParentManga.MangaName == manga.MangaName);

            RedrawLibraryItems();
        }

        private void RedrawLibraryItems()
        {
            //flush the queue
            //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle, new Action(() => { }));
            //await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.SystemIdle);
            //await System.Windows.Threading.Dispatcher.Yield();

            var libraryItemsView2 = CollectionViewSource.GetDefaultView(LibraryItems);

            libraryItemsView2.Refresh();
        }

        private void InitializeCommands()
        {
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

            CreateCBZCommand = CommandManager.CreateProperCommand(async (o) =>
            {
                var results = Crystal.Services.ServiceManager.Resolve<Crystal.Services.IFileSaveDialogService>().ShowDialog("Comic Book Archive (.cbz)|*.cbz", 0);

                if (results.Item1)
                {
                    ChapterLight chapter = ((ChapterLight)o);

                    ProgressDialog pd = new ProgressDialog();

                    pd.Owner = App.Current.MainWindow;

                    pd.UpdateText(LocalizationManager.GetLocalizedValue("CreatingCBATitle"), LocalizationManager.GetLocalizedValue("CompressingImgMsg"));

                    pd.Show();

                    await Task.Delay(1000); //wait a second to get the window open.

                    ZipFile zip = new Ionic.Zip.ZipFile();

                    int imgcount = chapter.PagesUrls.Count;
                    int i = 1;
                    foreach (var image in chapter.PagesUrls)
                    {
                        await Task.Run(() =>
                            {
                                Uri url = new Uri(image);
                                var fileName = LibraryService.GetPath(chapter) + url.Segments.Last();
                                zip.AddFile(fileName).FileName = i.ToString().PadLeft(3, '0') + fileName.Substring(fileName.IndexOf('.'));
                            });

                        if (pd.IsCancel) break;

                        await Dispatcher.InvokeAsync(() =>
                            {
                                pd.UpdateText(LocalizationManager.GetLocalizedValue("CreatingCBATitle"),
                                    string.Format(LocalizationManager.GetLocalizedValue("CompressingImgFormatMsg"), i.ToString(), imgcount.ToString()));
                            });

                        i++;
                    }

                    if (!pd.IsCancel)
                        zip.Save(results.Item2[0]);

                    zip.Dispose();

                    pd.Close();
                }
            }, (o) =>
                o != null && o is ChapterLight);
        }

        async void LibraryService_LibraryItemRemoved(Tuple<ChapterLight, string> tuple)
        {
            await UIDispatcher.InvokeAsync(() =>
            {
                LibraryItems.Remove(tuple.Item1);

                var libraryItemsView = CollectionViewSource.GetDefaultView(LibraryItems);

                libraryItemsView.Refresh();
            });
        }

        async void LibraryService_LibraryItemAdded(Tuple<Manga.Base.ChapterLight, string> tuple)
        {
            await UIDispatcher.InvokeAsync(() =>
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
        public CrystalProperCommand CreateCBZCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.CreateCBZCommand); }
            set { SetProperty(x => this.CreateCBZCommand, value); }
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
