using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MangaEpsilon.Extensions
{
    public static class BitmapImageEx
    {
        public static async Task WaitForDownloadCompletion(this BitmapImage image)
        {
            await Task.Delay(1000);
            await Task.Run(async () =>
                {
                    bool crossThread = false;
                    bool downloading = false;
                    try
                    {
                        downloading = image.IsDownloading;
                    }
                    catch (InvalidOperationException)
                    {
                        crossThread = true;
                    }

                    if (crossThread)
                        downloading = Dispatcher.CurrentDispatcher.Invoke<bool>(() =>
                            image.IsDownloading);


                    if (downloading)
                    {
                        do
                        {
                            await Task.Delay(500);
                        }
                        while (Dispatcher.CurrentDispatcher.Invoke<bool>(() =>
                            image.IsDownloading));
                    }
                });
        }
    }
}
