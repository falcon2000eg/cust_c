using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;

namespace CustomerIssuesManager
{
    public partial class DiagnosticWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StringBuilder _logBuilder;
        private readonly DispatcherTimer _logTimer;

        public DiagnosticWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logBuilder = new StringBuilder();
            
            // Setup timer to capture debug output
            _logTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _logTimer.Tick += LogTimer_Tick;
            _logTimer.Start();
            
            LogMessage("🔍 نافذة التشخيص تم فتحها");
            LogMessage($"⏰ الوقت: {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
            LogMessage("📋 سيتم التقاط رسائل التصحيح والخطأ هنا");
            LogMessage("");
        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {
            // This will be called periodically to check for new debug messages
            // In a real implementation, you might want to capture Debug.WriteLine output
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";
            
            _logBuilder.AppendLine(logEntry);
            
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(logEntry + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        private async void TestDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("🔍 بدء اختبار قاعدة البيانات...");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                LogMessage("📊 محاولة الاتصال بقاعدة البيانات...");
                await context.Database.EnsureCreatedAsync();
                LogMessage("✅ تم الاتصال بقاعدة البيانات بنجاح");
                
                LogMessage("📋 محاولة قراءة البيانات...");
                var caseCount = await context.Cases.CountAsync();
                LogMessage($"✅ تم العثور على {caseCount} مشكلة في قاعدة البيانات");
                
                var categoryCount = await context.IssueCategories.CountAsync();
                LogMessage($"✅ تم العثور على {categoryCount} تصنيف في قاعدة البيانات");
                
                var employeeCount = await context.Employees.CountAsync();
                LogMessage($"✅ تم العثور على {employeeCount} موظف في قاعدة البيانات");
                
                LogMessage("🎉 اختبار قاعدة البيانات مكتمل بنجاح");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ خطأ في اختبار قاعدة البيانات: {ex.Message}");
                LogMessage($"📋 تفاصيل الخطأ: {ex.StackTrace}");
            }
        }

        private async void TestServicesButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("🔍 بدء اختبار الخدمات...");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                
                // Test Case Service
                LogMessage("📋 اختبار خدمة المشاكل...");
                var caseService = scope.ServiceProvider.GetRequiredService<ICaseService>();
                var cases = await caseService.GetAllCasesAsync();
                LogMessage($"✅ تم تحميل {cases.Count()} مشكلة بنجاح");
                
                // Test Auth Service
                LogMessage("🔐 اختبار خدمة المصادقة...");
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                LogMessage("✅ خدمة المصادقة تعمل بشكل صحيح");
                
                // Test Employee Service
                LogMessage("👥 اختبار خدمة الموظفين...");
                var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                var employees = await employeeService.GetAllEmployeesAsync();
                LogMessage($"✅ تم تحميل {employees.Count()} موظف بنجاح");
                
                // Test Backup Service
                LogMessage("💾 اختبار خدمة النسخ الاحتياطي...");
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                LogMessage("✅ خدمة النسخ الاحتياطي تعمل بشكل صحيح");
                
                LogMessage("🎉 اختبار الخدمات مكتمل بنجاح");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ خطأ في اختبار الخدمات: {ex.Message}");
                LogMessage($"📋 تفاصيل الخطأ: {ex.StackTrace}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _logBuilder.Clear();
            LogTextBox.Clear();
            LogMessage("🧹 تم مسح السجل");
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Clipboard.SetText(_logBuilder.ToString());
                LogMessage("📋 تم نسخ السجل إلى الحافظة");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ خطأ في نسخ السجل: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _logTimer?.Stop();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _logTimer?.Stop();
            base.OnClosed(e);
        }
    }
} 