using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace MangaEpsilon.Common
{
    /// <summary>
    /// http://michelsalib.com/2012/10/21/winrt-how-to-properly-implement-isupportincrementalloading-with-navigation/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private Func<uint, Task<IEnumerable>> load;
        public bool HasMoreItems { get; protected set; }

        public PaginatedCollection(Func<uint, Task<IEnumerable>> load)
        {
            HasMoreItems = true;
            this.load = load;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async c =>
            {
                var data = await load(count);

                foreach (var item in data)
                {
                    Add((T)item);
                }

                HasMoreItems = Enumerable.Any((IEnumerable<T>)data);

                return new LoadMoreItemsResult()
                {
                    Count = (uint)Enumerable.Count((IEnumerable<T>)data),
                };
            });
        }
    }


}
