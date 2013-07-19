using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.IO;

namespace MangaEpsilonWP.Reimps
{
    /// <summary>
    /// Limited reimplementation
    /// </summary>
    public class HttpClient: IDisposable
    {
        public async Task<string> GetStringAsync(string url)
        {
            Stream stream = await GetStreamAsync(url).ConfigureAwait(false);

            string result = null;

            using (StreamReader reader = new StreamReader(stream))
            {
                result = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            stream.Close();

            return result;
        }
        public async Task<Stream> GetStreamAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.CreateHttp(url);
            request.AllowReadStreamBuffering = true;
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);

            return response.GetResponseStream();
        }

        public void Dispose()
        {
            return;
        }
    }
}
