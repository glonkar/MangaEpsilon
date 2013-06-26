using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crystal.Services;

namespace MangaEpsilon.CServices
{
    [Export(typeof(IFileSaveDialogService))]
    public class SaveFileDialogService: Crystal.Services.IFileSaveDialogService
    {
        public Tuple<bool, string[]> ShowDialog(string filter, int filterIndex = 0)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = filter;
            sfd.FilterIndex = filterIndex;

            return Tuple.Create<bool, string[]>(sfd.ShowDialog() == DialogResult.OK, sfd.FileNames);
        }
    }
}
