using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CustomerIssuesManager.Services;

namespace CustomerIssuesManager.Converters
{
    public class ResponsiveLayoutConverter : IValueConverter
    {
        private static readonly ResponsiveDesignService _responsiveService = ResponsiveDesignService.Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return value;

            var param = parameter.ToString() ?? string.Empty;
            
            switch (param.ToLower())
            {
                case "fontsize":
                    if (value is double baseSize)
                        return _responsiveService.GetResponsiveFontSize(baseSize);
                    break;
                    
                case "padding":
                    if (value is double basePadding)
                        return _responsiveService.GetResponsivePadding(basePadding);
                    break;
                    
                case "buttonwidth":
                    if (value is double baseWidth)
                    {
                        var buttonSize = _responsiveService.GetResponsiveButtonSize(baseWidth, 35);
                        return buttonSize.Width;
                    }
                    break;
                    
                case "buttonheight":
                    if (value is double baseHeight)
                    {
                        var buttonSize = _responsiveService.GetResponsiveButtonSize(80, baseHeight);
                        return buttonSize.Height;
                    }
                    break;
                    
                case "columnwidth":
                    if (value is double columnBaseWidth)
                        return _responsiveService.GetResponsiveColumnWidth(columnBaseWidth);
                    break;
                    
                case "windowwidth":
                    if (value is double windowBaseWidth)
                    {
                        var windowSize = _responsiveService.GetResponsiveWindowSize(windowBaseWidth, 900);
                        return windowSize.Width;
                    }
                    break;
                    
                case "windowheight":
                    if (value is double windowBaseHeight)
                    {
                        var windowSize = _responsiveService.GetResponsiveWindowSize(1400, windowBaseHeight);
                        return windowSize.Height;
                    }
                    break;
                    
                case "minwidth":
                    var minSize = _responsiveService.GetMinimumWindowSize();
                    return minSize.Width;
                    
                case "minheight":
                    var minSize2 = _responsiveService.GetMinimumWindowSize();
                    return minSize2.Height;
                    
                case "optimalwidth":
                    var optimalSize = _responsiveService.GetOptimalWindowSize();
                    return optimalSize.Width;
                    
                case "optimalheight":
                    var optimalSize2 = _responsiveService.GetOptimalWindowSize();
                    return optimalSize2.Height;
                    
                case "headerheight":
                    return _responsiveService.GetResponsiveHeaderHeight();
                    
                case "sidebarwidth":
                    return _responsiveService.GetResponsiveSidebarWidth();
                    
                case "supportscompact":
                    return _responsiveService.SupportsCompactLayout();
                    
                case "supportsexpanded":
                    return _responsiveService.SupportsExpandedLayout();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResponsiveVisibilityConverter : IValueConverter
    {
        private static readonly ResponsiveDesignService _responsiveService = ResponsiveDesignService.Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return Visibility.Visible;

            var param = parameter.ToString() ?? string.Empty;
            
            switch (param.ToLower())
            {
                case "compactonly":
                    return _responsiveService.SupportsCompactLayout() ? Visibility.Visible : Visibility.Collapsed;
                    
                case "expandedonly":
                    return _responsiveService.SupportsExpandedLayout() ? Visibility.Visible : Visibility.Collapsed;
                    
                case "sidebar":
                    return _responsiveService.SupportsCompactLayout() ? Visibility.Collapsed : Visibility.Visible;
                    
                case "headerbuttons":
                    return _responsiveService.SupportsCompactLayout() ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResponsiveGridLengthConverter : IValueConverter
    {
        private static readonly ResponsiveDesignService _responsiveService = ResponsiveDesignService.Instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return new GridLength(1, GridUnitType.Star);

            var param = parameter.ToString() ?? string.Empty;
            
            switch (param.ToLower())
            {
                case "sidebar":
                    if (_responsiveService.SupportsCompactLayout())
                        return new GridLength(0);
                    else
                        return _responsiveService.GetResponsiveColumnWidth(300);
                    
                case "spacer":
                    if (_responsiveService.SupportsCompactLayout())
                        return new GridLength(0);
                    else
                        return new GridLength(5);
                    
                case "maincontent":
                    return new GridLength(1, GridUnitType.Star);
                    
                case "header":
                    return GridLength.Auto;
                    
                case "footer":
                    return GridLength.Auto;
            }

            return new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 