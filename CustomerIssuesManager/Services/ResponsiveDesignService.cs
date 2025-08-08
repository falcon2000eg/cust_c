using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfLabel = System.Windows.Controls.Label;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace CustomerIssuesManager.Services
{
    public class ResponsiveDesignService
    {
        private static ResponsiveDesignService _instance = null!;
        private static readonly object _lock = new object();

        public static ResponsiveDesignService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ResponsiveDesignService();
                    }
                }
                return _instance;
            }
        }

        // Screen size categories
        public enum ScreenSize
        {
            Small,      // < 1024x768
            Medium,     // 1024x768 - 1366x768
            Large,      // 1366x768 - 1920x1080
            ExtraLarge  // > 1920x1080
        }

        // Density categories
        public enum ScreenDensity
        {
            Low,        // < 96 DPI
            Normal,     // 96 DPI
            High,       // 120 DPI
            ExtraHigh   // > 144 DPI
        }

        public ScreenSize CurrentScreenSize { get; private set; }
        public ScreenDensity CurrentScreenDensity { get; private set; }
        public double ScaleFactor { get; private set; }

        private ResponsiveDesignService()
        {
            // Initialize with default values first
            CurrentScreenSize = ScreenSize.Medium;
            CurrentScreenDensity = ScreenDensity.Normal;
            ScaleFactor = 1.0;
            
            // Try to initialize screen properties, but don't fail if it doesn't work
            try
            {
                InitializeScreenProperties();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveDesignService: Error initializing screen properties: {ex.Message}");
                // Keep default values if initialization fails
            }
        }

        private void InitializeScreenProperties()
        {
            // Use WPF screen information instead of WinForms
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var dpi = GetSystemDpi();

            // Determine screen size category
            if (screenWidth < 1024 || screenHeight < 768)
                CurrentScreenSize = ScreenSize.Small;
            else if (screenWidth <= 1366 && screenHeight <= 768)
                CurrentScreenSize = ScreenSize.Medium;
            else if (screenWidth <= 1920 && screenHeight <= 1080)
                CurrentScreenSize = ScreenSize.Large;
            else
                CurrentScreenSize = ScreenSize.ExtraLarge;

            // Determine density category
            if (dpi < 96)
                CurrentScreenDensity = ScreenDensity.Low;
            else if (dpi <= 96)
                CurrentScreenDensity = ScreenDensity.Normal;
            else if (dpi <= 120)
                CurrentScreenDensity = ScreenDensity.High;
            else
                CurrentScreenDensity = ScreenDensity.ExtraHigh;

            // Calculate scale factor
            ScaleFactor = dpi / 96.0;
        }

        private double GetSystemDpi()
        {
            // Use WPF DPI information instead of WinForms
            try
            {
                if (System.Windows.Application.Current?.MainWindow != null)
                {
                    var presentationSource = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);
                    if (presentationSource?.CompositionTarget != null)
                    {
                        return presentationSource.CompositionTarget.TransformToDevice.M11 * 96.0;
                    }
                }
                
                // Fallback to system DPI
                return 96.0; // Default DPI
            }
            catch
            {
                // Default to 96 DPI if all else fails
                return 96.0;
            }
        }

        // Get responsive font size based on screen size and density
        public double GetResponsiveFontSize(double baseSize)
        {
            var sizeMultiplier = GetSizeMultiplier();
            var densityMultiplier = GetDensityMultiplier();
            
            return Math.Max(8, Math.Min(24, baseSize * sizeMultiplier * densityMultiplier));
        }

        // Get responsive padding/margin based on screen size
        public Thickness GetResponsivePadding(double basePadding)
        {
            var multiplier = GetSizeMultiplier();
            var padding = basePadding * multiplier;
            
            return new Thickness(padding);
        }

        // Get responsive button size
        public System.Windows.Size GetResponsiveButtonSize(double baseWidth, double baseHeight)
        {
            var sizeMultiplier = GetSizeMultiplier();
            var densityMultiplier = GetDensityMultiplier();
            
            return new System.Windows.Size(
                Math.Max(60, baseWidth * sizeMultiplier * densityMultiplier),
                Math.Max(30, baseHeight * sizeMultiplier * densityMultiplier)
            );
        }

        // Get responsive grid column width
        public GridLength GetResponsiveColumnWidth(double baseWidth, bool isStar = false)
        {
            if (isStar)
                return new GridLength(baseWidth, GridUnitType.Star);
            
            var sizeMultiplier = GetSizeMultiplier();
            var densityMultiplier = GetDensityMultiplier();
            
            return new GridLength(baseWidth * sizeMultiplier * densityMultiplier);
        }

        // Get responsive window size
        public System.Windows.Size GetResponsiveWindowSize(double baseWidth, double baseHeight)
        {
            var sizeMultiplier = GetSizeMultiplier();
            
            return new System.Windows.Size(
                Math.Max(800, baseWidth * sizeMultiplier),
                Math.Max(600, baseHeight * sizeMultiplier)
            );
        }

        // Get minimum window size for current screen
        public System.Windows.Size GetMinimumWindowSize()
        {
            switch (CurrentScreenSize)
            {
                case ScreenSize.Small:
                    return new System.Windows.Size(800, 600);
                case ScreenSize.Medium:
                    return new System.Windows.Size(1000, 700);
                case ScreenSize.Large:
                    return new System.Windows.Size(1200, 800);
                case ScreenSize.ExtraLarge:
                    return new System.Windows.Size(1400, 900);
                default:
                    return new System.Windows.Size(1000, 700);
            }
        }

        // Get optimal window size for current screen
        public System.Windows.Size GetOptimalWindowSize()
        {
            switch (CurrentScreenSize)
            {
                case ScreenSize.Small:
                    return new System.Windows.Size(1000, 700);
                case ScreenSize.Medium:
                    return new System.Windows.Size(1200, 800);
                case ScreenSize.Large:
                    return new System.Windows.Size(1400, 900);
                case ScreenSize.ExtraLarge:
                    return new System.Windows.Size(1600, 1000);
                default:
                    return new System.Windows.Size(1200, 800);
            }
        }

        // Apply responsive design to a window
        public void ApplyResponsiveDesign(Window window)
        {
            var optimalSize = GetOptimalWindowSize();
            var minSize = GetMinimumWindowSize();

            window.MinWidth = minSize.Width;
            window.MinHeight = minSize.Height;
            
            // Only set size if window is not already sized
            if (window.Width < minSize.Width || window.Height < minSize.Height)
            {
                window.Width = optimalSize.Width;
                window.Height = optimalSize.Height;
            }

            // Center window on screen
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        // Apply responsive design to a control
        public void ApplyResponsiveDesign(System.Windows.Controls.Control control)
        {
            if (control is WpfButton button)
            {
                var buttonSize = GetResponsiveButtonSize(80, 35);
                button.MinWidth = buttonSize.Width;
                button.MinHeight = buttonSize.Height;
                
                var padding = GetResponsivePadding(10);
                button.Padding = padding;
                
                var fontSize = GetResponsiveFontSize(12);
                button.FontSize = fontSize;
            }
            else if (control is WpfTextBox textBox)
            {
                var fontSize = GetResponsiveFontSize(12);
                textBox.FontSize = fontSize;
                
                var padding = GetResponsivePadding(8);
                textBox.Padding = padding;
            }
            else if (control is WpfLabel label)
            {
                var fontSize = GetResponsiveFontSize(12);
                label.FontSize = fontSize;
            }
            else if (control is WpfComboBox comboBox)
            {
                var fontSize = GetResponsiveFontSize(12);
                comboBox.FontSize = fontSize;
                
                var padding = GetResponsivePadding(6);
                comboBox.Padding = padding;
            }
        }

        // Get size multiplier based on screen size
        private double GetSizeMultiplier()
        {
            switch (CurrentScreenSize)
            {
                case ScreenSize.Small:
                    return 0.8;
                case ScreenSize.Medium:
                    return 1.0;
                case ScreenSize.Large:
                    return 1.2;
                case ScreenSize.ExtraLarge:
                    return 1.4;
                default:
                    return 1.0;
            }
        }

        // Get density multiplier based on screen density
        private double GetDensityMultiplier()
        {
            switch (CurrentScreenDensity)
            {
                case ScreenDensity.Low:
                    return 0.9;
                case ScreenDensity.Normal:
                    return 1.0;
                case ScreenDensity.High:
                    return 1.1;
                case ScreenDensity.ExtraHigh:
                    return 1.2;
                default:
                    return 1.0;
            }
        }

        // Check if current screen supports compact layout
        public bool SupportsCompactLayout()
        {
            return CurrentScreenSize == ScreenSize.Small || CurrentScreenSize == ScreenSize.Medium;
        }

        // Check if current screen supports expanded layout
        public bool SupportsExpandedLayout()
        {
            return CurrentScreenSize == ScreenSize.Large || CurrentScreenSize == ScreenSize.ExtraLarge;
        }

        // Get responsive grid definition for main layout
        public Grid GetResponsiveMainGrid()
        {
            var grid = new Grid();
            
            if (SupportsCompactLayout())
            {
                // Compact layout: single column
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer
            }
            else
            {
                // Expanded layout: sidebar + main content
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GetResponsiveColumnWidth(300) }); // Sidebar
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) }); // Spacer
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Main content
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer
            }
            
            return grid;
        }

        // Get responsive header height
        public double GetResponsiveHeaderHeight()
        {
            return GetResponsiveFontSize(50);
        }

        // Get responsive sidebar width
        public double GetResponsiveSidebarWidth()
        {
            if (SupportsCompactLayout())
                return 0; // No sidebar in compact layout
            else
            {
                var sizeMultiplier = GetSizeMultiplier();
                var densityMultiplier = GetDensityMultiplier();
                return 300 * sizeMultiplier * densityMultiplier;
            }
        }
        
        // Method to reinitialize screen properties after application is loaded
        public void ReinitializeScreenProperties()
        {
            try
            {
                InitializeScreenProperties();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveDesignService: Error reinitializing screen properties: {ex.Message}");
            }
        }
    }
} 