using CustomerIssuesManager.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using WpfApplication = System.Windows.Application;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfSolidColorBrush = System.Windows.Media.SolidColorBrush;
using CoreNotificationType = CustomerIssuesManager.Core.Services.NotificationType;

namespace CustomerIssuesManager.Services
{
    public class NotificationService : INotificationService
    {
        private readonly Window _parentWindow;
        private Border? _notificationBorder;
        private TextBlock? _notificationText;
        private DispatcherTimer? _hideTimer;

        public NotificationService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        // Enhanced notification methods with better error details and actionable messages
        public void ShowSuccess(string message, string? details = null)
        {
            var fullMessage = details != null ? $"{message}\n\nالتفاصيل: {details}" : message;
            ShowNotification(fullMessage, CoreNotificationType.Success);
        }
        
        public void ShowError(string message, string? details = null, string? actionHint = null)
        {
            var fullMessage = message;
            if (details != null)
            {
                fullMessage += $"\n\nالتفاصيل: {details}";
            }
            if (actionHint != null)
            {
                fullMessage += $"\n\nاقتراح الحل: {actionHint}";
            }
            ShowNotification(fullMessage, CoreNotificationType.Error);
        }
        
        public void ShowWarning(string message, string? details = null, string? actionHint = null)
        {
            var fullMessage = message;
            if (details != null)
            {
                fullMessage += $"\n\nالتفاصيل: {details}";
            }
            if (actionHint != null)
            {
                fullMessage += $"\n\nاقتراح الحل: {actionHint}";
            }
            ShowNotification(fullMessage, CoreNotificationType.Warning);
        }

        public void ShowInfo(string message) => ShowNotification(message, CoreNotificationType.Info);

        // Enhanced error handling with specific error types
        public void ShowDatabaseError(string operation, Exception ex)
        {
            var message = $"فشل في {operation}";
            var details = $"خطأ في قاعدة البيانات: {ex.Message}";
            var actionHint = "تأكد من اتصال قاعدة البيانات وحاول مرة أخرى";
            ShowError(message, details, actionHint);
        }

        public void ShowFileError(string operation, Exception ex)
        {
            var message = $"فشل في {operation}";
            var details = $"خطأ في الملف: {ex.Message}";
            var actionHint = "تأكد من وجود الملف وصلاحيات الوصول إليه";
            ShowError(message, details, actionHint);
        }

        public void ShowNetworkError(string operation, Exception ex)
        {
            var message = $"فشل في {operation}";
            var details = $"خطأ في الشبكة: {ex.Message}";
            var actionHint = "تأكد من اتصال الإنترنت وحاول مرة أخرى";
            ShowError(message, details, actionHint);
        }

        public void ShowValidationError(string field, string message)
        {
            var errorMessage = $"خطأ في التحقق من صحة البيانات";
            var details = $"الحقل: {field}\nالخطأ: {message}";
            var actionHint = "يرجى تصحيح البيانات وإعادة المحاولة";
            ShowError(errorMessage, details, actionHint);
        }

        public void ShowPermissionError(string operation)
        {
            var message = $"فشل في {operation}";
            var details = "ليس لديك صلاحية لتنفيذ هذه العملية";
            var actionHint = "تواصل مع المسؤول للحصول على الصلاحيات المطلوبة";
            ShowError(message, details, actionHint);
        }

        public void ShowSuccess(string message) => ShowNotification(message, CoreNotificationType.Success);
        public void ShowError(string message) => ShowNotification(message, CoreNotificationType.Error);
        public void ShowWarning(string message) => ShowNotification(message, CoreNotificationType.Warning);

