using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfLabel = System.Windows.Controls.Label;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace CustomerIssuesManager.Services
{
    public class ResponsiveLayoutManager
    {
        private static ResponsiveLayoutManager _instance = null!;
        private static readonly object _lock = new object();
        private readonly ResponsiveDesignService _responsiveService;
        private readonly Dictionary<Window, ResponsiveLayoutInfo> _windowLayouts;

        public static ResponsiveLayoutManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ResponsiveLayoutManager();
                    }
                }
                return _instance;
            }
        }

        private ResponsiveLayoutManager()
        {
            _responsiveService = ResponsiveDesignService.Instance;
            _windowLayouts = new Dictionary<Window, ResponsiveLayoutInfo>();
        }

        public void RegisterWindow(Window window)
        {
            if (!_windowLayouts.ContainsKey(window))
            {
                var layoutInfo = new ResponsiveLayoutInfo
                {
                    Window = window,
                    OriginalSize = new System.Windows.Size(window.Width, window.Height),
                    OriginalMinSize = new System.Windows.Size(window.MinWidth, window.MinHeight)
                };

                _windowLayouts[window] = layoutInfo;

                // Subscribe to window events
                window.SizeChanged += Window_SizeChanged;
                window.StateChanged += Window_StateChanged;
                window.Loaded += Window_Loaded;
            }
        }

        public void UnregisterWindow(Window window)
        {
            if (_windowLayouts.ContainsKey(window))
            {
                window.SizeChanged -= Window_SizeChanged;
                window.StateChanged -= Window_StateChanged;
                window.Loaded -= Window_Loaded;
                _windowLayouts.Remove(window);
            }
        }

        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                ApplyResponsiveLayout(window);
            }
        }

        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (sender is Window window)
            {
                AdaptLayoutToSize(window, e.NewSize);
            }
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                AdaptLayoutToWindowState(window);
            }
        }

        private void ApplyResponsiveLayout(Window window)
        {
            try
            {
                // Apply responsive design to the window
                _responsiveService.ApplyResponsiveDesign(window);

                // Apply responsive design to all controls
                ApplyResponsiveDesignToControls(window);

                // Setup responsive layout
                SetupResponsiveLayout(window);

                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Applied responsive layout to {window.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error applying responsive layout: {ex.Message}");
            }
        }

        private void ApplyResponsiveDesignToControls(Window window)
        {
            try
            {
                // Apply responsive design to all buttons
                var buttons = FindVisualChildren<WpfButton>(window);
                foreach (var button in buttons)
                {
                    _responsiveService.ApplyResponsiveDesign(button);
                }

                // Apply responsive design to all text boxes
                var textBoxes = FindVisualChildren<WpfTextBox>(window);
                foreach (var textBox in textBoxes)
                {
                    _responsiveService.ApplyResponsiveDesign(textBox);
                }

                // Apply responsive design to all labels
                var labels = FindVisualChildren<WpfLabel>(window);
                foreach (var label in labels)
                {
                    _responsiveService.ApplyResponsiveDesign(label);
                }

                // Apply responsive design to all combo boxes
                var comboBoxes = FindVisualChildren<WpfComboBox>(window);
                foreach (var comboBox in comboBoxes)
                {
                    _responsiveService.ApplyResponsiveDesign(comboBox);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error applying responsive design to controls: {ex.Message}");
            }
        }

        private void SetupResponsiveLayout(Window window)
        {
            try
            {
                // Get the main grid
                var mainGrid = window.FindName("MainGrid") as Grid;
                if (mainGrid != null)
                {
                    // Clear existing definitions
                    mainGrid.ColumnDefinitions.Clear();
                    mainGrid.RowDefinitions.Clear();

                    // Apply responsive grid layout
                    var responsiveGrid = _responsiveService.GetResponsiveMainGrid();
                    
                    // Copy definitions from responsive grid
                    foreach (var colDef in responsiveGrid.ColumnDefinitions)
                    {
                        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = colDef.Width });
                    }
                    
                    foreach (var rowDef in responsiveGrid.RowDefinitions)
                    {
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = rowDef.Height });
                    }
                }

                // Update header height
                var headerBorder = window.FindName("HeaderBorder") as Border;
                if (headerBorder != null)
                {
                    headerBorder.Height = _responsiveService.GetResponsiveHeaderHeight();
                }

                // Update sidebar width if exists
                var sidebarBorder = window.FindName("SidebarBorder") as Border;
                if (sidebarBorder != null && !_responsiveService.SupportsCompactLayout())
                {
                    sidebarBorder.Width = _responsiveService.GetResponsiveSidebarWidth();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error setting up responsive layout: {ex.Message}");
            }
        }

        private void AdaptLayoutToSize(Window window, System.Windows.Size newSize)
        {
            try
            {
                var layoutInfo = _windowLayouts[window];
                
                // Determine if we need to switch to compact layout
                if (newSize.Width < 1200 && !_responsiveService.SupportsCompactLayout())
                {
                    // Switch to compact layout
                    SwitchToCompactLayout(window);
                }
                else if (newSize.Width >= 1200 && _responsiveService.SupportsCompactLayout())
                {
                    // Switch to expanded layout
                    SwitchToExpandedLayout(window);
                }

                // Update control sizes based on new window size
                UpdateControlSizes(window, newSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error adapting layout to size: {ex.Message}");
            }
        }

        private void AdaptLayoutToWindowState(Window window)
        {
            try
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    // Use expanded layout when maximized
                    SwitchToExpandedLayout(window);
                }
                else if (window.WindowState == WindowState.Normal)
                {
                    // Check if we should use compact layout
                    if (window.Width < 1200)
                    {
                        SwitchToCompactLayout(window);
                    }
                    else
                    {
                        SwitchToExpandedLayout(window);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error adapting layout to window state: {ex.Message}");
            }
        }

        private void SwitchToCompactLayout(Window window)
        {
            try
            {
                // Hide sidebar
                var sidebarBorder = window.FindName("SidebarBorder") as Border;
                if (sidebarBorder != null)
                {
                    sidebarBorder.Visibility = Visibility.Collapsed;
                }

                // Adjust main grid layout
                var mainGrid = window.FindName("MainGrid") as Grid;
                if (mainGrid != null && mainGrid.ColumnDefinitions.Count >= 3)
                {
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(0); // Hide sidebar
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(0); // Hide spacer
                    mainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star); // Expand main content
                }

                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Switched to compact layout for {window.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error switching to compact layout: {ex.Message}");
            }
        }

        private void SwitchToExpandedLayout(Window window)
        {
            try
            {
                // Show sidebar
                var sidebarBorder = window.FindName("SidebarBorder") as Border;
                if (sidebarBorder != null)
                {
                    sidebarBorder.Visibility = Visibility.Visible;
                }

                // Adjust main grid layout
                var mainGrid = window.FindName("MainGrid") as Grid;
                if (mainGrid != null && mainGrid.ColumnDefinitions.Count >= 3)
                {
                    mainGrid.ColumnDefinitions[0].Width = _responsiveService.GetResponsiveColumnWidth(300); // Show sidebar
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(5); // Show spacer
                    mainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star); // Main content
                }

                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Switched to expanded layout for {window.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error switching to expanded layout: {ex.Message}");
            }
        }

        private void UpdateControlSizes(Window window, System.Windows.Size windowSize)
        {
            try
            {
                // Update font sizes based on window size
                var scaleFactor = Math.Min(windowSize.Width / 1400, windowSize.Height / 900);
                scaleFactor = Math.Max(0.8, Math.Min(1.4, scaleFactor));

                // Update text elements
                var textBlocks = FindVisualChildren<TextBlock>(window);
                foreach (var textBlock in textBlocks)
                {
                    if (textBlock.FontSize > 0)
                    {
                        textBlock.FontSize = Math.Max(8, textBlock.FontSize * scaleFactor);
                    }
                }

                // Update buttons
                var buttons = FindVisualChildren<WpfButton>(window);
                foreach (var button in buttons)
                {
                    var padding = button.Padding;
                    button.Padding = new Thickness(
                        padding.Left * scaleFactor,
                        padding.Top * scaleFactor,
                        padding.Right * scaleFactor,
                        padding.Bottom * scaleFactor
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResponsiveLayoutManager: Error updating control sizes: {ex.Message}");
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                    yield return t;
                
                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        private class ResponsiveLayoutInfo
        {
            public Window Window { get; set; } = null!;
            public System.Windows.Size OriginalSize { get; set; }
            public System.Windows.Size OriginalMinSize { get; set; }
            public bool IsCompactLayout { get; set; }
        }
    }
} 