using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

//From my old IRC Bot.
namespace Sayuka.IRC.Utilities
{
    /// <summary>
    /// This is old code from my last engine that I am adapting.
    /// </summary>
    public class UnixTimeUtil
    {
        //http://stackoverflow.com/questions/1674215/parsing-unix-time-in-c
        private readonly static DateTime unix = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime UnixTimeToDateTime(string text)
        {
            double seconds = double.Parse(text, CultureInfo.InvariantCulture);
            return unix.AddSeconds(seconds);
        }
    }
}
