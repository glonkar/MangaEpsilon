using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;
using MangaEpsilon.Manga.Base;
//using MangaEpsilon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Crystal.Messaging;
using MangaEpsilon.Services;

namespace MangaEpsilon.ViewModel
{
    public class MangaDetailPageViewModel : BaseViewModel
    {
        public override void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            if (IsDesignMode) return;

            Manga = (Manga.Base.Manga)argument[0].Value;

            //var firstEntry = Yukihyo.MAL.MyAnimeListAPI.Search(Manga.MangaName, Yukihyo.MAL.MALSearchType.manga).First(x => x.Title.ToLower() == Manga.MangaName.ToLower());

            OpenMangaChapterCommand = CommandManager.CreateCommand(x =>
                {
                    ChapterEntry selectedChapter = x as ChapterEntry;

                    NavigationService.ShowWindow<MangaChapterViewPageViewModel>(new KeyValuePair<string, object>("chapter", selectedChapter));
                });

            MangaDownloadCommand = CommandManager.CreateProperCommand((o) =>
            {
                if (o is ChapterEntry)
                {
                    var chapter = ((ChapterEntry)o);
                    var manga = chapter.ParentManga;

                    Messenger.PushMessage(this, "MangaChapterDownload", chapter);
                }
            }, (o) =>
                o != null && o is ChapterEntry && !LibraryService.Contains((ChapterEntry)o));

            GetUpdatedInfo();
        }

        private async void GetUpdatedInfo()
        {
            var newManga = await App.MangaSource.GetMangaInfo(Manga.MangaName, false); //Get fresh, updated information.
            Manga.Description = newManga.Description;
            Manga.Author = newManga.Author;
        }

        public Manga.Base.Manga Manga
        {
            get { return GetPropertyOrDefaultType<Manga.Base.Manga>(x => this.Manga); }
            set { SetProperty(x => this.Manga, value); }
        }

        public CrystalCommand OpenMangaChapterCommand { get; set; }
        public CrystalProperCommand MangaDownloadCommand
        {
            get { return (CrystalProperCommand)GetProperty(x => this.MangaDownloadCommand); }
            set { SetProperty(x => this.MangaDownloadCommand, value); }
        }

        public ChapterEntry SelectedChapterItem
        {
            get { return (ChapterEntry)GetProperty(x => this.SelectedChapterItem); }
            set
            {
                SetProperty(x => this.SelectedChapterItem, value);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
