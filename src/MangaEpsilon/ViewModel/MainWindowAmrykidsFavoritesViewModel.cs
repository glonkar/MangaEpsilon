using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowAmrykidsFavoritesViewModel : BaseViewModel
    {
        public MainWindowAmrykidsFavoritesViewModel()
        {
            if (IsDesignMode) return;

            Initialize();

        }

        private async void Initialize()
        {
            IsBusy = true;
            await Task.WhenAny(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.

            await Task.WhenAll(App.MangaSource.GetMangaInfo("Naruto"), App.MangaSource.GetMangaInfo("Sekirei"), App.MangaSource.GetMangaInfo("Fairy Tail"))
                .ContinueWith(x => AmrykidsFavorites = x.Result);
            IsBusy = false;

            MangaClickCommand = CommandManager.CreateCommand((o) =>
            {
                var manga = ((Manga.Base.Manga)o);

                NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
            });
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

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
