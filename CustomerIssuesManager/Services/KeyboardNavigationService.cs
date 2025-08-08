using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation;
using System.Collections.Generic;
using System.Linq;
using CustomerIssuesManager;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfControl = System.Windows.Controls.Control;
using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfRadioButton = System.Windows.Controls.RadioButton;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfDataGrid = System.Windows.Controls.DataGrid;
using WpfMenuItem = System.Windows.Controls.MenuItem;

namespace CustomerIssuesManager.Services
{
    public class KeyboardNavigationService
    {
        private readonly Window _window;
        private readonly NotificationService _notificationService;
        private readonly Dictionary<Key, Action> _shortcuts;
        private bool _highContrastMode = false;

        public KeyboardNavigationService(Window window, NotificationService notificationService)
        {
            _window = window;
            _notificationService = notificationService;
            _shortcuts = new Dictionary<Key, Action>();
            
            SetupKeyboardShortcuts();
            SetupAccessibility();
        }

        public void SetupKeyboardShortcuts(Window window)
        {
            window.KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.F1:
                        ShowHelp();
                        e.Handled = true;
                        break;
                        
                    case Key.F2:
                        QuickSearch();
                        e.Handled = true;
                        break;
                        
                    case Key.F3:
                        FindNext();
                        e.Handled = true;
                        break;
                        
                    case Key.F4:
                        FindPrevious();
                        e.Handled = true;
                        break;
                }
            };
        }

        private void SetupKeyboardShortcuts()
        {
            // Register global shortcuts
            _shortcuts[Key.F1] = ShowHelp;
            _shortcuts[Key.F2] = QuickSearch;
            _shortcuts[Key.F3] = FindNext;
            _shortcuts[Key.F4] = FindPrevious;

            // Setup window-level keyboard handling
            _window.KeyDown += Window_KeyDown;
            _window.PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void SetupAccessibility()
        {
            // Enable screen reader support
            AutomationProperties.SetIsOffscreenBehavior(_window, IsOffscreenBehavior.Default);
            
            // Setup tab navigation
            SetupTabNavigation();
            
            // Setup high contrast mode detection
            SetupHighContrastMode();
        }

        private void SetupTabNavigation()
        {
            // Ensure all interactive elements are tab-accessible
            var interactiveElements = FindInteractiveElements(_window);
            foreach (var element in interactiveElements)
            {
                if (element is WpfControl control)
                {
                    control.IsTabStop = true;
                    control.TabIndex = GetTabIndex(control);
                }
            }
        }

        private void SetupHighContrastMode()
        {
            // Check if high contrast mode is enabled
            _highContrastMode = SystemParameters.HighContrast;
            
            if (_highContrastMode)
            {
                ApplyHighContrastTheme();
            }
        }

        private void ApplyHighContrastTheme()
        {
            // Apply high contrast colors
            var highContrastBackground = new SolidColorBrush(Colors.Black);
            var highContrastForeground = new SolidColorBrush(Colors.White);
            var highContrastBorder = new SolidColorBrush(Colors.White);

            // Apply to window background
            _window.Background = highContrastBackground;
            
            // Apply to all controls
            ApplyHighContrastToControls(_window);
        }

        private void ApplyHighContrastToControls(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is WpfControl control)
                {
                    // Apply high contrast styling
                    if (control is WpfButton button)
                    {
                        button.Background = new SolidColorBrush(Colors.Black);
                        button.Foreground = new SolidColorBrush(Colors.White);
                        button.BorderBrush = new SolidColorBrush(Colors.White);
                    }
                    else if (control is WpfTextBox textBox)
                    {
                        textBox.Background = new SolidColorBrush(Colors.Black);
                        textBox.Foreground = new SolidColorBrush(Colors.White);
                        textBox.BorderBrush = new SolidColorBrush(Colors.White);
                    }
                    else if (control is WpfComboBox comboBox)
                    {
                        comboBox.Background = new SolidColorBrush(Colors.Black);
                        comboBox.Foreground = new SolidColorBrush(Colors.White);
                        comboBox.BorderBrush = new SolidColorBrush(Colors.White);
                    }
                }
                
                ApplyHighContrastToControls(child);
            }
        }

        private void Window_KeyDown(object sender, WpfKeyEventArgs e)
        {
            // Handle registered shortcuts
            if (_shortcuts.ContainsKey(e.Key))
            {
                _shortcuts[e.Key]();
                e.Handled = true;
                return;
            }

            // Handle additional navigation shortcuts
            switch (e.Key)
            {
                case Key.Tab:
                    HandleTabNavigation(e);
                    break;
                    
                case Key.Enter:
                    HandleEnterKey(e);
                    break;
                    
                case Key.Escape:
                    HandleEscapeKey(e);
                    break;
                    
                case Key.Space:
                    HandleSpaceKey(e);
                    break;
            }
        }

        private void Window_PreviewKeyDown(object sender, WpfKeyEventArgs e)
        {
            // Handle Ctrl+key combinations
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.F:
                        FocusSearchBox();
                        e.Handled = true;
                        break;
                        
                    case Key.N:
                        TriggerNewCase();
                        e.Handled = true;
                        break;
                        
                    case Key.S:
                        TriggerSave();
                        e.Handled = true;
                        break;
                        
                    case Key.O:
                        TriggerOpen();
                        e.Handled = true;
                        break;
                        
                    case Key.P:
                        TriggerPrint();
                        e.Handled = true;
                        break;
                        
                    case Key.R:
                        TriggerRefresh();
                        e.Handled = true;
                        break;
                        
                    case Key.H:
                        ShowHelp();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void HandleTabNavigation(WpfKeyEventArgs e)
        {
            // Custom tab navigation logic
            var currentElement = Keyboard.FocusedElement as FrameworkElement;
            if (currentElement != null)
            {
                var nextElement = GetNextTabElement(currentElement, e.Key == Key.Tab);
                if (nextElement != null)
                {
                    nextElement.Focus();
                    e.Handled = true;
                }
            }
        }

        private void HandleEnterKey(WpfKeyEventArgs e)
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement is WpfButton button)
            {
                button.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
                e.Handled = true;
            }
            else if (focusedElement is WpfTextBox textBox)
            {
                // For login window, trigger login button when Enter is pressed on textbox
                if (_window is LoginWindow)
                {
                    var loginButton = FindElementByName(_window, "LoginButton") as WpfButton;
                    loginButton?.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
                    e.Handled = true;
                }
            }
        }

        private void HandleEscapeKey(WpfKeyEventArgs e)
        {
            // Close dialogs or clear selection
            if (_window.OwnedWindows.Count > 0)
            {
                _window.OwnedWindows[0].Close();
                e.Handled = true;
            }
            else
            {
                ClearSelection();
                e.Handled = true;
            }
        }

        private void HandleSpaceKey(WpfKeyEventArgs e)
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement is WpfCheckBox checkBox)
            {
                checkBox.IsChecked = !checkBox.IsChecked;
                e.Handled = true;
            }
            else if (focusedElement is WpfButton button)
            {
                button.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
                e.Handled = true;
            }
        }

        private FrameworkElement GetNextTabElement(FrameworkElement currentElement, bool forward)
        {
            var allElements = FindInteractiveElements(_window)
                .Where(e => e.IsEnabled && e.IsVisible)
                .OrderBy(e => GetTabIndex(e))
                .ToList();

            var currentIndex = allElements.IndexOf(currentElement);
            if (currentIndex == -1) return allElements.FirstOrDefault() ?? currentElement;

            if (forward)
            {
                return allElements[(currentIndex + 1) % allElements.Count];
            }
            else
            {
                return allElements[currentIndex == 0 ? allElements.Count - 1 : currentIndex - 1];
            }
        }

        private int GetTabIndex(FrameworkElement element)
        {
            if (element is WpfControl control)
            {
                return control.TabIndex;
            }
            return 0;
        }

        private IEnumerable<FrameworkElement> FindInteractiveElements(DependencyObject parent)
        {
            var elements = new List<FrameworkElement>();
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is FrameworkElement element)
                {
                    if (IsInteractiveElement(element))
                    {
                        elements.Add(element);
                    }
                }
                
                elements.AddRange(FindInteractiveElements(child));
            }
            
            return elements;
        }

        private bool IsInteractiveElement(FrameworkElement element)
        {
            return element is WpfButton || element is WpfTextBox || element is WpfComboBox || 
                   element is WpfCheckBox || element is WpfRadioButton || element is WpfListBox ||
                   element is WpfDataGrid || element is WpfMenuItem;
        }

        // Shortcut action methods
        private void ShowHelp()
        {
            _notificationService?.ShowInfo("F1: عرض المساعدة");
            // TODO: Implement help system
        }

        private void QuickSearch()
        {
            _notificationService?.ShowInfo("F2: البحث السريع");
            FocusSearchBox();
        }

        private void FindNext()
        {
            _notificationService?.ShowInfo("F3: البحث التالي");
            // TODO: Implement find next functionality
        }

        private void FindPrevious()
        {
            _notificationService?.ShowInfo("F4: البحث السابق");
            // TODO: Implement find previous functionality
        }

        private void FocusSearchBox()
        {
            // Find search box in the window
            var searchBox = FindElementByName(_window, "QuickSearchBox") as WpfTextBox;
            if (searchBox != null)
            {
                searchBox.Focus();
                searchBox.SelectAll();
            }
        }

        private void TriggerNewCase()
        {
            var addButton = FindElementByName(_window, "AddButton") as WpfButton;
            addButton?.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
        }

        private void TriggerSave()
        {
            var saveButton = FindElementByName(_window, "SaveButton") as WpfButton;
            saveButton?.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
        }

        private void TriggerOpen()
        {
            // TODO: Implement open functionality
            _notificationService?.ShowInfo("Ctrl+O: فتح ملف");
        }

        private void TriggerPrint()
        {
            var printButton = FindElementByName(_window, "PrintButton") as WpfButton;
            printButton?.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
        }

        private void TriggerRefresh()
        {
            var refreshButton = FindElementByName(_window, "RefreshButton") as WpfButton;
            refreshButton?.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent));
        }

        private void ClearSelection()
        {
            // Clear any current selection
            if (_window is MainWindow mainWindow)
            {
                // Use reflection to call private method if needed
                var method = typeof(MainWindow).GetMethod("ClearSelection", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(mainWindow, null);
            }
        }

        private FrameworkElement? FindElementByName(DependencyObject parent, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is FrameworkElement element && element.Name == name)
                {
                    return element;
                }
                
                var result = FindElementByName(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            
            return null;
        }

        public void EnableHighContrastMode()
        {
            _highContrastMode = true;
            ApplyHighContrastTheme();
        }

        public void DisableHighContrastMode()
        {
            _highContrastMode = false;
            // TODO: Restore original theme
        }

        public bool IsHighContrastModeEnabled => _highContrastMode;
    }
} 