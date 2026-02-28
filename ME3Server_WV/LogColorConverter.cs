using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ME3Server_WV
{
    public class LogColorConverter : IValueConverter
    {
        public static readonly LogColorConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogColor color)
            {
                return color switch
                {
                    LogColor.Black => Brushes.Black,
                    LogColor.Red => Brushes.Red,
                    LogColor.Blue => Brushes.Blue,
                    LogColor.DarkBlue => Brushes.DarkBlue,
                    LogColor.DarkGreen => Brushes.DarkGreen,
                    LogColor.Gray => Brushes.Gray,
                    LogColor.White => Brushes.DimGray,
                    LogColor.Cyan => Brushes.DarkCyan,
                    LogColor.Yellow => Brushes.Goldenrod,
                    LogColor.Orange => Brushes.OrangeRed,
                    LogColor.DarkRed => Brushes.DarkRed,
                    LogColor.Magenta => Brushes.DarkMagenta,
                    _ => Brushes.Black,
                };
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
