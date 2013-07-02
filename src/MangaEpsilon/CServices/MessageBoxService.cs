using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MangaEpsilon.CServices
{
    class MessageBoxService: Crystal.Services.IMessageBoxService
    {
        public void ShowMessage(string title = "Title", string message = "Message")
        {
            MessageBox.Show(title, message);
        }

        public bool? ShowOkayCancelMessage(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
