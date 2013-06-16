using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Command;
using Crystal.Core;
using Crystal.Localization;
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
            await Task.WhenAll(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.

            if (App.MangaSourceInitializationTask.IsCanceled)
            {
                //until I implement the IMessageBoxService, this will do.

                System.Windows.MessageBox.Show(LocalizationManager.GetLocalizedValue("RareTaskInitializationInternetSpeedBug"));
            }
            else
            {
                await Task.WhenAll(App.MangaSource.GetMangaInfo("Naruto"), App.MangaSource.GetMangaInfo("Sekirei"), App.MangaSource.GetMangaInfo("Fairy Tail"), App.MangaSource.GetMangaInfo("Freezing"))
                    .ContinueWith(x => AmrykidsFavorites = x.Result);
            }
            IsBusy = false;

            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                var manga = ((Manga.Base.Manga)o);

                NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
            }, (o) => o != null && o is Manga.Base.Manga);
        }


        public object AmrykidsFavorites
        {
            get
            {
                return GetPropertyOrDefaultType<object>(x => this.AmrykidsFavorites);
            }
            set { SetProperty<object>(x => this.AmrykidsFavorites, value); }
        }

        public CrystalProperCommand MangaClickCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
