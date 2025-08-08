using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomerIssuesManager.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "جديدة" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)), // #3498db
                    "قيد التنفيذ" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 156, 18)), // #f39c12
                    "تم حلها" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)), // #27ae60
                    "مغلقة" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166)), // #95a5a6
                    _ => new SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166)) // Default gray
                };
            }
            
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166)); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush && brush != null)
            {
                var color = brush.Color;
                
                // Convert color back to status based on the same color mapping
                if (color.R == 52 && color.G == 152 && color.B == 219)
                    return "جديدة";
                if (color.R == 243 && color.G == 156 && color.B == 18)
                    return "قيد التنفيذ";
                if (color.R == 39 && color.G == 174 && color.B == 96)
                    return "تم حلها";
                if (color.R == 149 && color.G == 165 && color.B == 166)
                    return "مغلقة";
            }
            
            return "مغلقة"; // Default status
        }
    }
} 