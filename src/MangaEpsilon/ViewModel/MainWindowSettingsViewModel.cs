using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Core;
using MahApps.Metro;
using MangaEpsilon.Data;
using Newtonsoft.Json;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowSettingsViewModel : BaseViewModel
    {
        public MainWindowSettingsViewModel()
        {
            if (IsDesignMode) return;

            App.Current.Exit += Current_Exit;

            LoadSettings();
        }

        private static string SettingsFile = App.AppDataDir + "Settings.json";

        private void LoadSettings()
        {
            SettingsInfo settings = null;
            if (File.Exists(SettingsFile))
            {
                using (var sr = new StreamReader(SettingsFile))
                {
                    using (var jtr = new JsonTextReader(sr))
                    {
                        settings = App.DefaultJsonSerializer.Deserialize<SettingsInfo>(jtr);
                    }
                }
            }
            else
            {
                //doesn't exist, set the defaults.
                settings = new SettingsInfo();
                settings.CurrentTheme = Theme.Light;
                settings.CurrentThemeAccent = "Blue";
            }

            #region Theme stuff
            AvailableAccents = new ObservableCollection<Accent>(MahApps.Metro.ThemeManager.DefaultAccents);
            SelectedAccent = ThemeManager.DefaultAccents.First(x => x.Name == settings.CurrentThemeAccent);
            SelectedTheme = (Theme)Enum.Parse(typeof(Theme), settings.CurrentTheme.ToString());
            #endregion
        }
        private void SaveSettings()
        {
            SettingsInfo settings = new SettingsInfo();
            settings.CurrentTheme = SelectedTheme;
            settings.CurrentThemeAccent = SelectedAccent.Name;

            using (var sw = new StreamWriter(SettingsFile))
            {
                using (var jtw = new JsonTextWriter(sw))
                {
                    App.DefaultJsonSerializer.Serialize(jtw, settings);
                }
            }
        }

        void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            //Save stuff.
            SaveSettings();
        }

        #region Theme related
        public MahApps.Metro.Theme SelectedTheme
        {
            get { return GetPropertyOrDefaultType<Theme>(x => this.SelectedTheme); }
            set
            {
                SetProperty(x => this.SelectedTheme, value); ThemeManager.ChangeTheme(App.Current.MainWindow, SelectedAccent, SelectedTheme);
                App.CurrentTheme = SelectedTheme;
            }
        }
        public ObservableCollection<MahApps.Metro.Accent> AvailableAccents
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<MahApps.Metro.Accent>>(x => this.AvailableAccents); }
            set { SetProperty(x => this.AvailableAccents, value); }
        }
        public MahApps.Metro.Accent SelectedAccent
        {
            get { return GetPropertyOrDefaultType<Accent>(x => this.SelectedAccent); }
            set
            {
                SetProperty(x => this.SelectedAccent, value); ThemeManager.ChangeTheme(App.Current.MainWindow, SelectedAccent, SelectedTheme);
                App.CurrentThemeAccent = SelectedAccent;
            }
        }
        #endregion
    }
}
