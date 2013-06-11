using Crystal.Core;
using System.Collections;
using System.Linq;
using System;
using MangaEpsilon.Model;
using Crystal.Command;
using System.Threading.Tasks;
using Crystal.Navigation;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MangaEpsilon.Extensions;
using System.Net;

namespace MangaEpsilon.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            if (IsDesignMode) return;

            Initialize();
        }

        private async void Initialize()
        {
            await Task.Delay(2000);

            App.ProgressIndicator.Visibility = System.Windows.Visibility.Visible;

            await InitializeMangaSourceIfNecessary();

            App.ProgressIndicator.Visibility = System.Windows.Visibility.Collapsed;

            MangaClickCommand = CommandManager.CreateCommand((o) =>
            {
                if (o is ChapterEntryWrapper)
                {
                    var chapter = ((ChapterEntryWrapper)o).WrappedObject;
                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", chapter));
                }
                else if (o is MangaWrapper)
                {
                    var manga = ((MangaWrapper)o).WrappedObject;

                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                }
            });

            //RandomSelectedMangas = new ObservableCollection<ImageSource>();

            //for (int i = 0; i < 2; i++)
            //{
            //    try
            //    {
            //        var count = NewReleasesToday.ToArray().Length;
            //        var img = NewReleasesToday.ToArray()[new Random().Next(0, count)];

            //        var bi = new BitmapImage();
            //        bi.BeginInit();

            //        var bookImageUri = new Uri(img.WrappedObject.ParentManga.BookImageUrl);
            //        if (!File.Exists(App.ImageCacheDir + bookImageUri.Segments.Last()))
            //        {

            //            bi.UriSource = new Uri(img.WrappedObject.ParentManga.BookImageUrl);

            //            using (WebClient wc = new WebClient())
            //            {
            //                await wc.DownloadFileTaskAsync(img.WrappedObject.ParentManga.BookImageUrl, App.ImageCacheDir + bookImageUri.Segments.Last());
            //            }
            //        }
            //        else
            //        {
            //            bi.UriSource = new Uri(App.ImageCacheDir + bookImageUri.Segments.Last());
            //        }

            //        bi.EndInit();
            //        RandomSelectedMangas.Add(bi);
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}

            //RandomSelectedMangaItem = RandomSelectedMangas[0];

        }


        //public override async void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        //{
        //    if (IsDesignMode) return;

        //    Initialize();
        //}

        private async Task InitializeMangaSourceIfNecessary()
        {
            await UpdateMangaCache();

            var favs = Task.WhenAll(App.MangaSource.GetMangaInfo("Naruto"), App.MangaSource.GetMangaInfo("Sekirei"), App.MangaSource.GetMangaInfo("Fairy Tail"))
                .ContinueWith(x => AmrykidsFavorites = x.Result.Select(y => new MangaWrapper((Manga.Base.Manga)y)));


            var latestMangas = await App.MangaSource.GetNewReleasesOfToday(8);

            NewReleasesToday = new ObservableCollection<ChapterEntryWrapper>();

            foreach (var manga in latestMangas.Select(x => new ChapterEntryWrapper(x)))
            {
                await Task.Delay(200);
                NewReleasesToday.Add(manga);
            }

            await Task.WhenAll(favs);
        }
        private static async Task UpdateMangaCache()
        {
            Dictionary<string, string> preloaded = null;
            Stream dicts = null;
            try
            {
                dicts = new FileStream(App.AppDataDir + "Manga.json", FileMode.Open);

                using (StreamReader sr = new StreamReader(dicts))
                {
                    preloaded = MangaEpsilon.JSON.JsonSerializer.Deserialize<Dictionary<string, string>>(await sr.ReadToEndAsync());
                }

                dicts.Dispose();
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

                using (dicts = new FileStream(App.AppDataDir + "Manga.json", FileMode.OpenOrCreate))
                {
                    var json = MangaEpsilon.JSON.JsonSerializer.Serialize(preloaded);

                    using (var sw = new StreamWriter(dicts))
                    {
                        await sw.WriteAsync(json);
                        await sw.FlushAsync();
                    }
                }
            }
        }

        public ObservableCollection<ChapterEntryWrapper> NewReleasesToday
        {
            get
            {
                return GetPropertyOrDefaultType<ObservableCollection<ChapterEntryWrapper>>(x => this.NewReleasesToday);
            }
            set { SetProperty<object>(x => this.NewReleasesToday, value); }
        }

        public object AmrykidsFavorites
        {
            get
            {
                return GetPropertyOrDefaultType<object>(x => this.AmrykidsFavorites);
            }
            set { SetProperty<object>(x => this.AmrykidsFavorites, value); }
        }
        public CrystalCommand MangaClickCommand { get; set; }

        public ObservableCollection<ImageSource> RandomSelectedMangas
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<ImageSource>>(x => this.RandomSelectedMangas); }
            set { SetProperty<ObservableCollection<ImageSource>>(x => this.RandomSelectedMangas, value); }
        }

        public ImageSource RandomSelectedMangaItem
        {
            get { return GetPropertyOrDefaultType<ImageSource>(x => this.RandomSelectedMangaItem); }
            set
            {
                if (value == GetPropertyOrDefaultType<ImageSource>(x => this.RandomSelectedMangaItem)) return;

                SetProperty<ImageSource>(x => this.RandomSelectedMangaItem, value);

                Dispatcher.InvokeAsync(() =>
                    {
                        RandomSelectedMangaBannerText = NewReleasesToday.First(z =>
                            z.WrappedObject.ParentManga.BookImageUrl == ((BitmapImage)value).UriSource.ToString()).Title;
                    });
            }
        }

        public string RandomSelectedMangaBannerText
        {
            get { return GetPropertyOrDefaultType<string>(x => this.RandomSelectedMangaBannerText); }
            set { SetProperty<string>(x => this.RandomSelectedMangaBannerText, value); }
        }
    }
}
