using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WpfColor = System.Windows.Media.Color;

namespace CustomerIssuesManager.Converters
{
    public class ColorCodeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorCode && !string.IsNullOrEmpty(colorCode))
            {
                try
                {
                    // Remove # if present
                    if (colorCode.StartsWith("#"))
                    {
                        colorCode = colorCode.Substring(1);
                    }

                    // Parse hex color
                    if (colorCode.Length == 6)
                    {
                        var r = System.Convert.ToByte(colorCode.Substring(0, 2), 16);
                        var g = System.Convert.ToByte(colorCode.Substring(2, 2), 16);
                        var b = System.Convert.ToByte(colorCode.Substring(4, 2), 16);
                        return new SolidColorBrush(WpfColor.FromRgb(r, g, b));
                    }
                }
                catch
                {
                    // If parsing fails, return a default color
                }
            }
            
            // Default color if conversion fails
            return new SolidColorBrush(WpfColor.FromRgb(128, 128, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 