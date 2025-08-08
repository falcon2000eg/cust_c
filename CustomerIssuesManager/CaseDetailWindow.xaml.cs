using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfMessageBox = System.Windows.MessageBox;

namespace CustomerIssuesManager;

public partial class CaseDetailWindow : Window
{
    private readonly ICaseService _caseService;
    private Case? _currentCase; // Null for new cases, existing for editing

    public CaseDetailWindow(ICaseService caseService, Case? caseToEdit = null)
    {
        InitializeComponent();
        _caseService = caseService;
        _currentCase = caseToEdit;
        this.Loaded += CaseDetailWindow_Loaded;
        AuditLogDataGrid.MouseDoubleClick += AuditLogDataGrid_MouseDoubleClick;
    }

    private async void CaseDetailWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Load categories
            var categories = await _caseService.GetAllCategoriesAsync();
            CategoryComboBox.ItemsSource = categories;

            // Load statuses
            StatusComboBox.ItemsSource = new[] { "جديدة", "قيد التنفيذ", "تم حلها", "مغلقة" };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CaseDetailWindow: Error loading categories: {ex.Message}");
        }

        if (_currentCase != null) // Edit mode
        {
            Title = $"تعديل المشكلة رقم: {_currentCase.Id}";
            
            // Load full case details including logs
            var fullCase = await _caseService.GetCaseByIdAsync(_currentCase.Id);
            if(fullCase == null) {
                WpfMessageBox.Show("لا يمكن العثور على تفاصيل المشكلة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            _currentCase = fullCase;

            // Populate fields
            CustomerNameTextBox.Text = _currentCase.CustomerName;
            SubscriberNumberTextBox.Text = _currentCase.SubscriberNumber;
            PhoneTextBox.Text = _currentCase.Phone ?? "";
            AddressTextBox.Text = _currentCase.Address ?? "";
            ReceivedDatePicker.SelectedDate = _currentCase.ReceivedDate;
            CategoryComboBox.SelectedValue = _currentCase.CategoryId;
            StatusComboBox.SelectedItem = _currentCase.Status;
            ProblemDescriptionTextBox.Text = _currentCase.ProblemDescription ?? "";
            ActionsTakenTextBox.Text = _currentCase.ActionsTaken ?? "";
            LastMeterReadingTextBox.Text = _currentCase.LastMeterReading?.ToString();
            LastReadingDatePicker.SelectedDate = _currentCase.LastReadingDate;
            DebtAmountTextBox.Text = _currentCase.DebtAmount?.ToString();

            // Populate Audit Log
            AuditLogDataGrid.ItemsSource = _currentCase.AuditLogs.OrderByDescending(log => log.Timestamp);
        }
        else // Add mode
        {
            Title = "إضافة مشكلة جديدة";
            StatusComboBox.SelectedItem = "جديدة";
            ReceivedDatePicker.SelectedDate = DateTime.Now;
        }
    }

    private void AuditLogDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (AuditLogDataGrid.SelectedItem is AuditLog log)
        {
            string oldVals = log.OldValues ?? "(لا يوجد)";
            string newVals = log.NewValues ?? "(لا يوجد)";
            string msg = $"القيم القديمة:\n{oldVals}\n\nالقيم الجديدة:\n{newVals}";
            WpfMessageBox.Show(msg, "تفاصيل التعديل", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StatusComboBox.SelectedItem is string selectedStatus)
        {
            System.Diagnostics.Debug.WriteLine($"CaseDetailWindow - StatusComboBox selection changed to: {selectedStatus}");
            if (_currentCase != null)
            {
                _currentCase.Status = selectedStatus;
            }
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Basic Validation
        if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text) || CategoryComboBox.SelectedItem == null)
        {
            WpfMessageBox.Show("اسم العميل والتصنيف حقول إجبارية.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool isNewCase = _currentCase == null;
        if (isNewCase)
        {
            var newSelectedCategory = CategoryComboBox.SelectedItem as IssueCategory;
            if (newSelectedCategory == null)
            {
                WpfMessageBox.Show("يرجى اختيار تصنيف للمشكلة.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (App.CurrentUser?.Id == null)
            {
                WpfMessageBox.Show("خطأ في تحديد المستخدم الحالي.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _currentCase = new Case
            {
                CustomerName = CustomerNameTextBox.Text,
                CategoryId = newSelectedCategory.Id,
                Status = StatusComboBox.SelectedItem as string ?? "جديدة",
                CreatedById = App.CurrentUser.Id,
                CreatedBy = App.CurrentUser,
                Category = newSelectedCategory,
            };
        }

        // Populate the case object from the form
        _currentCase.CustomerName = CustomerNameTextBox.Text;
        _currentCase.SubscriberNumber = SubscriberNumberTextBox.Text;
        _currentCase.Phone = PhoneTextBox.Text;
        _currentCase.Address = AddressTextBox.Text;
        _currentCase.ReceivedDate = ReceivedDatePicker.SelectedDate;
        
        var selectedCategory = CategoryComboBox.SelectedItem as IssueCategory;
        if (selectedCategory != null)
        {
            _currentCase.CategoryId = selectedCategory.Id;
        }
        
        _currentCase.Status = StatusComboBox.SelectedItem as string;
        _currentCase.ProblemDescription = ProblemDescriptionTextBox.Text;
        _currentCase.ActionsTaken = ActionsTakenTextBox.Text;
        
        // Validate decimal inputs
        if (!string.IsNullOrWhiteSpace(LastMeterReadingTextBox.Text))
        {
            if (decimal.TryParse(LastMeterReadingTextBox.Text, out var lastMeter))
                _currentCase.LastMeterReading = lastMeter;
            else
            {
                WpfMessageBox.Show("قيمة قراءة العداد غير صحيحة. يرجى إدخال رقم صحيح.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else
        {
            _currentCase.LastMeterReading = null;
        }
        
        if (!string.IsNullOrWhiteSpace(DebtAmountTextBox.Text))
        {
            if (decimal.TryParse(DebtAmountTextBox.Text, out var debt))
                _currentCase.DebtAmount = debt;
            else
            {
                WpfMessageBox.Show("قيمة الدين غير صحيحة. يرجى إدخال رقم صحيح.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else
        {
            _currentCase.DebtAmount = null;
        }
        
        _currentCase.LastReadingDate = LastReadingDatePicker.SelectedDate;
        _currentCase.ModifiedDate = DateTime.Now;
        
        if (App.CurrentUser?.Id != null)
        {
            _currentCase.ModifiedById = App.CurrentUser.Id;
        }

        try
        {
            if (App.CurrentUser?.Id == null)
            {
                WpfMessageBox.Show("خطأ في تحديد المستخدم الحالي.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (isNewCase)
            {
                await _caseService.CreateCaseAsync(_currentCase, App.CurrentUser.Id);
            }
            else
            {
                await _caseService.UpdateCaseAsync(_currentCase, App.CurrentUser.Id);
            }
            
            DialogResult = true; // This will close the dialog and indicate success
            Close();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show($"حدث خطأ أثناء حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
