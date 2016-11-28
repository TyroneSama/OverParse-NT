using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverParse_NT.Client.Controls
{
    public class PrettyDamageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var temp = value as long?;
            if (!temp.HasValue)
                return value.ToString();

            if (temp.Value >= 100000000 /* 100,000,000 */)
                return (temp.Value / 1000000 /* 1,000,000 */).ToString("#,00") + "M";
            if (temp.Value >= 1000000 /* 1,000,000 */)
                return (temp.Value / 1000000.0 /* 1,000,000 */).ToString("0.##") + "M";
            if (temp.Value >= 100000 /* 100,000 */)
                return (temp.Value / 1000).ToString("#,00") + "K";
            if (temp.Value >= 1000)
                return (temp.Value / 1000.0).ToString("0.##") + "K";
            return temp.Value.ToString("#,00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
