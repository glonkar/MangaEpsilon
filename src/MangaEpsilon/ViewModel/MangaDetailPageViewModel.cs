using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MangaEpsilon.ViewModel
{
    public class MangaDetailPageViewModel : BaseViewModel
    {
        public override void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            if (IsDesignMode) return;

            Manga = (Manga.Base.Manga)argument[0].Value;

            MangaName = Manga.MangaName;

            SetCoverImage();

            OpenMangaChapterCommand = CommandManager.CreateCommand(x =>
                {
                    ChapterEntry selectedChapter = x is ChapterEntry ? (ChapterEntry)x : (x is ChapterEntryWrapper ? ((ChapterEntryWrapper)x).WrappedObject : null);

                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", selectedChapter));
                });
        }

        private async void SetCoverImage()
        {
            await Manga.FetchImage();
            CoverImage = (ImageSource)Manga.BookImageFld;
        }

        public Manga.Base.Manga Manga
        {
            get { return GetPropertyOrDefaultType<Manga.Base.Manga>(x => this.Manga); }
            set { SetProperty(x => this.Manga, value); }
        }

        public string MangaName
        {
            get { return GetPropertyOrDefaultType<string>(x => this.MangaName); }
            set { SetProperty<string>(x => this.MangaName, value); }
        }

        public ImageSource CoverImage
        {
            get { return GetPropertyOrDefaultType<ImageSource>(x => this.CoverImage); }
            set { SetProperty<ImageSource>(x => this.CoverImage, value); }
        }

        public CrystalCommand OpenMangaChapterCommand { get; set; }
    }
}
