using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfMessageBox = System.Windows.MessageBox;

namespace CustomerIssuesManager
{
    public partial class DashboardWindow : Window
    {
        private readonly ICaseService _caseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly NotificationService? _notificationService;

        public DashboardWindow(ICaseService caseService, IServiceProvider serviceProvider)
        {
            try
            {
                InitializeComponent();
                _caseService = caseService;
                _serviceProvider = serviceProvider;
                _notificationService = new NotificationService(this);
                
                this.Loaded += DashboardWindow_Loaded;
                this.Closed += DashboardWindow_Closed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DashboardWindow constructor error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                System.Windows.MessageBox.Show($"خطأ في إنشاء النافذة: {ex.Message}", "خطأ", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw;
            }
        }

        private void DashboardWindow_Closed(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DashboardWindow closed");
        }

        private async void DashboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DashboardWindow_Loaded started");
                
                // Load statistics asynchronously
                await LoadStatistics();
                
                // Load active cases asynchronously
                await LoadActiveCases();
                
                System.Diagnostics.Debug.WriteLine("DashboardWindow_Loaded completed successfully");
            }
            catch (Exception ex)
            {
                // Log the error and show a user-friendly message
                System.Diagnostics.Debug.WriteLine($"DashboardWindow_Loaded error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Show error message box
                System.Windows.MessageBox.Show($"خطأ في تحميل البيانات:\n{ex.Message}", "خطأ", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task LoadStatistics()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadStatistics: Starting to get case statistics");
                
                // Add a small delay to ensure the UI is ready
                await Task.Delay(50);
                
                var statistics = await _caseService.GetCaseStatisticsAsync();
                System.Diagnostics.Debug.WriteLine("LoadStatistics: Got case statistics successfully");
                
                if (statistics == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadStatistics: Statistics is null");
                    _notificationService?.ShowWarning("لا يمكن تحميل الإحصائيات");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("LoadStatistics: Updating UI elements");
                
                // Use Dispatcher to ensure UI updates happen on the UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // Update basic statistics with null checks
                        if (TotalCasesLabel != null) TotalCasesLabel.Text = statistics.TotalCases.ToString();
                        if (NewCasesLabel != null) NewCasesLabel.Text = statistics.NewCases.ToString();
                        if (InProgressLabel != null) InProgressLabel.Text = statistics.InProgressCases.ToString();
                        if (SolvedLabel != null) SolvedLabel.Text = statistics.SolvedCases.ToString();
                        if (ClosedLabel != null) ClosedLabel.Text = statistics.ClosedCases.ToString();
                        if (ActiveCasesLabel != null) ActiveCasesLabel.Text = statistics.ActiveCases.ToString();

                        // Update additional statistics
                        if (CasesThisMonthLabel != null) CasesThisMonthLabel.Text = statistics.CasesThisMonth.ToString();
                        if (CasesThisYearLabel != null) CasesThisYearLabel.Text = statistics.CasesThisYear.ToString();
                        if (TotalAttachmentsLabel != null) TotalAttachmentsLabel.Text = statistics.TotalAttachments.ToString();
                        if (TotalCorrespondencesLabel != null) TotalCorrespondencesLabel.Text = statistics.TotalCorrespondences.ToString();

                        // Update detailed information
                        if (AverageResolutionTimeLabel != null)
                        {
                            if (statistics.AverageResolutionTime > 0)
                            {
                                AverageResolutionTimeLabel.Text = $"{statistics.AverageResolutionTime:F1} يوم";
                            }
                            else
                            {
                                AverageResolutionTimeLabel.Text = "غير متوفر";
                            }
                        }

                        if (MostCommonCategoryLabel != null)
                        {
                            MostCommonCategoryLabel.Text = string.IsNullOrEmpty(statistics.MostCommonCategory) 
                                ? "غير متوفر" 
                                : statistics.MostCommonCategory;
                        }

                        if (MostActiveEmployeeLabel != null)
                        {
                            MostActiveEmployeeLabel.Text = string.IsNullOrEmpty(statistics.MostActiveEmployee) 
                                ? "غير متوفر" 
                                : statistics.MostActiveEmployee;
                        }

                        if (LastCaseDateLabel != null)
                        {
                            LastCaseDateLabel.Text = statistics.LastCaseDate != DateTime.MinValue 
                                ? statistics.LastCaseDate.ToString("yyyy/MM/dd") 
                                : "غير متوفر";
                        }

                        // Update progress bars
                        UpdateProgressBars(statistics);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating UI elements: {ex.Message}");
                        _notificationService?.ShowError($"خطأ في تحديث واجهة المستخدم: {ex.Message}");
                    }
                });
                
                System.Diagnostics.Debug.WriteLine("LoadStatistics: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStatistics error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _notificationService?.ShowError($"خطأ في تحميل الإحصائيات: {ex.Message}");
            }
        }

