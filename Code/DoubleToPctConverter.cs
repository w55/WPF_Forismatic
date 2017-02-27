using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyWpfForismatic
{
    /// <summary>
    /// Вспомогательный класс - конвертер для отображения числа в круглом прогрессбаре
    /// </summary>
    public class DoubleToPctConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "";
            double d = (double)value;
            int i = (int)d;

            //result = string.Format("{0}%", d);
            result = string.Format("{0} %", i);
            // result = d.ToString("0.0") + "%";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
