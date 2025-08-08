using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace CustomerIssuesManager
{
    public partial class SettingsTestWindow : Window
    {
        private readonly ISettingsService _settingsService;
        private readonly NotificationService _notificationService;

        public SettingsTestWindow(ISettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _notificationService = new NotificationService(this);
        }

        private async void TestExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _settingsService.ExportSettingsAsync("test_settings.json");
                if (success)
                {
                    _notificationService.ShowNotification("تم تصدير الإعدادات بنجاح", NotificationType.Success);
                }
                else
                {
                    _notificationService.ShowNotification("فشل في تصدير الإعدادات", NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في التصدير: {ex.Message}", NotificationType.Error);
            }
        }

        private async void TestImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _settingsService.ImportSettingsAsync("test_settings.json");
                if (success)
                {
                    _notificationService.ShowNotification("تم استيراد الإعدادات بنجاح", NotificationType.Success);
                }
                else
                {
                    _notificationService.ShowNotification("فشل في استيراد الإعدادات", NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في الاستيراد: {ex.Message}", NotificationType.Error);
            }
        }

        private void TestAttachmentMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentMode = _settingsService.GetAttachmentManagementMode();
                string newMode = currentMode == "KeepOriginal" ? "MoveToFolder" : "KeepOriginal";
                
                _settingsService.SetAttachmentManagementMode(newMode);
                
                _notificationService.ShowNotification($"تم تغيير وضع المرفقات من '{currentMode}' إلى '{newMode}'", NotificationType.Info);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تغيير وضع المرفقات: {ex.Message}", NotificationType.Error);
            }
        }

        private void TestOrganizeByDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool currentValue = _settingsService.GetOrganizeByDate();
                bool newValue = !currentValue;
                
                _settingsService.SetOrganizeByDate(newValue);
                
                _notificationService.ShowNotification($"تم {((newValue ? "تفعيل" : "إلغاء"))} تنظيم المرفقات حسب التاريخ", NotificationType.Info);
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تغيير تنظيم التاريخ: {ex.Message}", NotificationType.Error);
            }
        }

        private void TestValidation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isValid = _settingsService.ValidateSettings();
                if (isValid)
                {
                    _notificationService.ShowNotification("جميع الإعدادات صحيحة", NotificationType.Success);
                }
                else
                {
                    var errors = _settingsService.GetValidationErrors();
                    string errorMessage = "أخطاء في الإعدادات:\n\n" + string.Join("\n", errors);
                    _notificationService.ShowNotification(errorMessage, NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في التحقق من الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }

        private void TestInvalidSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set invalid settings to test validation
                _settingsService.SetBackupCount(-1); // Invalid value
                _settingsService.SetBackupRetention(400); // Invalid value
                
                bool isValid = _settingsService.ValidateSettings();
                if (!isValid)
                {
                    var errors = _settingsService.GetValidationErrors();
                    string errorMessage = "تم اكتشاف أخطاء في الإعدادات:\n\n" + string.Join("\n", errors);
                    _notificationService.ShowNotification(errorMessage, NotificationType.Warning);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في اختبار الإعدادات غير الصحيحة: {ex.Message}", NotificationType.Error);
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow(
                    ((App)App.Current).Services.GetRequiredService<IBackupService>(),
                    ((App)App.Current).Services.GetRequiredService<IFileManagerService>(),
                    ((App)App.Current).Services.GetRequiredService<ISettingsService>()
                );
                settingsWindow.Show();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في فتح إعدادات النظام: {ex.Message}", NotificationType.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 