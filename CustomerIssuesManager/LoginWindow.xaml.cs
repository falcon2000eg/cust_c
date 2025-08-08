using CustomerIssuesManager.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMessageBox = System.Windows.MessageBox;
using WpfApplication = System.Windows.Application;

namespace CustomerIssuesManager;
public partial class LoginWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthService _authService;

    public LoginWindow(IServiceProvider serviceProvider, IAuthService authService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = authService;
        
        // Focus on performance number textbox
        PerformanceNumberTextBox.Focus();
        
        // Handle Enter key press
        this.KeyDown += LoginWindow_KeyDown;
    }

    private void LoginWindow_KeyDown(object sender, WpfKeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LoginButton_Click(sender, new RoutedEventArgs());
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string perfText = PerformanceNumberTextBox.Text.Trim();
            
            // Validate input
            if (string.IsNullOrWhiteSpace(perfText))
            {
                WpfMessageBox.Show("الرجاء إدخال رقم الأداء.", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                PerformanceNumberTextBox.Focus();
                return;
            }

            // Validate that input contains only digits
            if (!perfText.All(char.IsDigit))
            {
                WpfMessageBox.Show("رقم الأداء يجب أن يحتوي على أرقام فقط.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Error);
                PerformanceNumberTextBox.Focus();
                return;
            }

            if (!int.TryParse(perfText, out int performanceNumber))
            {
                WpfMessageBox.Show("رقم الأداء يجب أن يكون عددًا صحيحًا.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                PerformanceNumberTextBox.Focus();
                return;
            }

            // Validate performance number range
            if (performanceNumber <= 0)
            {
                WpfMessageBox.Show("رقم الأداء يجب أن يكون أكبر من صفر.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Error);
                PerformanceNumberTextBox.Focus();
                return;
            }

            try
            {
                // Special handling for admin (performance number 1)
                var employee = await _authService.LoginByPerformanceNumberAsync(perfText);

                if (employee != null)
                {
                    // Store the logged-in user's information
                    App.SetCurrentUser(employee);

                    // Open the dashboard window after login
                    var dashboardWindow = _serviceProvider.GetRequiredService<DashboardWindow>();
                    dashboardWindow.Show();

                    // Close the login window
                    this.Close();
                }
                else
                {
                    WpfMessageBox.Show("رقم الأداء غير صحيح أو الحساب غير نشط.", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                    PerformanceNumberTextBox.Focus();
                    PerformanceNumberTextBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"حدث خطأ أثناء التحقق من الدخول:\n{ex.Message}", "خطأ في النظام", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"حدث خطأ غير متوقع:\n{ex.Message}", "خطأ في النظام", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        WpfApplication.Current.Shutdown();
    }
}
