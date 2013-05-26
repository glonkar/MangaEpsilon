using Crystal.Core;
using MangaEpsilon.Manga.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MangaEpsilon.Model
{
    public class MangaChapterPage : BaseModel
    {
        public MangaChapterPage(ChapterLight parent)
        {
            Chapter = parent;
        }

        public ChapterLight Chapter { get; set; }

        public string ImageUrl
        {
            get
            {
                var val = (string)GetProperty(x => this.ImageUrl);

                if (string.IsNullOrWhiteSpace(val))
                    SetImageUrl(Index);

                return val;
            }
            set { SetProperty(x => this.ImageUrl, value); }
        }

        public ImageSource Image
        {
            get
            {
                return (ImageSource)GetProperty(x => this.Image);
            }
            set { SetProperty(x => this.Image, value); }
        }

        public int Index
        {
            get { return (int)GetProperty(x => this.Index); }
            set { SetProperty(x => this.Index, value); }
        }

        private async Task SetImageUrl(int value)
        {
            //Breaking the rules of MVVM. :|

            App.ProgressIndicator.Visibility = Visibility.Visible;

            ImageUrl = await App.MangaSource.GetChapterPageImageUrl(Chapter, value);

            App.ProgressIndicator.Visibility = Visibility.Collapsed;
        }
    }
}
