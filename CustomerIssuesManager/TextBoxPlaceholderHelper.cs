using System.Windows;
using System.Windows.Controls;

namespace CustomerIssuesManager
{
    public static class TextBoxPlaceholderHelper
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(TextBoxPlaceholderHelper),
                new PropertyMetadata(null, OnPlaceholderTextChanged)
            );

        public static string GetPlaceholderText(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderTextProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.TextBox textBox)
            {
                textBox.Loaded += (s, args) => ShowPlaceholder(textBox);
                textBox.TextChanged += (s, args) => ShowPlaceholder(textBox);
            }
        }

        private static void ShowPlaceholder(System.Windows.Controls.TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                var placeholder = GetPlaceholder(textBox);
                if (placeholder == null)
                {
                    placeholder = new TextBlock
                    {
                        Text = GetPlaceholderText(textBox),
                        FontStyle = FontStyles.Italic,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        IsHitTestVisible = false,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    SetPlaceholder(textBox, placeholder);
                    var grid = textBox.Parent as Grid;
                    if (grid != null)
                    {
                        grid.Children.Add(placeholder);
                    }
                }
                placeholder.Visibility = Visibility.Visible;
            }
            else
            {
                var placeholder = GetPlaceholder(textBox);
                if (placeholder != null)
                {
                    placeholder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached("Placeholder", typeof(TextBlock), typeof(TextBoxPlaceholderHelper), new PropertyMetadata(null));

        private static TextBlock GetPlaceholder(DependencyObject obj)
        {
            return (TextBlock)obj.GetValue(PlaceholderProperty);
        }

        private static void SetPlaceholder(DependencyObject obj, TextBlock value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }
    }
}
