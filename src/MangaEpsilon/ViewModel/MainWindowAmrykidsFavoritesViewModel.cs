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
#if WINDOWS_PHONE
    using MangaEpsilonWP;
    using System.Collections.ObjectModel;
#endif
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
#if !WINDOWS_PHONE
            await Task.WhenAll(App.MangaSourceInitializationTask); //Checks (and waits if needed) for the Manga Source's initialization.
#else
            await TaskEx.WhenAll(App.MangaSourceInitializationTask);
#endif

            if (App.MangaSourceInitializationTask.IsCanceled)
            {
                //until I implement the IMessageBoxService, this will do.

                ServiceManager.Resolve<IMessageBoxService>().ShowMessage(LocalizationManager.GetLocalizedValue("GenericErrorTitle"), 
                    LocalizationManager.GetLocalizedValue("RareTaskInitializationInternetSpeedBugMsg"));
            }
            else
            {
#if !WINDOWS_PHONE
                await Task.WhenAll(App.MangaSource.GetMangaInfo("Naruto"), App.MangaSource.GetMangaInfo("Sekirei"), 
                    App.MangaSource.GetMangaInfo("Fairy Tail"), App.MangaSource.GetMangaInfo("Freezing"), App.MangaSource.GetMangaInfo("Steins;Gate"))
                    .ContinueWith(x => AmrykidsFavorites = new ObservableCollection<Manga.Base.Manga>(x.Result));
#else
                await TaskEx.WhenAll(App.MangaSource.GetMangaInfo("Naruto"),
                    App.MangaSource.GetMangaInfo("Fairy Tail"), App.MangaSource.GetMangaInfo("Steins;Gate"))
                    .ContinueWith(x => Dispatcher.BeginInvoke(() => AmrykidsFavorites = new ObservableCollection<Manga.Base.Manga>(x.Result)));

                RaisePropertyChanged(x => this.AmrykidsFavorites);
#endif
            }
            IsBusy = false;

#if !WINDOWS_PHONE
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
#else
            MangaClickCommand = CommandManager.CreateCommand((o) =>
            {
                if (o != null && o is Manga.Base.Manga)
                {
                    var manga = (Manga.Base.Manga)o;
                    NavigationService.NavigateTo<MangaDetailPageViewModel>(new KeyValuePair<string, string>("manga", manga.MangaName));
                }
            });
#endif
        }


        public ObservableCollection<Manga.Base.Manga> AmrykidsFavorites
        {
            get
            {
                return GetPropertyOrDefaultType<ObservableCollection<Manga.Base.Manga>>(x => this.AmrykidsFavorites);
            }
            set { SetProperty<ObservableCollection<Manga.Base.Manga>>(x => this.AmrykidsFavorites, value); }
        }

#if !WINDOWS_PHONE
        public CrystalProperCommand MangaClickCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
        }
#else
        public CrystalCommand MangaClickCommand
        {
            get { return (CrystalCommand)GetProperty(x => this.MangaClickCommand); }
            set { SetProperty(x => this.MangaClickCommand, value); }
        }
#endif

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty<bool>(x => this.IsBusy, value); }
        }
    }
}
