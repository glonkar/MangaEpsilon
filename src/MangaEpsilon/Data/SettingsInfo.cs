using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MangaEpsilon.Data
{
    [DataContract]
    internal class SettingsInfo
    {
        [DataMember]
        public object CurrentTheme { get; set; }
        [DataMember]
        public string CurrentThemeAccent { get; set; }
    }
}
