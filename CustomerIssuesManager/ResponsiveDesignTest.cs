using System;
using System.Windows;
using CustomerIssuesManager.Services;

namespace CustomerIssuesManager
{
    public static class ResponsiveDesignTest
    {
        public static void TestResponsiveDesign()
        {
            try
            {
                var responsiveService = ResponsiveDesignService.Instance;
                
                // Test screen size detection
                Console.WriteLine($"Current Screen Size: {responsiveService.CurrentScreenSize}");
                Console.WriteLine($"Current Screen Density: {responsiveService.CurrentScreenDensity}");
                Console.WriteLine($"Scale Factor: {responsiveService.ScaleFactor}");
                
                // Test responsive calculations
                var fontSize = responsiveService.GetResponsiveFontSize(12);
                Console.WriteLine($"Responsive Font Size (base 12): {fontSize}");
                
                var buttonSize = responsiveService.GetResponsiveButtonSize(80, 35);
                Console.WriteLine($"Responsive Button Size (base 80x35): {buttonSize.Width}x{buttonSize.Height}");
                
                var windowSize = responsiveService.GetOptimalWindowSize();
                Console.WriteLine($"Optimal Window Size: {windowSize.Width}x{windowSize.Height}");
                
                var minSize = responsiveService.GetMinimumWindowSize();
                Console.WriteLine($"Minimum Window Size: {minSize.Width}x{minSize.Height}");
                
                // Test layout support
                Console.WriteLine($"Supports Compact Layout: {responsiveService.SupportsCompactLayout()}");
                Console.WriteLine($"Supports Expanded Layout: {responsiveService.SupportsExpandedLayout()}");
                
                Console.WriteLine("Responsive Design Test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Responsive Design Test failed: {ex.Message}");
            }
        }
    }
} 