using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MangaEpsilon.Converters
{
    public class ItemIsInCollectionToBoolConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var selection = values[1];
            var items = values[0];

            if (selection == null) return false;

            foreach (var item in (IEnumerable)items)
                if (item == selection)
                    return true;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
