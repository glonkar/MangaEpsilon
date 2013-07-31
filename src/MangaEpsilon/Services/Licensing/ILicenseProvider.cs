using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaEpsilon.Services.Licensing
{
    public interface ILicenseProvider
    {
        string Name { get; }
        string FriendlyName { get; }
        string GetShopLink(Manga.Base.Manga Manga);
        string ProviderLicensedTitlesFile { get; }

        bool IsMangaLicensedFromProvider(Manga.Base.Manga Manga);

        Task LoadLicensedMangas();

        System.Globalization.RegionInfo LicenseRegion { get; }
    }
}
