using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Command;
using Crystal.Core;
using Crystal.Localization;
using Crystal.Navigation;
using Crystal.Services;

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

                ServiceManager.Resolve<IMessageBoxService>().ShowMessage(LocalizationManager.GetLocalizedValue("GenericErrorTitle"), 
                    LocalizationManager.GetLocalizedValue("RareTaskInitializationInternetSpeedBugMsg"));
            }
            else
            {
                await Task.WhenAll(App.MangaSource.GetMangaInfo("Naruto"), App.MangaSource.GetMangaInfo("Sekirei"), 
                    App.MangaSource.GetMangaInfo("Fairy Tail"), App.MangaSource.GetMangaInfo("Freezing"), App.MangaSource.GetMangaInfo("Steins;Gate"))
                    .ContinueWith(x => AmrykidsFavorites = x.Result);
            }
            IsBusy = false;

            MangaClickCommand = CommandManager.CreateProperCommand((o) =>
            {
                var manga = (Manga.Base.Manga)o;

                Predicate<ICrystalViewModel> pred = (vm) => { return vm.ContainsProperty("Manga") && ((Manga.Base.Manga)vm.GetProperty("Manga")).MangaName == manga.MangaName; };
                var win = ViewModelOperations.FindWindow(pred); //checks if its already open. probably not MVVM-ish.

                if (win == null)
                    NavigationService.ShowWindow<MangaDetailPageViewModel>(new KeyValuePair<string, object>("manga", manga));
                else
                    win.Focus();
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
