using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;

namespace ZloGUILauncher.Converters
{
    public class RelativeStatsPathToImageConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {            
            return new BitmapImage(new Uri("pack://application:,,,/Media/" + value.ToString() ));
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class JTokenToStringConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class JTokenToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            double v = 0;
            if (double.TryParse(value.ToString() , out v))
            {
                var ts = TimeSpan.FromSeconds(v);
                return $"{(int)ts.TotalHours} H {ts.Minutes} M {ts.Seconds} S";
            }
            else
            {
                return "0 H 0 M 0 S";
            }
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
