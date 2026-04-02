using AppDataCleaner.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AppDataCleaner.Converters
{
    [ValueConversion(typeof(RiskLevel), typeof(Brush))]
    public class RiskLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RiskLevel risk)
            {
                return risk switch
                {
                    RiskLevel.Safe => new SolidColorBrush(Colors.Green),
                    RiskLevel.Caution => new SolidColorBrush(Colors.Orange),
                    RiskLevel.Danger => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(RiskLevel), typeof(string))]
    public class RiskLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RiskLevel risk)
            {
                return risk switch
                {
                    RiskLevel.Safe => "安全",
                    RiskLevel.Caution => "谨慎",
                    RiskLevel.Danger => "危险",
                    _ => "未知"
                };
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
