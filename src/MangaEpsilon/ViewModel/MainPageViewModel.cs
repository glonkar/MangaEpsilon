using Crystal.Core;
using System.Collections;
using System.Linq;
using System;
using MangaEpsilon.Model;
using Crystal.Command;
using System.Threading.Tasks;
using Crystal.Navigation;
using System.Collections.Generic;
using Windows.Storage;
using System.IO;

namespace MangaEpsilon.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            InitializeMangaSourceIfNecessary();
        }


        public override async void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            if (IsDesignMode) return;

            MangaClickCommand = CommandManager.CreateCommand((o) =>
            {
                if (o is ChapterEntryWrapper)
                {
                    var chapter = ((ChapterEntryWrapper)o).WrappedObject;
                    NavigationService.NavigateTo<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                }
                else if (o is MangaWrapper)
                {
                    var manga = ((MangaWrapper)o).WrappedObject;

                    NavigationService.NavigateTo<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                }
            });

            IsLatestReleasesView = true;
        }

        private static async Task InitializeMangaSourceIfNecessary()
        {
            if (App.MangaSource == null)
            {
                Dictionary<string, string> preloaded = null;
                StorageFile dicts = null;
                try
                {
                    dicts = await App.AppFolder.GetFileAsync("Manga.json");


                    var str = await dicts.OpenReadAsync();

                    using (StreamReader sr = new StreamReader(str.AsStream()))
                    {
                        preloaded = MangaEpsilon.JSON.JsonSerializer.Deserialize<Dictionary<string, string>>(await sr.ReadToEndAsync());
                    }

                    str.Dispose();
                }
                catch (Exception)
                {
                }

                if (preloaded != null)
                {
                    if (App.MangaSource == null)
                        App.MangaSource = new MangaEpsilon.Manga.Sources.MangaReader.MangaReaderSource(preloaded);
                }
                else
                {
                    App.MangaSource = new MangaEpsilon.Manga.Sources.MangaReader.MangaReaderSource(false);

                    preloaded = await App.MangaSource.GetAvailableManga();

                    App.MangaSource = new MangaEpsilon.Manga.Sources.MangaReader.MangaReaderSource(preloaded);

                    dicts = await App.AppFolder.CreateFileAsync("Manga.json");

                    var json = MangaEpsilon.JSON.JsonSerializer.Serialize(preloaded);

                    using (var w = await dicts.OpenStreamForWriteAsync())
                    {
                        using (var sw = new StreamWriter(w))
                        {
                            await sw.WriteAsync(json);
                            await sw.FlushAsync();
                        }
                    }
                }
            }
        }

        private volatile bool _loading_latestmanga = false;
        private volatile bool _loading_amrykid_manga = false;

        private async void GetNewlyReleasedManga()
        {
            if (IsDesignMode) return;

            if (App.ProgressIndicator == null)
                await Task.Delay(1000);

            App.ProgressIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;

            _loading_latestmanga = true;

            await InitializeMangaSourceIfNecessary();

            var latestMangas = await App.MangaSource.GetNewReleasesOfToday();

            NewReleasesToday = Enumerable.Select(latestMangas,
                x =>
                    new ChapterEntryWrapper(x));

            if (IsLatestReleasesView)
                RaisePropertyChanged(x => this.CurrentViewDataSource);

            _loading_latestmanga = false;

            App.ProgressIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void GetAmrykidsFavorites()
        {
            if (IsDesignMode) return;

            if (App.ProgressIndicator == null)
                await Task.Delay(1000);

            App.ProgressIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;

            _loading_amrykid_manga = true;

            await InitializeMangaSourceIfNecessary();

            AmrykidsFavorites = new object[] { await App.MangaSource.GetMangaInfo("Naruto"), await App.MangaSource.GetMangaInfo("Sekirei"), await App.MangaSource.GetMangaInfo("Fairy Tail") }.Select(x => new MangaWrapper((Manga.Base.Manga)x));

            if (!IsLatestReleasesView)
                RaisePropertyChanged(x => this.CurrentViewDataSource);

            _loading_amrykid_manga = false;

            App.ProgressIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        public object NewReleasesToday
        {
            get
            {
                var val = GetPropertyOrDefaultType<object>(x => this.NewReleasesToday);

                if (val == null && _loading_latestmanga == false)
                    GetNewlyReleasedManga();

                return val;
            }
            set { SetProperty<object>(x => this.NewReleasesToday, value); }
        }

        public object AmrykidsFavorites
        {
            get
            {
                var val = GetPropertyOrDefaultType<object>(x => this.AmrykidsFavorites);

                if (val == null && _loading_amrykid_manga == false)
                    GetAmrykidsFavorites();

                return val;
            }
            set { SetProperty<object>(x => this.AmrykidsFavorites, value); }
        }

        public bool IsLatestReleasesView
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsLatestReleasesView); }
            set { SetProperty<bool>(x => this.IsLatestReleasesView, value); RaisePropertyChanged(x => this.CurrentViewDataSource); }
        }

        public object CurrentViewDataSource
        {
            get
            {
                return (IsLatestReleasesView) ? NewReleasesToday : AmrykidsFavorites;
            }
        }

        public CrystalCommand MangaClickCommand { get; set; }
    }
}
