using Crystal.Core;
using System.Collections;
using System.Linq;
using System;
using Crystal.Command;
using System.Threading.Tasks;
using Crystal.Navigation;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MangaEpsilon.Extensions;
using System.Net;

namespace MangaEpsilon.ViewModel
{
    //Cleaned out

    public class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            if (IsDesignMode) return;

            RegisterForMessages("UpdateMainWindowState");
            RegisterForMessages("UpdateMainWindowProgress");
        }

        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            switch (message.MessageString)
            {
                case "UpdateMainWindowState":
                    MainWindowProgressState = (System.Windows.Shell.TaskbarItemProgressState)message.Data;
                    return true;
                case "UpdateMainWindowProgress":
                    MainWindowProgressValue = (double)message.Data;
                    return true;
                default:
                    return base.ReceiveMessage(source, message);
            }
        }

        public System.Windows.Shell.TaskbarItemProgressState MainWindowProgressState
        {
            get { return GetPropertyOrDefaultType<System.Windows.Shell.TaskbarItemProgressState>(x => this.MainWindowProgressState); }
            set { SetProperty(x => this.MainWindowProgressState, value); }
        }

        public double MainWindowProgressValue
        {
            get { return GetPropertyOrDefaultType<double>(x => this.MainWindowProgressValue); }
            set { SetProperty(x => this.MainWindowProgressValue, value); }
        }
    }
}
