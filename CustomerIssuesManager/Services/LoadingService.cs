using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using WpfColor = System.Windows.Media.Color;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;
using WpfTextAlignment = System.Windows.TextAlignment;
using WpfFontWeights = System.Windows.FontWeights;

namespace CustomerIssuesManager.Services
{
    public class LoadingService
    {
        private readonly Window _parentWindow;
        private Grid? _loadingOverlay;
        private TextBlock? _loadingText;
        private System.Windows.Controls.ProgressBar? _progressBar;

        public LoadingService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        // Simple methods for compatibility with task specification
        public void ShowLoading(string message = "جاري التحميل...")
        {
            ShowLoadingOverlay(message);
        }

        public void HideLoading()
        {
            HideLoadingOverlay();
        }

        public void ShowLoadingOverlay(string message = "جاري التحميل...", bool showProgress = false)
        {
            // Hide any existing overlay
            HideLoadingOverlay();

            // Create overlay grid
            _loadingOverlay = new Grid
            {
                Background = new SolidColorBrush(WpfColor.FromArgb(180, 0, 0, 0)),
                HorizontalAlignment = WpfHorizontalAlignment.Stretch,
                VerticalAlignment = WpfVerticalAlignment.Stretch
            };
            Grid.SetZIndex(_loadingOverlay, 1000);

            // Create content panel
            var contentPanel = new Border
            {
                HorizontalAlignment = WpfHorizontalAlignment.Center,
                VerticalAlignment = WpfVerticalAlignment.Center,
                Background = new SolidColorBrush(WpfColor.FromRgb(255, 255, 255)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(30),
                MinWidth = 300
            };

            // Add shadow effect
            contentPanel.Effect = new DropShadowEffect
            {
                Color = WpfColor.FromArgb(100, 0, 0, 0),
                Direction = 270,
                ShadowDepth = 8,
                BlurRadius = 16
            };

            // Loading icon (spinning)
            var iconText = new TextBlock
            {
                Text = "⏳",
                FontSize = 32,
                HorizontalAlignment = WpfHorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };

            // Animate the icon
            var rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            iconText.RenderTransform = new RotateTransform();
            iconText.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(iconText);

            // Loading text
            _loadingText = new TextBlock
            {
                Text = message,
                FontSize = 16,
                FontWeight = WpfFontWeights.Medium,
                HorizontalAlignment = WpfHorizontalAlignment.Center,
                TextAlignment = WpfTextAlignment.Center,
                Foreground = new SolidColorBrush(WpfColor.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 16)
            };
            stackPanel.Children.Add(_loadingText);

            // Progress bar (optional)
            if (showProgress)
            {
                _progressBar = new System.Windows.Controls.ProgressBar
                {
                    IsIndeterminate = true,
                    Height = 4,
                    Margin = new Thickness(0, 8, 0, 0),
                    Background = new SolidColorBrush(WpfColor.FromRgb(229, 231, 235)),
                    Foreground = new SolidColorBrush(WpfColor.FromRgb(59, 130, 246))
                };
                stackPanel.Children.Add(_progressBar);
            }

            contentPanel.Child = stackPanel;

            _loadingOverlay.Children.Add(contentPanel);

            // Add to parent window
            if (_parentWindow.Content is Grid mainGrid)
            {
                mainGrid.Children.Add(_loadingOverlay);
            }

            // Animate in
            AnimateIn(_loadingOverlay);
        }

        public void UpdateLoadingMessage(string message)
        {
            if (_loadingText != null)
            {
                _loadingText.Text = message;
            }
        }

        public void ShowProgress(double progress, string? message = null)
        {
            if (_progressBar != null)
            {
                _progressBar.IsIndeterminate = false;
                _progressBar.Value = progress;
            }

            if (message != null && _loadingText != null)
            {
                _loadingText.Text = message;
            }
        }

        public void HideLoadingOverlay()
        {
            if (_loadingOverlay != null)
            {
                try
                {
                    // Remove from parent
                    if (_loadingOverlay.Parent is Grid parentGrid)
                    {
                        parentGrid.Children.Remove(_loadingOverlay);
                    }
                    
                    // Dispose resources
                    _loadingOverlay.Children.Clear();
                    _loadingOverlay = null;
                    _loadingText = null;
                    _progressBar = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error hiding loading overlay: {ex.Message}");
                }
            }
        }

        private void AnimateIn(Grid overlay)
        {
            overlay.Opacity = 0;
            overlay.RenderTransform = new ScaleTransform(0.8, 0.8);

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var scaleAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            overlay.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            overlay.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            overlay.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        private void AnimateOut(Grid overlay, Action onComplete)
        {
            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var scaleAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            opacityAnimation.Completed += (s, e) => onComplete?.Invoke();

            overlay.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            overlay.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            overlay.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        public async Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation, string loadingMessage = "جاري التحميل...")
        {
            ShowLoadingOverlay(loadingMessage);
            
            try
            {
                var result = await operation();
                return result;
            }
            finally
            {
                HideLoadingOverlay();
            }
        }

        public async Task ExecuteWithLoadingAsync(Func<Task> operation, string loadingMessage = "جاري التحميل...")
        {
            ShowLoadingOverlay(loadingMessage);
            
            try
            {
                await operation();
            }
            finally
            {
                HideLoadingOverlay();
            }
        }
    }
} 