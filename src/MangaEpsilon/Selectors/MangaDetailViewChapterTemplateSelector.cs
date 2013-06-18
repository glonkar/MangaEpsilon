using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Crystal.Messaging;
using MangaEpsilon.Manga.Base;
using MangaEpsilon.Services;

namespace MangaEpsilon.Selectors
{
    public class MangaDetailViewChapterTemplateSelector: DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate DownloadedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
          DependencyObject container)
        {
            ChapterEntry path = (ChapterEntry)item;

            if (LibraryService.Contains(path))
                return DownloadedTemplate;

            return DefaultTemplate;
        }
    }
}
