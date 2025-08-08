using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfMessageBox = System.Windows.MessageBox;

namespace CustomerIssuesManager
{
    public partial class ManageEmployeesWindow : Window
    {
        private readonly IEmployeeService _employeeService;
        private readonly IServiceProvider _serviceProvider;
        private readonly NotificationService _notificationService;
        private List<Employee> _allEmployees = new();

        public ManageEmployeesWindow(IEmployeeService employeeService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _employeeService = employeeService;
            _serviceProvider = serviceProvider;
            _notificationService = new NotificationService(this);
            
            this.Loaded += ManageEmployeesWindow_Loaded;
        }

        private async void ManageEmployeesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                _allEmployees = (await _employeeService.GetAllEmployeesAsync()).ToList();
                EmployeesDataGrid.ItemsSource = _allEmployees;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تحميل الموظفين: {ex.Message}", NotificationType.Error);
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear form
                NameTextBox.Text = string.Empty;
                PositionTextBox.Text = string.Empty;
                PerformanceNumberTextBox.Text = string.Empty;
                
                // Show form
                EmployeeFormPanel.Visibility = Visibility.Visible;
                AddButton.Content = "إضافة";
                AddButton.Tag = null; // New employee
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في إعداد نموذج الإضافة: {ex.Message}", NotificationType.Error);
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeesDataGrid.SelectedItem is Employee selectedEmployee)
                {
                    // Fill form with selected employee data
                    NameTextBox.Text = selectedEmployee.Name;
                    PositionTextBox.Text = selectedEmployee.Position ?? string.Empty;
                    PerformanceNumberTextBox.Text = selectedEmployee.PerformanceNumber;
                    
                    // Show form
                    EmployeeFormPanel.Visibility = Visibility.Visible;
                    AddButton.Content = "تحديث";
                    AddButton.Tag = selectedEmployee.Id; // Existing employee
                }
                else
                {
                    WpfMessageBox.Show("يرجى اختيار موظف للتعديل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في تحميل بيانات الموظف: {ex.Message}", NotificationType.Error);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    WpfMessageBox.Show("يرجى إدخال اسم الموظف", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(PerformanceNumberTextBox.Text))
                {
                    WpfMessageBox.Show("يرجى إدخال رقم الأداء", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var employee = new Employee
                {
                    Name = NameTextBox.Text.Trim(),
                    Position = PositionTextBox.Text.Trim(),
                    PerformanceNumber = PerformanceNumberTextBox.Text.Trim(),
                    IsActive = true
                };

                if (AddButton.Tag is int employeeId)
                {
                    // Update existing employee
                    employee.Id = employeeId;
                    await _employeeService.UpdateEmployeeAsync(employee);
                    _notificationService.ShowNotification("تم تحديث الموظف بنجاح", NotificationType.Success);
                }
                else
                {
                    // Add new employee
                    await _employeeService.CreateEmployeeAsync(employee);
                    _notificationService.ShowNotification("تم إضافة الموظف بنجاح", NotificationType.Success);
                }

                // Hide form and reload data
                EmployeeFormPanel.Visibility = Visibility.Collapsed;
                await LoadEmployees();
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في حفظ الموظف: {ex.Message}", NotificationType.Error);
            }
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeesDataGrid.SelectedItem is Employee selectedEmployee)
                {
                    var result = WpfMessageBox.Show(
                        $"هل أنت متأكد من حذف الموظف '{selectedEmployee.Name}'؟",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _employeeService.DeleteEmployeeAsync(selectedEmployee.Id);
                        _notificationService.ShowNotification("تم حذف الموظف بنجاح", NotificationType.Success);
                        await LoadEmployees();
                    }
                }
                else
                {
                    WpfMessageBox.Show("يرجى اختيار موظف للحذف", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في حذف الموظف: {ex.Message}", NotificationType.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeFormPanel.Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var searchTerm = SearchTextBox.Text.ToLower();
                var filteredEmployees = _allEmployees.Where(emp =>
                    emp.Name.ToLower().Contains(searchTerm) ||
                    emp.Position?.ToLower().Contains(searchTerm) == true ||
                    emp.PerformanceNumber.ToLower().Contains(searchTerm)
                ).ToList();

                EmployeesDataGrid.ItemsSource = filteredEmployees;
            }
            catch (Exception ex)
            {
                _notificationService.ShowNotification($"خطأ في البحث: {ex.Message}", NotificationType.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadEmployees();
        }
    }
}
