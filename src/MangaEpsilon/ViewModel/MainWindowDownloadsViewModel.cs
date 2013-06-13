using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Core;
using MangaEpsilon.Model;

namespace MangaEpsilon.ViewModel
{
    public class MainWindowDownloadsViewModel: BaseViewModel
    {
        public MainWindowDownloadsViewModel()
        {
            Downloads = new ObservableCollection<MangaChapterDownload>();
        }

        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            return base.ReceiveMessage(source, message);
        }

        public ObservableCollection<MangaChapterDownload> Downloads
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<MangaChapterDownload>>(x => this.Downloads); }
            set { SetProperty<ObservableCollection<MangaChapterDownload>>(x => this.Downloads, value); }
        }
    }
}
