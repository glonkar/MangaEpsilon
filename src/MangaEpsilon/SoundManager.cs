using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;

namespace MangaEpsilon
{
    public static class SoundManager
    {
        public static void Initialize()
        {
            string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            WindowsNotify = new SoundPlayer(windir + "\\Media\\Windows Notify.wav");
            WindowsNotify.Load();

            WindowsBalloon = new SoundPlayer(windir + "\\Media\\Windows Balloon.wav");
            WindowsBalloon.Load();

            WindowsPrintCompleted = new SoundPlayer(windir + "\\Media\\Windows Print complete.wav");
            WindowsPrintCompleted.Load();
        }
        public static SoundPlayer WindowsNotify { get; private set; }
        public static SoundPlayer WindowsBalloon { get; private set; }
        public static SoundPlayer WindowsPrintCompleted { get; private set; }
    }
}