        private async Task LoadActiveCases()
        {
            try
            {
                // Get all cases and filter for active ones (not solved or closed)
                var allCases = await _caseService.GetAllCasesAsync();
                var activeCases = allCases.Where(c => c.Status != "تم حلها" && c.Status != "مغلقة")
                                        .OrderByDescending(c => c.CreatedDate)
                                        .ToList();

                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        if (ActiveCasesDataGrid != null)
                        {
                            ActiveCasesDataGrid.ItemsSource = activeCases;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating ActiveCasesDataGrid: {ex.Message}");
                        _notificationService?.ShowError($"خطأ في تحديث قائمة المشاكل النشطة: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadActiveCases error: {ex.Message}");
                _notificationService?.ShowError($"خطأ في تحميل المشاكل النشطة: {ex.Message}");
            }
        }

        private void UpdateProgressBars(CaseStatistics statistics)
        {
            if (statistics.TotalCases > 0)
            {
                NewCasesProgressBar.Value = (double)statistics.NewCases / statistics.TotalCases * 100;
                InProgressProgressBar.Value = (double)statistics.InProgressCases / statistics.TotalCases * 100;
                SolvedProgressBar.Value = (double)statistics.SolvedCases / statistics.TotalCases * 100;
                ClosedProgressBar.Value = (double)statistics.ClosedCases / statistics.TotalCases * 100;
            }
        }

        private void OpenCasesManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                _notificationService?.ShowNotification($"خطأ في فتح إدارة المشاكل: {ex.Message}", NotificationType.Error);
            }
        }

        private void OpenAdvancedSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var advancedSearchWindow = _serviceProvider.GetRequiredService<AdvancedSearchWindow>();
                advancedSearchWindow.Owner = this;
                advancedSearchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService?.ShowNotification($"خطأ في فتح البحث المتقدم: {ex.Message}", NotificationType.Error);
            }
        }

        private void OpenEmployeeManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var employeeManagementWindow = _serviceProvider.GetRequiredService<ManageEmployeesWindow>();
                employeeManagementWindow.Show();
            }
            catch (Exception ex)
            {
                _notificationService?.ShowNotification($"خطأ في فتح إدارة الموظفين: {ex.Message}", NotificationType.Error);
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
                settingsWindow.Show();
            }
            catch (Exception ex)
            {
                _notificationService?.ShowNotification($"خطأ في فتح الإعدادات: {ex.Message}", NotificationType.Error);
            }
        }

        private async void RefreshStatistics_Click(object sender, RoutedEventArgs e)
        {
            await LoadStatistics();
            await LoadActiveCases();
            _notificationService?.ShowNotification("تم تحديث البيانات بنجاح", NotificationType.Success);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpMessage = @"دليل استخدام النظام:

            1. إدارة المشاكل: عرض وإدارة جميع مشاكل العملاء
            2. البحث المتقدم: البحث في المشاكل بمعايير متعددة
3. إدارة الموظفين: إضافة وتعديل وحذف الموظفين
4. الإعدادات: تكوين النظام والنسخ الاحتياطية

للحصول على مساعدة إضافية، يرجى التواصل مع مدير النظام.";

            WpfMessageBox.Show(helpMessage, "دليل الاستخدام", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutMessage = @"نظام إدارة مشاكل العملاء
الإصدار: 5.0.0
المطور: مصطفى اسماعيل
التاريخ: 2024

هذا النظام مصمم لإدارة مشاكل العملاء وتتبعها بكفاءة.";

            WpfMessageBox.Show(aboutMessage, "حول النظام", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}