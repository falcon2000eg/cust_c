using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CustomerIssuesManager
{
    public partial class AdvancedSearchWindow : Window
    {
        private readonly ICaseService _caseService;
        private readonly NotificationService _notificationService;
        public List<Case> SearchResults { get; private set; } = new();

        public AdvancedSearchWindow(ICaseService caseService)
        {
            InitializeComponent();
            _caseService = caseService;
            _notificationService = new NotificationService(this);
            
            Loaded += AdvancedSearchWindow_Loaded;
        }

        private async void AdvancedSearchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadComboBoxData();
        }

        private async Task LoadComboBoxData()
        {
            try
            {
                // Load years
                var years = (await _caseService.GetAvailableYearsAsync()).ToList();
                years.Insert(0, "الكل");
                YearComboBox.ItemsSource = years;
                YearComboBox.SelectedIndex = 0;

                // Load categories
                var categories = (await _caseService.GetAvailableCategoriesAsync()).ToList();
                categories.Insert(0, "الكل");
                CategoryComboBox.ItemsSource = categories;
                CategoryComboBox.SelectedIndex = 0;

                // Load statuses
                var statuses = (await _caseService.GetAvailableStatusesAsync()).ToList();
                statuses.Insert(0, "الكل");
                StatusComboBox.ItemsSource = statuses;
                StatusComboBox.SelectedIndex = 0;

                // Load employees
                var employees = (await _caseService.GetAvailableEmployeesAsync()).ToList();
                employees.Insert(0, "الكل");
                EmployeeComboBox.ItemsSource = employees;
                EmployeeComboBox.SelectedIndex = 0;

                // Set default date field
                DateFieldComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تحميل البيانات: {ex.Message}", NotificationType.Error);
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is string selectedStatus)
            {
                System.Diagnostics.Debug.WriteLine($"AdvancedSearchWindow - StatusComboBox selection changed to: {selectedStatus}");
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _notificationService.ShowLoadingIndicatorAsync("جاري البحث...");

                var criteria = new SearchCriteria
                {
                    CustomerName = CustomerNameTextBox.Text.Trim(),
                    SubscriberNumber = SubscriberNumberTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    ProblemDescription = ProblemDescriptionTextBox.Text.Trim(),
                    ActionsTaken = ActionsTakenTextBox.Text.Trim(),
                    CorrespondenceText = CorrespondenceTextBox.Text.Trim(),
                    AttachmentText = AttachmentTextBox.Text.Trim()
                };

                // Set status if selected
                if (StatusComboBox.SelectedItem is string selectedStatus && selectedStatus != "الكل")
                {
                    criteria.Status = selectedStatus;
                }

                // Set category if selected
                if (CategoryComboBox.SelectedItem is string selectedCategory && selectedCategory != "الكل")
                {
                    criteria.Category = selectedCategory;
                }

                // Set employee if selected
                if (EmployeeComboBox.SelectedItem is string selectedEmployee && selectedEmployee != "الكل")
                {
                    criteria.EmployeeName = selectedEmployee;
                }

                // Set year if selected
                if (YearComboBox.SelectedItem is string selectedYear && selectedYear != "الكل")
                {
                    if (int.TryParse(selectedYear, out int year))
                    {
                        criteria.Year = year;
                    }
                }

                // Set date field
                if (DateFieldComboBox.SelectedItem is ComboBoxItem selectedDateField)
                {
                    criteria.DateField = selectedDateField.Tag.ToString();
                }

                // Perform search
                var results = await _caseService.SearchCasesComprehensiveAsync(criteria);
                SearchResults = results.ToList();

                await _notificationService.HideLoadingIndicatorAsync();
                
                if (SearchResults.Count > 0)
                {
                    _notificationService.ShowNotification($"تم العثور على {SearchResults.Count} مشكلة", NotificationType.Success);
                    // Only set DialogResult if the window is shown as a dialog
                    try
                    {
                        DialogResult = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Window is not shown as dialog, just close it
                    }
                    Close();
                }
                else
                {
                    _notificationService.ShowNotification("لم يتم العثور على نتائج", NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                await _notificationService.HideLoadingIndicatorAsync();
                _notificationService.ShowNotification($"خطأ في البحث: {ex.Message}", NotificationType.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all text boxes
            CustomerNameTextBox.Clear();
            SubscriberNumberTextBox.Clear();
            AddressTextBox.Clear();
            ProblemDescriptionTextBox.Clear();
            ActionsTakenTextBox.Clear();
            CorrespondenceTextBox.Clear();
            AttachmentTextBox.Clear();

            // Reset combo boxes
            YearComboBox.SelectedIndex = 0;
            CategoryComboBox.SelectedIndex = 0;
            StatusComboBox.SelectedIndex = 0;
            EmployeeComboBox.SelectedIndex = 0;
            DateFieldComboBox.SelectedIndex = 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Only set DialogResult if the window is shown as a dialog
            try
            {
                DialogResult = false;
            }
            catch (InvalidOperationException)
            {
                // Window is not shown as dialog, just close it
            }
            Close();
        }
    }
} 