        public void ShowNotification(string message, CoreNotificationType type = CoreNotificationType.Info, int duration = 4000)
        {
            // Hide any existing notification
            HideNotification();

            // Create notification configuration
            var config = GetNotificationConfig(type);

            // Create notification container
            _notificationBorder = new Border
            {
                Background = new WpfSolidColorBrush(config.BackgroundColor),
                BorderBrush = new WpfSolidColorBrush(config.BorderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                MaxWidth = 500, // Increased width for longer messages
                HorizontalAlignment = WpfHorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0),
                Effect = new DropShadowEffect
                {
                    Color = WpfColor.FromArgb(100, 0, 0, 0),
                    Direction = 270,
                    ShadowDepth = 4,
                    BlurRadius = 8
                }
            };

            // Create content grid
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Icon
            var iconText = new TextBlock
            {
                Text = config.Icon,
                FontSize = 20,
                Foreground = WpfBrushes.White,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(iconText, 0);
            grid.Children.Add(iconText);

            // Content stack panel
            var contentPanel = new StackPanel();
            Grid.SetColumn(contentPanel, 1);

            // Title
            var titleText = new TextBlock
            {
                Text = config.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = WpfBrushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            };
            contentPanel.Children.Add(titleText);

            // Message
            _notificationText = new TextBlock
            {
                Text = message,
                FontSize = 12,
                Foreground = WpfBrushes.White,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 16
            };
            contentPanel.Children.Add(_notificationText);

            grid.Children.Add(contentPanel);
            _notificationBorder.Child = grid;

            // Add to parent window - use a more flexible approach
            if (_parentWindow.Content is Grid mainGrid)
            {
                // Try to add to the first row and span all columns
                try
                {
                    Grid.SetRow(_notificationBorder, 0);
                    if (mainGrid.ColumnDefinitions.Count > 0)
                    {
                        Grid.SetColumnSpan(_notificationBorder, mainGrid.ColumnDefinitions.Count);
                    }
                    mainGrid.Children.Add(_notificationBorder);
                }
                catch
                {
                    // If that fails, just add it as a child
                    mainGrid.Children.Add(_notificationBorder);
                }
            }
            else if (_parentWindow.Content is System.Windows.Controls.Panel panel)
            {
                // For other panel types, just add as child
                panel.Children.Add(_notificationBorder);
            }

            // Animate in
            AnimateIn(_notificationBorder);

            // Set timer to hide (only if duration > 0)
            if (duration > 0)
            {
                _hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(duration)
                };
                _hideTimer.Tick += (s, e) => HideNotification();
                _hideTimer.Start();
            }
        }

        public void HideNotification()
        {
            if (_notificationBorder != null)
            {
                AnimateOut(_notificationBorder, () =>
                {
                    if (_parentWindow.Content is Grid mainGrid)
                    {
                        mainGrid.Children.Remove(_notificationBorder);
                    }
                    else if (_parentWindow.Content is System.Windows.Controls.Panel panel)
                    {
                        panel.Children.Remove(_notificationBorder);
                    }
                    _notificationBorder = null;
                    _notificationText = null;
                });
            }

            _hideTimer?.Stop();
            _hideTimer = null;
        }

        // Interface implementation methods
        public async Task ShowNotificationAsync(string message, CoreNotificationType type = CoreNotificationType.Info)
        {
            await WpfApplication.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowNotification(message, type);
            });
        }

        public async Task ShowLoadingIndicatorAsync(string message = "جاري التحميل...")
        {
            await WpfApplication.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowNotification(message, CoreNotificationType.Info, 0); // No auto-hide
            });
        }

        public async Task HideLoadingIndicatorAsync()
        {
            await WpfApplication.Current.Dispatcher.InvokeAsync(() =>
            {
                HideNotification();
            });
        }

        private void AnimateIn(Border notification)
        {
            notification.Opacity = 0;
            notification.RenderTransform = new TranslateTransform(0, -50);

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var translateAnimation = new DoubleAnimation
            {
                From = -50,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            notification.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            notification.RenderTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
        }

        private void AnimateOut(Border notification, Action onComplete)
        {
            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var translateAnimation = new DoubleAnimation
            {
                From = 0,
                To = -30,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            opacityAnimation.Completed += (s, e) => onComplete?.Invoke();

            notification.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            notification.RenderTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
        }

        private NotificationConfig GetNotificationConfig(CoreNotificationType type)
        {
            return type switch
            {
                CoreNotificationType.Success => new NotificationConfig
                {
                    BackgroundColor = WpfColor.FromRgb(16, 185, 129), // #10b981
                    BorderColor = WpfColor.FromRgb(5, 150, 105),
                    Icon = "✅",
                    Title = "نجح"
                },
                CoreNotificationType.Error => new NotificationConfig
                {
                    BackgroundColor = WpfColor.FromRgb(239, 68, 68), // #ef4444
                    BorderColor = WpfColor.FromRgb(220, 38, 38),
                    Icon = "❌",
                    Title = "خطأ"
                },
                CoreNotificationType.Warning => new NotificationConfig
                {
                    BackgroundColor = WpfColor.FromRgb(245, 158, 11), // #f59e0b
                    BorderColor = WpfColor.FromRgb(217, 119, 6),
                    Icon = "⚠️",
                    Title = "تحذير"
                },
                _ => new NotificationConfig
                {
                    BackgroundColor = WpfColor.FromRgb(59, 130, 246), // #3b82f6
                    BorderColor = WpfColor.FromRgb(37, 99, 235),
                    Icon = "ℹ️",
                    Title = "معلومات"
                }
            };
        }

        private class NotificationConfig
        {
            public WpfColor BackgroundColor { get; set; }
            public WpfColor BorderColor { get; set; }
            public string Icon { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
        }
    }
} 