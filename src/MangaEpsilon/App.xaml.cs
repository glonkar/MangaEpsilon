using Crystal.Core;
using Crystal.Navigation;
using MangaEpsilon.ViewModel;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace MangaEpsilon
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : BaseCrystalApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void PreStartup()
        {
            EnableSelfAssemblyResolution = true;
            EnableDeepReflectionCaching = true;
            EnableCrystalLocalization = true;

            InitializeAppFolder();
        }

        private static async void InitializeAppFolder()
        {
            StorageFolder appFolder = null;
            try
            {
                appFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("MangaEpsilon");
            }
            catch (Exception)
            {
            }

            if (appFolder == null)
                appFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("MangaEpsilon");

            AppFolder = appFolder;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void PostStartupNormalLaunch(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = this.RootFrame;

            RootFrame.Style = Resources["RootFrameStyle"] as Style;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                RootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            NavigationService.NavigateTo<MainPageViewModel>(new KeyValuePair<string, object>("args", args.Arguments));
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public static ProgressBar TopProgressIndicator
        {
            get
            {
                if (Window.Current.Content == null) return null;

                DependencyObject rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);

                var pi = (ProgressBar)VisualTreeHelper.GetChild(rootGrid, 0);
                if (pi.Name == "TopProgressIndicator")
                    return pi;
                else
                    return null;
            }
        }

        public static ProgressRing ProgressIndicator
        {
            get
            {
                if (Window.Current.Content == null) return null;

                DependencyObject rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);

                var pr = (ProgressRing)VisualTreeHelper.GetChild(rootGrid, 1);
                if (pr.Name == "ProgressIndicator")
                    return pr;
                else
                    return null;
            }
        }

        public static MangaEpsilon.Manga.Base.IMangaSource MangaSource { get; set; }

        public static StorageFolder AppFolder { get; private set; }
    }
}
