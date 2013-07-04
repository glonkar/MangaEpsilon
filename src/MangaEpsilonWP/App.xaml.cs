using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using System.IO;
using Crystal.Core;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;

namespace MangaEpsilonWP
{
    public partial class App : BaseCrystalApplication
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        protected override void PreStartup()
        {
            base.PreStartup();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private async void Application_Launching(object sender, LaunchingEventArgs e)
        {
            DefaultJsonSerializer = Newtonsoft.Json.JsonSerializer.Create(
                   new Newtonsoft.Json.JsonSerializerSettings()
                   {
                       ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                       PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All
                   });

            this.EnableCrystalLocalization = true;
            this.EnableDeepReflectionCaching = true;
            this.EnableSelfAssemblyResolution = true;
            this.LocalizationFallbackBehavior = Crystal.Localization.LocalizationFallbackBehavior.Fallback;
            this.FallbackLocale = new System.Globalization.CultureInfo("en-US");

            CatalogFile = "Manga.jml";

            MangaSourceInitializationTask = InitializeMangaComponents();

        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        private static async Task InitializeMangaComponents()
        {
            App.MangaSource = new MangaEpsilon.Manga.Sources.MangaEden.MangaEdenSource();

            if (isoPath.FileExists(CatalogFile))
            {
                App.MangaSource.LoadAvilableMangaFromFile(CatalogFile);

                if (App.MangaSource.AvailableManga == null)
                {
                    //corruption.
                }
            }
            else
            {
                await App.MangaSource.AcquireAvailableManga();
                SaveAvailableManga();
                await TaskEx.Delay(500);

            }
        }

        private static void SaveAvailableManga()
        {
            var preloaded = App.MangaSource.AvailableManga;

            //save
            try
            {
                using (var sw = new StreamWriter(isoPath.CreateFile(CatalogFile)))
                {
                    using (var jtw = new Newtonsoft.Json.JsonTextWriter(sw))
                    {
                        jtw.Formatting = Formatting.Indented;

                        DefaultJsonSerializer.Serialize(jtw, preloaded);

                        jtw.Flush();
                    }

                    sw.Close();
                }
            }
            catch (Exception)
            {
            }

        }

        internal static Task MangaSourceInitializationTask = null;

        internal static JsonSerializer DefaultJsonSerializer = null;

        internal static IsolatedStorageFile isoPath = IsolatedStorageFile.GetUserStoreForApplication();

        public static MangaEpsilon.Manga.Base.IMangaSource MangaSource { get; private set; }

        public static string CatalogFile { get; private set; }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Navigating += RootFrame_Navigating;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            //http://blogs.msdn.com/b/ptorr/archive/2010/08/28/redirecting-an-initial-navigation.aspx?wa=wsignin1.0

            //MainPage.xaml is under the 'View' folder so we need to redirect.

            // Only care about MainPage 
            if (e.Uri.ToString().Equals("/MainPage.xaml") != true)
                return;

            e.Cancel = true;

            RootFrame.Dispatcher.BeginInvoke(delegate
            {
                RootFrame.Navigate(new Uri("/View/MainPage.xaml", UriKind.Relative));
            });
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}