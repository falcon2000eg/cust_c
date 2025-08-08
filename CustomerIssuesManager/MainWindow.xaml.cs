using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using CustomerIssuesManager.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Effects;
using WpfMessageBox = System.Windows.MessageBox;
using WpfColor = System.Windows.Media.Color;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfCursors = System.Windows.Input.Cursors;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfOrientation = System.Windows.Controls.Orientation;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace CustomerIssuesManager
{
    public partial class MainWindow : Window
    {
        private readonly ICaseService _caseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly NotificationService _notificationService;
        private readonly LoadingService _loadingService;
        private readonly DispatcherTimer _timer;
        private List<Case> _allCases = new();
        private Case? _selectedCase;
        private int _selectedCaseIndex = -1;

        public MainWindow(ICaseService caseService, IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: Constructor started");
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("MainWindow: InitializeComponent completed");
                
                _caseService = caseService;
                _serviceProvider = serviceProvider;
                _notificationService = new NotificationService(this);
                _loadingService = new LoadingService(this);
                System.Diagnostics.Debug.WriteLine("MainWindow: Services initialized");

                // Setup timer for clock
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
                System.Diagnostics.Debug.WriteLine("MainWindow: Timer setup completed");

                // Setup keyboard shortcuts
                SetupKeyboardShortcuts();
                System.Diagnostics.Debug.WriteLine("MainWindow: Keyboard shortcuts setup completed");

                // Setup form validation
                SetupFormValidation();
                System.Diagnostics.Debug.WriteLine("MainWindow: Form validation setup completed");

                // Load initial data
                System.Diagnostics.Debug.WriteLine("MainWindow: Starting initial data load");
                var _ = LoadInitialDataAsync();
                System.Diagnostics.Debug.WriteLine("MainWindow: Constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: Constructor error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"MainWindow: Constructor stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void SetupKeyboardShortcuts()
        {
            // Add keyboard shortcuts
            this.KeyDown += MainWindow_KeyDown;
            
            // Focus search on Ctrl+F
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    QuickSearchBox.Focus();
                    e.Handled = true;
                }
            };
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.N when Keyboard.Modifiers == ModifierKeys.Control:
                    AddButton_Click(sender, e);
                    e.Handled = true;
                    break;
                    
                case Key.S when Keyboard.Modifiers == ModifierKeys.Control:
                    SaveChanges();
                    e.Handled = true;
                    break;
                    
                case Key.Delete:
                    DeleteButton_Click(sender, e);
                    e.Handled = true;
                    break;
                    
                case Key.F5:
                    RefreshData();
                    e.Handled = true;
                    break;
                    
                case Key.Up when Keyboard.Modifiers == ModifierKeys.Control:
                    NavigateToPreviousCase();
                    e.Handled = true;
                    break;
                    
                case Key.Down when Keyboard.Modifiers == ModifierKeys.Control:
                    NavigateToNextCase();
                    e.Handled = true;
                    break;
                    
                case Key.Escape:
                    ClearSelection();
                    e.Handled = true;
                    break;
            }
        }

        private void NavigateToPreviousCase()
        {
            if (_selectedCaseIndex > 0)
            {
                _selectedCaseIndex--;
                SelectCaseByIndex(_selectedCaseIndex);
            }
        }

        private void NavigateToNextCase()
        {
            if (_selectedCaseIndex < _allCases.Count - 1)
            {
                _selectedCaseIndex++;
                SelectCaseByIndex(_selectedCaseIndex);
            }
        }

        private void SelectCaseByIndex(int index)
        {
            if (index >= 0 && index < _allCases.Count)
            {
                var caseToSelect = _allCases[index];
                SelectCase(caseToSelect.Id);
            }
        }

        private void ClearSelection()
        {
            _selectedCase = null;
            _selectedCaseIndex = -1;
            ClearCaseDisplay();
            _notificationService.ShowInfo("تم مسح التحديد");
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeLabel.Text = $"🕐 {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                await _loadingService.ExecuteWithLoadingAsync(async () =>
                {
                    await LoadCategoriesAsync();
                    await LoadStatusOptionsAsync();
                    await LoadYearsAsync();
                    await LoadSortOptionsAsync();
                    await LoadSearchTypeOptionsAsync();
                    await LoadEmployeesAsync();
                    await LoadAllCasesAsync();
                    
                    // إعداد placeholder text للبحث بالرقم السنوي
                    YearlyNumberSearchBox.Text = "مثال: 2024-0001";
                    YearlyNumberSearchBox.Foreground = System.Windows.Media.Brushes.Gray;
                    
                    YearlyNumberSearchBox.GotFocus += (s, e) =>
                    {
                        if (YearlyNumberSearchBox.Text == "مثال: 2024-0001")
                        {
                            YearlyNumberSearchBox.Text = "";
                            YearlyNumberSearchBox.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    };
                    
                    YearlyNumberSearchBox.LostFocus += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(YearlyNumberSearchBox.Text))
                        {
                            YearlyNumberSearchBox.Text = "مثال: 2024-0001";
                            YearlyNumberSearchBox.Foreground = System.Windows.Media.Brushes.Gray;
                        }
                    };
                }, "جاري تحميل البيانات...");
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowError("خطأ في تحميل البيانات الأولية", ex.Message);
                });
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadCategoriesAsync: Starting to load categories...");
                var categories = await _caseService.GetAllCategoriesAsync();
                System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync: Loaded {categories?.Count() ?? 0} categories");
                
                // Add "الكل" option for filter
                var filterCategories = new List<string> { "الكل" };
                var categoryNames = categories?.Select(c => c.CategoryName).ToList() ?? new List<string>();
                filterCategories.AddRange(categoryNames);
                
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // Load full category objects for the main ComboBox
                        CategoryComboBox.ItemsSource = categories?.ToList() ?? new List<IssueCategory>();
                        CategoryComboBox.DisplayMemberPath = "CategoryName";
                        CategoryComboBox.SelectedValuePath = "Id";
                        
                        // Also populate the filter combobox with "الكل" option
                        CategoryFilterComboBox.ItemsSource = filterCategories;
                        CategoryFilterComboBox.SelectedIndex = 0; // Select "الكل" by default
                        System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync: Categories loaded to UI successfully. CategoryComboBox count: {CategoryComboBox.Items.Count}, CategoryFilterComboBox count: {CategoryFilterComboBox.Items.Count}");
                        
                        // Categories loaded successfully
                        System.Diagnostics.Debug.WriteLine($"Categories loaded successfully: {categories?.Count() ?? 0} categories");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync: Error updating UI: {ex.Message}");
                        _notificationService.ShowError("خطأ في تحميل التصنيفات", ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync: Error loading categories: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync: Stack trace: {ex.StackTrace}");
                
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل التصنيفات", ex);
                });
            }
        }

        private async Task LoadStatusOptionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadStatusOptionsAsync: Starting to load status options...");
                var statusOptions = await _caseService.GetAvailableStatusesAsync();
                System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Loaded {statusOptions?.Count() ?? 0} status options");
                
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // Remove existing event handler to avoid duplicates
                        StatusComboBox.SelectionChanged -= StatusComboBox_SelectionChanged;
                        
                        // Ensure we have a valid list of statuses
                        var statusList = statusOptions?.ToList() ?? new List<string>();
                        if (!statusList.Any())
                        {
                            statusList = new List<string> { "جديدة", "قيد التنفيذ", "تم حلها", "مغلقة" };
                            System.Diagnostics.Debug.WriteLine("LoadStatusOptionsAsync: Using fallback status list");
                        }
                        
                        // Ensure we have the basic statuses
                        var basicStatuses = new[] { "جديدة", "قيد التنفيذ", "تم حلها", "مغلقة" };
                        foreach (var status in basicStatuses)
                        {
                            if (!statusList.Contains(status))
                            {
                                statusList.Add(status);
                            }
                        }
                        
                        StatusComboBox.ItemsSource = statusList;
                        
                        // Add event handler for status changes
                        StatusComboBox.SelectionChanged += StatusComboBox_SelectionChanged;
                        
                        // Also populate the filter combobox with "الكل" option
                        var filterStatuses = new List<string> { "الكل" };
                        filterStatuses.AddRange(statusList);
                        StatusFilterComboBox.ItemsSource = filterStatuses;
                        StatusFilterComboBox.SelectedIndex = 0; // Select "الكل" by default
                        
                        System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Loaded {statusList.Count} status options: {string.Join(", ", statusList)}");
                        
                        // Statuses loaded successfully
                        System.Diagnostics.Debug.WriteLine($"Statuses loaded successfully: {statusList.Count} statuses");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Error updating UI: {ex.Message}");
                        _notificationService.ShowError("خطأ في تحميل خيارات حالة المشكلة", ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Error loading status options: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Stack trace: {ex.StackTrace}");
                
                // Fallback to static status list if service fails
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // Remove existing event handler to avoid duplicates
                        StatusComboBox.SelectionChanged -= StatusComboBox_SelectionChanged;
                        
                        var fallbackStatuses = new[] { "جديدة", "قيد التنفيذ", "تم حلها", "مغلقة" };
                        StatusComboBox.ItemsSource = fallbackStatuses;
                        StatusComboBox.SelectionChanged += StatusComboBox_SelectionChanged;
                        
                        System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Using fallback status list: {string.Join(", ", fallbackStatuses)}");
                        _notificationService.ShowWarning("تم استخدام قائمة المشاكل الافتراضية", ex.Message, "سيتم استخدام القائمة الافتراضية حتى يتم حل مشكلة الاتصال بقاعدة البيانات");
                    }
                    catch (Exception uiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadStatusOptionsAsync: Error setting fallback statuses: {uiEx.Message}");
                        _notificationService.ShowError("خطأ في تحميل خيارات حالة المشكلة", uiEx.Message);
                    }
                });
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is string selectedStatus)
            {
                System.Diagnostics.Debug.WriteLine($"StatusComboBox selection changed to: {selectedStatus}");
                if (_selectedCase != null)
                {
                    var oldStatus = _selectedCase.Status;
                    _selectedCase.Status = selectedStatus;
                    
                    System.Diagnostics.Debug.WriteLine($"Updated case status from '{oldStatus}' to '{selectedStatus}'");
                    
                    // Show a brief notification that the status has been updated
                    _notificationService.ShowInfo($"تم تحديث حالة المشكلة من '{oldStatus}' إلى '{selectedStatus}'");
                    
                    // Update the case card in the list to reflect the status change
                    UpdateCasesList();
                    
                    // Mark the case as modified so it will be saved when the user clicks Save
                    _selectedCase.ModifiedDate = DateTime.Now;
                    
                    // Force a UI update to show the change immediately
                    Dispatcher.Invoke(() =>
                    {
                        // This will trigger a UI refresh
                        StatusComboBox.GetBindingExpression(System.Windows.Controls.ComboBox.SelectedItemProperty)?.UpdateTarget();
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("_selectedCase is null, cannot update status");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"StatusComboBox.SelectedItem is not a string: {StatusComboBox.SelectedItem}");
            }
        }

        private async Task LoadYearsAsync()
        {
            try
            {
                var years = await _caseService.GetAvailableYearsAsync();
                await Dispatcher.InvokeAsync(() =>
                {
                    YearComboBox.Items.Clear();
                    YearComboBox.Items.Add("الكل");
                    foreach (var year in years)
                    {
                        YearComboBox.Items.Add(year.ToString());
                    }
                    YearComboBox.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل السنوات", ex);
                });
            }
        }

        private async Task LoadSortOptionsAsync()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    SortComboBox.Items.Clear();
                    SortComboBox.Items.Add("تاريخ الإنشاء (الأحدث)");
                    SortComboBox.Items.Add("تاريخ الإنشاء (الأقدم)");
                    SortComboBox.Items.Add("اسم العميل");
                    SortComboBox.Items.Add("رقم المشترك");
                    SortComboBox.Items.Add("حالة المشكلة");
                    SortComboBox.Items.Add("التصنيف");
                    SortComboBox.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل خيارات الترتيب", ex);
                });
            }
        }

        private async Task LoadSearchTypeOptionsAsync()
        {
            try
            {
                var searchTypes = new List<string>
                {
                    "الكل",
                    "اسم العميل",
                    "رقم المشترك",
                    "التصنيف",
                    "حالة المشكلة"
                };

                await Dispatcher.InvokeAsync(() =>
                {
                    SearchTypeComboBox.ItemsSource = searchTypes;
                    SearchTypeComboBox.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowError("خطأ في تحميل أنواع البحث", ex.Message);
                });
            }
        }

        private async Task LoadAllCasesAsync()
        {
            try
            {
                var cases = await _caseService.GetAllCasesAsync();
                _allCases = cases.ToList();
                
                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateCasesList();
                    _notificationService.ShowSuccess("تم تحميل البيانات بنجاح", $"تم تحميل {_allCases.Count} مشكلة بنجاح");
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل الحالات", ex);
                });
            }
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                var employeeService = _serviceProvider.GetRequiredService<IEmployeeService>();
                var employees = await employeeService.GetAllEmployeesAsync();
                await Dispatcher.InvokeAsync(() =>
                {
                    CreatedByComboBox.ItemsSource = employees;
                    ModifiedByComboBox.ItemsSource = employees;
                    SolvedByComboBox.ItemsSource = employees;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل بيانات الموظفين", ex);
                });
            }
        }

        private void UpdateCasesList()
        {
            var currentSelection = CasesListBox.SelectedItem;
            CasesListBox.ItemsSource = null;
            CasesListBox.ItemsSource = _allCases;
            
            // Restore selection if it was a Case object
            if (currentSelection is Case selectedCase)
            {
                var caseToSelect = _allCases.FirstOrDefault(c => c.Id == selectedCase.Id);
                if (caseToSelect != null)
                {
                    CasesListBox.SelectedItem = caseToSelect;
                }
            }
        }



        private SolidColorBrush GetStatusColor(string status)
        {
            return status switch
            {
                "جديدة" => new SolidColorBrush(WpfColor.FromRgb(59, 130, 246)), // Blue
                "قيد التنفيذ" => new SolidColorBrush(WpfColor.FromRgb(245, 158, 11)), // Orange
                "تم حلها" => new SolidColorBrush(WpfColor.FromRgb(16, 185, 129)), // Green
                "مغلقة" => new SolidColorBrush(WpfColor.FromRgb(107, 114, 128)), // Gray
                _ => new SolidColorBrush(WpfColor.FromRgb(107, 114, 128))
            };
        }

        private async void SelectCase(int caseId)
        {
            _selectedCase = _allCases.FirstOrDefault(c => c.Id == caseId);
            if (_selectedCase != null)
            {
                _selectedCaseIndex = _allCases.IndexOf(_selectedCase);
                
                System.Diagnostics.Debug.WriteLine($"Selecting case {caseId} with status: {_selectedCase.Status}");
                
                // Ensure categories and statuses are loaded first
                if (CategoryComboBox.ItemsSource == null)
                {
                    await LoadCategoriesAsync();
                }
                if (StatusComboBox.ItemsSource == null)
                {
                    await LoadStatusOptionsAsync();
                }
                if (CreatedByComboBox.ItemsSource == null)
                {
                    await LoadEmployeesAsync();
                }
                
                // Load case details after ensuring ItemsSource is loaded
                LoadCaseDetails(_selectedCase);
                
                // Verify that the status was set correctly
                await Dispatcher.InvokeAsync(() =>
                {
                    if (StatusComboBox.SelectedItem?.ToString() != _selectedCase.Status)
                    {
                        System.Diagnostics.Debug.WriteLine($"Status verification failed. Expected: {_selectedCase.Status}, Actual: {StatusComboBox.SelectedItem}");
                        // Try to set it again
                        StatusComboBox.SelectedItem = _selectedCase.Status;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Status was set correctly");
                    }
                });
                
                // Temporarily remove the event handler to avoid infinite loop
                CasesListBox.SelectionChanged -= CasesListBox_SelectionChanged;
                CasesListBox.SelectedItem = _selectedCase;
                CasesListBox.SelectionChanged += CasesListBox_SelectionChanged;
            }
        }

        private void LoadCaseDetails(Case caseItem)
        {
            System.Diagnostics.Debug.WriteLine($"Loading case details for case {caseItem.Id} with status: {caseItem.Status}");
            
            // Clear any existing validation errors
            ClearAllValidationErrors();
            
            // Load case details into the display area
            CustomerNameTextBox.Text = caseItem.CustomerName;
            SubscriberNumberTextBox.Text = caseItem.SubscriberNumber;
            PhoneTextBox.Text = caseItem.Phone ?? "";
            AddressTextBox.Text = caseItem.Address ?? "";
            ProblemDescriptionTextBox.Text = caseItem.ProblemDescription ?? "";
            ActionsTakenTextBox.Text = caseItem.ActionsTaken ?? "";
            LastMeterReadingTextBox.Text = caseItem.LastMeterReading?.ToString() ?? "";
            DebtAmountTextBox.Text = caseItem.DebtAmount?.ToString() ?? "";
            
            if (caseItem.LastReadingDate.HasValue)
                LastReadingDatePicker.SelectedDate = caseItem.LastReadingDate.Value;
            
            if (caseItem.ReceivedDate.HasValue)
                ReceivedDatePicker.SelectedDate = caseItem.ReceivedDate.Value;
            
            // Set category and status - with null checks
            if (CategoryComboBox.ItemsSource != null)
            {
                CategoryComboBox.SelectedValue = caseItem.CategoryId;
            }
            
            if (StatusComboBox.ItemsSource != null)
            {
                // Temporarily remove event handler to avoid triggering during load
                StatusComboBox.SelectionChanged -= StatusComboBox_SelectionChanged;
                
                // Ensure the status exists in the ItemsSource before setting it
                var statusItems = StatusComboBox.ItemsSource as IEnumerable<string>;
                if (statusItems != null)
                {
                    var statusList = statusItems.ToList();
                    
                    // If the status doesn't exist in the list, add it temporarily
                    if (!statusList.Contains(caseItem.Status))
                    {
                        statusList.Add(caseItem.Status);
                        StatusComboBox.ItemsSource = statusList;
                        System.Diagnostics.Debug.WriteLine($"Added missing status '{caseItem.Status}' to the list");
                    }
                    
                    // Set the selected item
                    StatusComboBox.SelectedItem = caseItem.Status;
                    System.Diagnostics.Debug.WriteLine($"Set StatusComboBox.SelectedItem to: {caseItem.Status}");
                    
                    // Verify the selection was set correctly
                    if (StatusComboBox.SelectedItem?.ToString() == caseItem.Status)
                    {
                        System.Diagnostics.Debug.WriteLine("StatusComboBox.SelectedItem was set successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"StatusComboBox.SelectedItem verification failed. Expected: {caseItem.Status}, Actual: {StatusComboBox.SelectedItem}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("StatusComboBox.ItemsSource is null or not IEnumerable<string>");
                }
                
                // Re-add event handler
                StatusComboBox.SelectionChanged += StatusComboBox_SelectionChanged;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("StatusComboBox.ItemsSource is null");
            }
            
            // Set employee data - with null checks
            if (CreatedByComboBox.ItemsSource != null)
            {
                CreatedByComboBox.SelectedValue = caseItem.CreatedById;
            }
            
            if (ModifiedByComboBox.ItemsSource != null)
            {
                ModifiedByComboBox.SelectedValue = caseItem.ModifiedById;
            }
            
            if (SolvedByComboBox.ItemsSource != null)
            {
                SolvedByComboBox.SelectedValue = caseItem.SolvedById;
            }
            
            // Update header labels
            CustomerNameLabel.Text = caseItem.CustomerName;
            SolvedByLabel.Text = caseItem.SolvedBy?.Name ?? "";
            
            // Load additional details
            LoadCaseAttachments(caseItem.Id);
            LoadCaseCorrespondences(caseItem.Id);
            LoadCaseAuditLog(caseItem.Id);
        }

        /// <summary>
        /// Clears all validation errors from form fields
        /// </summary>
        private void ClearAllValidationErrors()
        {
            ValidationHelper.ClearValidationError(CustomerNameTextBox);
            ValidationHelper.ClearValidationError(SubscriberNumberTextBox);
            ValidationHelper.ClearValidationError(PhoneTextBox);
            ValidationHelper.ClearValidationError(AddressTextBox);
            ValidationHelper.ClearValidationError(ProblemDescriptionTextBox);
            ValidationHelper.ClearValidationError(LastMeterReadingTextBox);
            ValidationHelper.ClearValidationError(DebtAmountTextBox);
            ValidationHelper.ClearValidationError(CategoryComboBox);
            ValidationHelper.ClearValidationError(StatusComboBox);
            ValidationHelper.ClearValidationError(ReceivedDatePicker);
            ValidationHelper.ClearValidationError(CreatedByComboBox);
        }

        private async void LoadCaseAttachments(int caseId)
        {
            try
            {
                var caseData = await _caseService.GetCaseByIdAsync(caseId);
                await Dispatcher.InvokeAsync(() =>
                {
                    AttachmentsDataGrid.ItemsSource = null; // إعادة تعيين المصدر
                    AttachmentsDataGrid.ItemsSource = caseData?.Attachments ?? new List<Attachment>();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل المرفقات", ex);
                });
            }
        }

        private async void LoadCaseCorrespondences(int caseId)
        {
            try
            {
                var caseData = await _caseService.GetCaseByIdAsync(caseId);
                await Dispatcher.InvokeAsync(() =>
                {
                    CorrespondencesDataGrid.ItemsSource = null; // إعادة تعيين المصدر
                    
                    if (caseData?.Correspondences != null)
                    {
                        // ترتيب المراسلات حسب الترقيم السنوي
                        var orderedCorrespondences = caseData.Correspondences
                            .OrderBy(c => c.YearlySequenceNumber)
                            .ToList();
                        
                        CorrespondencesDataGrid.ItemsSource = orderedCorrespondences;
                    }
                    else
                    {
                        CorrespondencesDataGrid.ItemsSource = new List<Correspondence>();
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل المراسلات", ex);
                });
            }
        }

        private async void LoadCaseAuditLog(int caseId)
        {
            try
            {
                var caseData = await _caseService.GetCaseByIdAsync(caseId);
                await Dispatcher.InvokeAsync(() =>
                {
                    AuditLogDataGrid.ItemsSource = caseData?.AuditLogs ?? new List<AuditLog>();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.ShowDatabaseError("تحميل سجل التدقيق", ex);
                });
            }
        }

        /// <summary>
        /// تحديث جميع بيانات الحالة المحددة
        /// </summary>
        private async void RefreshCaseData()
        {
            if (_selectedCase != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    // إعادة تحميل بيانات الحالة
                    var updatedCase = await _caseService.GetCaseByIdAsync(_selectedCase.Id);
                    if (updatedCase != null)
                    {
                        _selectedCase = updatedCase;
                        LoadCaseDetails(updatedCase);
                    }
                });
            }
        }

        /// <summary>
        /// تحديث عرض الترقيم السنوي في جدول المراسلات
        /// </summary>
        private void UpdateCorrespondencesDisplay()
        {
            if (CorrespondencesDataGrid.ItemsSource is IEnumerable<Correspondence> correspondences)
            {
                // إعادة ترتيب المراسلات حسب الترقيم السنوي
                var orderedCorrespondences = correspondences
                    .OrderBy(c => c.YearlySequenceNumber)
                    .ToList();
                
                CorrespondencesDataGrid.ItemsSource = null;
                CorrespondencesDataGrid.ItemsSource = orderedCorrespondences;
            }
        }

        private void HighlightSelectedCase()
        {
            // Set the selected item in the ListBox
            if (_selectedCase != null)
            {
                CasesListBox.SelectedItem = _selectedCase;
            }
        }

        private void ClearCaseDisplay()
        {
            CustomerNameTextBox.Text = "";
            SubscriberNumberTextBox.Text = "";
            PhoneTextBox.Text = "";
            AddressTextBox.Text = "";
            ProblemDescriptionTextBox.Text = "";
            ActionsTakenTextBox.Text = "";
            LastMeterReadingTextBox.Text = "";
            DebtAmountTextBox.Text = "";
            
            LastReadingDatePicker.SelectedDate = null;
            ReceivedDatePicker.SelectedDate = null;
            CategoryComboBox.SelectedItem = null;
            
            // Temporarily remove event handler to avoid triggering during clear
            StatusComboBox.SelectionChanged -= StatusComboBox_SelectionChanged;
            StatusComboBox.SelectedItem = null;
            StatusComboBox.SelectionChanged += StatusComboBox_SelectionChanged;
            
            CreatedByComboBox.SelectedItem = null;
            ModifiedByComboBox.SelectedItem = null;
            SolvedByComboBox.SelectedItem = null;
            
                            CustomerNameLabel.Text = "اختر مشكلة من القائمة";
            SolvedByLabel.Text = "";
            
            AttachmentsDataGrid.ItemsSource = null;
            CorrespondencesDataGrid.ItemsSource = null;
            AuditLogDataGrid.ItemsSource = null;
        }

        private async void RefreshData()
        {
            await LoadInitialDataAsync();
        }

        private async void SaveChanges()
        {
            if (_selectedCase == null)
            {
                _notificationService.ShowWarning("لا توجد مشكلة محددة للحفظ");
                return;
            }

            // Validate required fields before saving
            if (!ValidateForm())
            {
                return;
            }

            await _loadingService.ExecuteWithLoadingAsync(async () =>
            {
                try
                {
                    // Update case properties from UI
                    _selectedCase.CustomerName = CustomerNameTextBox.Text;
                    _selectedCase.SubscriberNumber = SubscriberNumberTextBox.Text;
                    _selectedCase.Phone = PhoneTextBox.Text;
                    _selectedCase.Address = AddressTextBox.Text;
                    _selectedCase.ProblemDescription = ProblemDescriptionTextBox.Text;
                    _selectedCase.ActionsTaken = ActionsTakenTextBox.Text;
                    
                    if (decimal.TryParse(LastMeterReadingTextBox.Text, out var lastMeter))
                        _selectedCase.LastMeterReading = lastMeter;
                    
                    if (decimal.TryParse(DebtAmountTextBox.Text, out var debt))
                        _selectedCase.DebtAmount = debt;
                    
                    _selectedCase.LastReadingDate = LastReadingDatePicker.SelectedDate;
                    _selectedCase.ReceivedDate = ReceivedDatePicker.SelectedDate;
                    
                    if (CategoryComboBox.SelectedItem is IssueCategory category)
                        _selectedCase.CategoryId = category.Id;
                    
                    if (StatusComboBox.SelectedItem is string status)
                    {
                        var oldStatus = _selectedCase.Status;
                        _selectedCase.Status = status;
                        // Log the status change for debugging
                        System.Diagnostics.Debug.WriteLine($"Status changed from '{oldStatus}' to '{status}'");
                        
                        // Show a notification about the status change
                        if (oldStatus != status)
                        {
                            _notificationService.ShowInfo($"تم تغيير حالة المشكلة من '{oldStatus}' إلى '{status}'");
                        }
                    }
                    else
                    {
                        // Use a default status if none is selected
                        _selectedCase.Status = "جديدة";
                        System.Diagnostics.Debug.WriteLine("No status selected, using default status: جديدة");
                    }

                    // Save to database
                    await _caseService.UpdateCaseAsync(_selectedCase, App.CurrentUser?.Id ?? 1);
                    
                    // Refresh the list
                    await LoadInitialDataAsync();
                    
                    _notificationService.ShowSuccess("تم حفظ التغييرات بنجاح", "تم حفظ جميع التغييرات في قاعدة البيانات");
                }
                catch (Exception ex)
                {
                    _notificationService.ShowDatabaseError("حفظ التغييرات", ex);
                }
            }, "جاري حفظ التغييرات...");
        }

        /// <summary>
        /// Validates all form fields before saving
        /// </summary>
        /// <returns>True if all validations pass, false otherwise</returns>
        private bool ValidateForm()
        {
            var errors = new List<string>();
            bool isValid = true;

            // Validate required fields
            if (!ValidationHelper.ValidateRequiredField(CustomerNameTextBox, "اسم العميل"))
            {
                errors.Add("• اسم العميل مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.ValidateSubscriberNumber(SubscriberNumberTextBox))
            {
                errors.Add("• رقم المشترك مطلوب أو غير صحيح");
                isValid = false;
            }

            // Optional fields - only validate format if they have content
            if (!string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                // Phone number is optional, no validation needed
            }

            if (!string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                // Address is optional, no validation needed
            }

            if (!string.IsNullOrWhiteSpace(ProblemDescriptionTextBox.Text))
            {
                // Problem description is optional, no validation needed
            }

            if (!ValidationHelper.ValidateRequiredComboBox(CategoryComboBox, "تصنيف المشكلة"))
            {
                errors.Add("• تصنيف المشكلة مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.ValidateRequiredComboBox(StatusComboBox, "حالة المشكلة"))
            {
                errors.Add("• حالة المشكلة مطلوبة");
                isValid = false;
            }

            // Optional numeric fields - only validate format if they have content
            if (!string.IsNullOrWhiteSpace(LastMeterReadingTextBox.Text))
            {
                if (!ValidationHelper.ValidateMeterReading(LastMeterReadingTextBox))
                {
                    errors.Add("• آخر قراءة عداد غير صحيحة");
                    isValid = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(DebtAmountTextBox.Text))
            {
                if (!ValidationHelper.ValidateDebtAmount(DebtAmountTextBox))
                {
                    errors.Add("• المديونية غير صحيحة");
                    isValid = false;
                }
            }

            if (!ValidationHelper.ValidateRequiredDatePicker(ReceivedDatePicker, "تاريخ الاستلام"))
            {
                errors.Add("• تاريخ الاستلام مطلوب");
                isValid = false;
            }

            if (!ValidationHelper.ValidateRequiredComboBox(CreatedByComboBox, "أنشأ بواسطة"))
            {
                errors.Add("• أنشأ بواسطة مطلوب");
                isValid = false;
            }

            // Show validation summary if there are errors
            if (!isValid)
            {
                ValidationHelper.ShowValidationSummary(errors);
            }

            return isValid;
        }

        /// <summary>
        /// Sets up real-time validation for form fields
        /// </summary>
        private void SetupFormValidation()
        {
            // Add TextChanged event handlers for real-time validation
            CustomerNameTextBox.TextChanged += (s, e) => ValidationHelper.ValidateRequiredField(CustomerNameTextBox, "اسم العميل");
            SubscriberNumberTextBox.TextChanged += (s, e) => ValidationHelper.ValidateSubscriberNumber(SubscriberNumberTextBox);
            
            // Optional fields - only validate format if they have content
            PhoneTextBox.TextChanged += (s, e) => ValidationHelper.ClearValidationError(PhoneTextBox);
            
            AddressTextBox.TextChanged += (s, e) => ValidationHelper.ClearValidationError(AddressTextBox);
            ProblemDescriptionTextBox.TextChanged += (s, e) => ValidationHelper.ClearValidationError(ProblemDescriptionTextBox);
            
            LastMeterReadingTextBox.TextChanged += (s, e) => 
            {
                if (!string.IsNullOrWhiteSpace(LastMeterReadingTextBox.Text))
                {
                    ValidationHelper.ValidateMeterReading(LastMeterReadingTextBox);
                }
                else
                {
                    ValidationHelper.ClearValidationError(LastMeterReadingTextBox);
                }
            };
            
            DebtAmountTextBox.TextChanged += (s, e) => 
            {
                if (!string.IsNullOrWhiteSpace(DebtAmountTextBox.Text))
                {
                    ValidationHelper.ValidateDebtAmount(DebtAmountTextBox);
                }
                else
                {
                    ValidationHelper.ClearValidationError(DebtAmountTextBox);
                }
            };

            // Add SelectionChanged event handlers for ComboBoxes
            CategoryComboBox.SelectionChanged += (s, e) => ValidationHelper.ValidateRequiredComboBox(CategoryComboBox, "تصنيف المشكلة");
            StatusComboBox.SelectionChanged += (s, e) => ValidationHelper.ValidateRequiredComboBox(StatusComboBox, "حالة المشكلة");
            CreatedByComboBox.SelectionChanged += (s, e) => ValidationHelper.ValidateRequiredComboBox(CreatedByComboBox, "أنشأ بواسطة");

            // Add SelectedDateChanged event handlers for DatePickers
            ReceivedDatePicker.SelectedDateChanged += (s, e) => ValidationHelper.ValidateRequiredDatePicker(ReceivedDatePicker, "تاريخ الاستلام");
        }

        // Event handlers
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var caseDetailWindow = _serviceProvider.GetRequiredService<CaseDetailWindow>();
                caseDetailWindow.Owner = this;
                if (caseDetailWindow.ShowDialog() == true)
                {
                    // Refresh the list after adding
                    await LoadInitialDataAsync();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowDatabaseError("إضافة مشكلة جديدة", ex);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCase == null)
            {
                _notificationService.ShowWarning("يرجى تحديد مشكلة للحذف");
                return;
            }

            var result = WpfMessageBox.Show(
                $"هل أنت متأكد من حذف المشكلة '{_selectedCase.CustomerName}'؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _loadingService.ExecuteWithLoadingAsync(async () =>
                {
                    try
                    {
                        await _caseService.DeleteCaseAsync(_selectedCase.Id, App.CurrentUser?.Id ?? 1);
                        _selectedCase = null;
                        _selectedCaseIndex = -1;
                        ClearCaseDisplay();
                        await LoadInitialDataAsync();
                        _notificationService.ShowSuccess("تم حذف المشكلة بنجاح");
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowDatabaseError("حذف المشكلة", ex);
                    }
                }, "جاري حذف المشكلة...");
            }
        }

        private void ManageEmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageEmployeesWindow = _serviceProvider.GetRequiredService<ManageEmployeesWindow>();
                manageEmployeesWindow.Owner = this;
                manageEmployeesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("فشل في فتح نافذة إدارة الموظفين", ex.Message, "تأكد من أن النافذة متاحة وحاول مرة أخرى");
            }
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            var dashboardWindow = _serviceProvider.GetRequiredService<DashboardWindow>();
            dashboardWindow.Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = WpfMessageBox.Show(
                "هل أنت متأكد من أنك تريد الخروج من النظام؟\n\nسيتم حفظ جميع التغييرات تلقائياً.",
                "تأكيد الخروج",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // Save any pending changes before exiting
                try
                {
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    _notificationService.ShowWarning("تحذير: لم يتم حفظ بعض التغييرات", ex.Message);
                }
                
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void HelpGuide_Click(object sender, RoutedEventArgs e)
        {
            var helpText = @"
📖 دليل استخدام النظام

🎯 الوظائف الأساسية:
            • إضافة مشكلة جديدة: Ctrl+N
• حفظ التغييرات: Ctrl+S
            • حذف المشكلة: Delete
• تحديث البيانات: F5
• تركيز على البحث: Ctrl+F
            • التنقل بين المشاكل: Ctrl+↑/↓
• مسح التحديد: Esc

🔍 البحث والفلترة:
• استخدم حقل البحث للبحث الشامل
• اختر نوع البحث المحدد
• استخدم فلتر السنة للتصفية

💡 نصائح:
            • انقر على أي مشكلة لعرض تفاصيلها
• استخدم لوحة المفاتيح للتنقل السريع
• تحقق من الإشعارات للحصول على التحديثات
";
            
            WpfMessageBox.Show(helpText, "دليل الاستخدام", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Diagnostic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var diagnosticWindow = _serviceProvider.GetRequiredService<DiagnosticWindow>();
                diagnosticWindow.Owner = this;
                diagnosticWindow.Show();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("فشل في فتح نافذة التشخيص", ex.Message, "تأكد من أن النافذة متاحة وحاول مرة أخرى");
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutText = @"
نظام إدارة مشاكل العملاء - النسخة المحسنة

الإصدار: 2.0.0
تاريخ الإصدار: 2024

المميزات:
• واجهة مستخدم محسنة
• إشعارات متقدمة
• دعم لوحة المفاتيح
• تحميل سريع
• تصميم عصري

تم التطوير باستخدام:
• .NET 9.0
• WPF
• Entity Framework Core
• SQLite
";
            
            WpfMessageBox.Show(aboutText, "حول النظام", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void QuickSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Implement real-time quick search
            PerformQuickSearch();
        }

        private void QuickSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger search manually
            PerformQuickSearch();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PerformAdvancedSearch();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PerformAdvancedSearch();
        }

        private void PerformQuickSearch()
        {
            var searchText = QuickSearchBox.Text?.Trim() ?? "";
            var yearFilter = YearComboBox.SelectedItem?.ToString();
            var searchType = SearchTypeComboBox.SelectedItem?.ToString();
            
            PerformSearchWithFilters(searchText, yearFilter, searchType);
        }

        private void PerformAdvancedSearch()
        {
            var searchText = QuickSearchBox.Text?.Trim() ?? "";
            var yearFilter = YearComboBox.SelectedItem?.ToString();
            var searchType = SearchTypeComboBox.SelectedItem?.ToString();
            var statusFilter = StatusFilterComboBox.SelectedItem?.ToString();
            var categoryFilter = CategoryFilterComboBox.SelectedItem?.ToString();
            
            PerformSearchWithAdvancedFilters(searchText, yearFilter, searchType, statusFilter, categoryFilter);
        }

        private void PerformSearchWithFilters(string searchText, string yearFilter, string searchType)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(CasesListBox.ItemsSource);
            if (string.IsNullOrEmpty(searchText) && (yearFilter == null || yearFilter == "الكل"))
            {
                view.Filter = null;
                UpdateSearchResultsSummary(0);
            }
            else
            {
                view.Filter = item =>
                {
                    if (item is Case caseItem)
                    {
                        bool matchesSearch = string.IsNullOrEmpty(searchText);
                        if (!matchesSearch)
                        {
                            switch (searchType)
                            {
                                case "اسم العميل":
                                    matchesSearch = caseItem.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                    break;
                                case "رقم المشترك":
                                    matchesSearch = caseItem.SubscriberNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
                                    break;
                                case "التصنيف":
                                    matchesSearch = caseItem.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
                                    break;
                                case "حالة المشكلة":
                                    matchesSearch = caseItem.Status.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                    break;
                                default: // "الكل"
                                    matchesSearch = caseItem.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                                   (caseItem.SubscriberNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                                   (caseItem.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                                   caseItem.Status.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                    break;
                            }
                        }
                        
                        bool matchesYear = yearFilter == null || yearFilter == "الكل" || 
                            (caseItem.CreatedDate?.Year.ToString() ?? "") == yearFilter;
                        
                        return matchesSearch && matchesYear;
                    }
                    return false;
                };
                
                // Count filtered results
                int resultCount = 0;
                foreach (var item in view)
                {
                    if (view.Filter(item))
                        resultCount++;
                }
                UpdateSearchResultsSummary(resultCount);
            }
        }

        private void PerformSearchWithAdvancedFilters(string searchText, string yearFilter, string searchType, string statusFilter, string categoryFilter)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(CasesListBox.ItemsSource);
            
            view.Filter = item =>
            {
                if (item is Case caseItem)
                {
                    // Basic search filter
                    bool matchesSearch = string.IsNullOrEmpty(searchText);
                    if (!matchesSearch)
                    {
                        switch (searchType)
                        {
                            case "اسم العميل":
                                matchesSearch = caseItem.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                break;
                            case "رقم المشترك":
                                matchesSearch = caseItem.SubscriberNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
                                break;
                            case "التصنيف":
                                matchesSearch = caseItem.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
                                break;
                            case "حالة المشكلة":
                                matchesSearch = caseItem.Status.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                break;
                            default: // "الكل"
                                matchesSearch = caseItem.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                               (caseItem.SubscriberNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                               (caseItem.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                               caseItem.Status.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                break;
                        }
                    }
                    
                    // Year filter
                    bool matchesYear = yearFilter == null || yearFilter == "الكل" || 
                        (caseItem.CreatedDate?.Year.ToString() ?? "") == yearFilter;
                    
                    // Status filter
                    bool matchesStatus = string.IsNullOrEmpty(statusFilter) || statusFilter == "الكل" ||
                        caseItem.Status == statusFilter;
                    
                    // Category filter
                    bool matchesCategory = string.IsNullOrEmpty(categoryFilter) || categoryFilter == "الكل" ||
                        caseItem.Category?.CategoryName == categoryFilter;
                    
                    return matchesSearch && matchesYear && matchesStatus && matchesCategory;
                }
                return false;
            };
            
            // Count filtered results
            int resultCount = 0;
            foreach (var item in view)
            {
                if (view.Filter(item))
                    resultCount++;
            }
            UpdateSearchResultsSummary(resultCount);
        }

        private void UpdateSearchResultsSummary(int resultCount)
        {
            if (SearchResultsSummary != null)
            {
                SearchResultsSummary.Text = $"تم العثور على {resultCount} نتيجة";
            }
        }

        private void YearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PerformAdvancedSearch();
        }

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PerformAdvancedSearch();
        }

        private async void YearlyNumberSearch_Click(object sender, RoutedEventArgs e)
        {
            var yearlyNumber = YearlyNumberSearchBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(yearlyNumber))
            {
                _notificationService.ShowWarning("يرجى إدخال رقم سنوي للبحث (مثال: 2024-0001)");
                return;
            }

            try
            {
                await _loadingService.ExecuteWithLoadingAsync(async () =>
                {
                    var correspondences = await _caseService.SearchCorrespondencesByYearlyNumberAsync(yearlyNumber);
                    
                    if (correspondences.Any())
                    {
                        // عرض النتائج في نافذة منفصلة أو تحديث الواجهة
                        var resultMessage = $"تم العثور على {correspondences.Count()} مراسلة بالرقم السنوي: {yearlyNumber}\n\n";
                        foreach (var correspondence in correspondences.Take(10)) // عرض أول 10 نتائج
                        {
                            resultMessage += $"• {correspondence.YearlySequenceNumber} - {correspondence.Case?.CustomerName} - {correspondence.SentDate:dd/MM/yyyy}\n";
                        }
                        
                        if (correspondences.Count() > 10)
                        {
                            resultMessage += $"\n... و {correspondences.Count() - 10} مراسلة أخرى";
                        }
                        
                        WpfMessageBox.Show(resultMessage, "نتائج البحث بالرقم السنوي", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _notificationService.ShowWarning($"لم يتم العثور على مراسلات بالرقم السنوي: {yearlyNumber}");
                    }
                }, "جاري البحث...");
            }
            catch (Exception ex)
            {
                _notificationService.ShowDatabaseError("البحث بالرقم السنوي", ex);
            }
        }

        private void CasesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CasesListBox.SelectedItem is Case selectedCase)
            {
                SelectCase(selectedCase.Id);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is string sortBy)
            {
                SortCases(sortBy);
            }
        }

        private void SortCases(string sortBy)
        {
            var currentSelection = CasesListBox.SelectedItem as Case;
            _allCases = sortBy switch
            {
                "تاريخ الإنشاء (الأحدث)" => _allCases.OrderByDescending(c => c.CreatedDate).ToList(),
                "تاريخ الإنشاء (الأقدم)" => _allCases.OrderBy(c => c.CreatedDate).ToList(),
                "اسم العميل" => _allCases.OrderBy(c => c.CustomerName).ToList(),
                "رقم المشترك" => _allCases.OrderBy(c => c.SubscriberNumber ?? "").ToList(),
                "حالة المشكلة" => _allCases.OrderBy(c => c.Status).ToList(),
                "التصنيف" => _allCases.OrderBy(c => c.Category?.CategoryName).ToList(),
                _ => _allCases.OrderByDescending(c => c.CreatedDate).ToList()
            };
            UpdateCasesList();
            
            // Re-apply current search filters after sorting
            PerformAdvancedSearch();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCase == null)
            {
                _notificationService.ShowWarning("يرجى اختيار مشكلة للطباعة");
                return;
            }

            try
            {
                var printService = _serviceProvider.GetRequiredService<IPrintService>();
                var casesToPrint = new List<Case> { _selectedCase };
                
                var printPreviewWindow = new PrintPreviewWindow(printService, casesToPrint);
                printPreviewWindow.Owner = this;
                printPreviewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("فشل في الطباعة", ex.Message, "تأكد من إعدادات الطباعة وحاول مرة أخرى");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("فشل في فتح الإعدادات", ex.Message, "تأكد من أن النافذة متاحة وحاول مرة أخرى");
            }
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCase == null)
            {
                _notificationService.ShowWarning("يرجى اختيار مشكلة لإضافة مرفق");
                return;
            }

            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "اختر ملف للمرفق",
                    Filter = "جميع الملفات|*.*|مستندات|*.pdf;*.doc;*.docx|صور|*.jpg;*.jpeg;*.png;*.gif",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    await _loadingService.ExecuteWithLoadingAsync(async () =>
                    {
                        var fileManager = _serviceProvider.GetRequiredService<IFileManagerService>();
                        var attachment = new Attachment
                        {
                            CaseId = _selectedCase.Id,
                            FileName = System.IO.Path.GetFileName(openFileDialog.FileName),
                            FilePath = openFileDialog.FileName,
                            FileType = System.IO.Path.GetExtension(openFileDialog.FileName),
                            Description = $"مرفق تم إضافته في {DateTime.Now:dd/MM/yyyy HH:mm}",
                            UploadDate = DateTime.Now,
                            UploadedById = App.CurrentUser?.Id ?? 1
                        };

                        await _caseService.AddAttachmentAsync(attachment);
                        
                        // تحديث البيانات تلقائياً
                        RefreshCaseData();
                        
                        _notificationService.ShowSuccess("تم إضافة المرفق بنجاح");
                    }, "جاري إضافة المرفق...");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowFileError("إضافة المرفق", ex);
            }
        }

        private async void DeleteAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AttachmentsDataGrid.SelectedItem is Attachment attachment)
            {
                try
                {
                    var result = WpfMessageBox.Show(
                        $"هل أنت متأكد من حذف المرفق '{attachment.FileName}'؟",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _loadingService.ExecuteWithLoadingAsync(async () =>
                        {
                            await _caseService.DeleteAttachmentAsync(attachment.Id);
                            
                            // تحديث البيانات تلقائياً
                            RefreshCaseData();
                            
                            _notificationService.ShowSuccess("تم حذف المرفق بنجاح");
                        }, "جاري حذف المرفق...");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowFileError("حذف المرفق", ex);
                }
            }
            else
            {
                _notificationService.ShowWarning("يرجى اختيار مرفق للحذف");
            }
        }

        private void AttachmentsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AttachmentsDataGrid.SelectedItem is Attachment attachment)
            {
                try
                {
                    if (System.IO.File.Exists(attachment.FilePath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = attachment.FilePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        _notificationService.ShowError("الملف غير موجود");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowFileError("فتح الملف", ex);
                }
            }
        }

        private async void CorrespondenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCase == null)
            {
                _notificationService.ShowWarning("يرجى اختيار مشكلة لإضافة مراسلة");
                return;
            }

            try
            {
                var correspondenceDialog = _serviceProvider.GetRequiredService<CorrespondenceDialog>();
                correspondenceDialog.Owner = this;
                if (correspondenceDialog.ShowDialog() == true)
                {
                    await _loadingService.ExecuteWithLoadingAsync(async () =>
                    {
                        var correspondence = new Correspondence
                        {
                            CaseId = _selectedCase.Id,
                            Sender = correspondenceDialog.Sender,
                            MessageContent = correspondenceDialog.MessageContent,
                            SentDate = DateTime.Now,
                            CreatedById = App.CurrentUser?.Id ?? 1,
                            CreatedDate = DateTime.Now
                        };

                        await _caseService.AddCorrespondenceAsync(correspondence);
                        
                        // تحديث البيانات تلقائياً
                        RefreshCaseData();
                        
                        _notificationService.ShowSuccess("تم إضافة المراسلة بنجاح");
                    }, "جاري إضافة المراسلة...");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowDatabaseError("إضافة المراسلة", ex);
            }
        }

        private async void DeleteCorrespondenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (CorrespondencesDataGrid.SelectedItem is Correspondence correspondence)
            {
                try
                {
                    var result = WpfMessageBox.Show(
                        $"هل أنت متأكد من حذف المراسلة؟",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _loadingService.ExecuteWithLoadingAsync(async () =>
                        {
                            await _caseService.DeleteCorrespondenceAsync(correspondence.Id);
                            
                            // تحديث البيانات تلقائياً
                            RefreshCaseData();
                            
                            _notificationService.ShowSuccess("تم حذف المراسلة بنجاح");
                        }, "جاري حذف المراسلة...");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowDatabaseError("حذف المراسلة", ex);
                }
            }
            else
            {
                _notificationService.ShowWarning("يرجى اختيار مراسلة للحذف");
            }
        }

        private void ShowAllCases_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allCasesWindow = new AllCasesWindow(_caseService);
                allCasesWindow.Owner = this;
                allCasesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("فشل في فتح نافذة جميع المشاكل", ex.Message, "تأكد من أن النافذة متاحة وحاول مرة أخرى");
            }
        }

        private void AdvancedSearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var advancedSearchWindow = _serviceProvider.GetRequiredService<AdvancedSearchWindow>();
                advancedSearchWindow.Owner = this;
                advancedSearchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("فشل في فتح نافذة البحث المتقدم", ex.Message, "تأكد من أن النافذة متاحة وحاول مرة أخرى");
            }
        }

        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            await _loadingService.ExecuteWithLoadingAsync(async () =>
            {
                try
                {
                    var backupService = _serviceProvider.GetRequiredService<IBackupService>();
                    await backupService.CreateBackupAsync();
                    _notificationService.ShowSuccess("تم إنشاء نسخة احتياطية بنجاح");
                }
                catch (Exception ex)
                {
                    _notificationService.ShowFileError("إنشاء النسخة الاحتياطية", ex);
                }
            }, "جاري إنشاء النسخة الاحتياطية...");
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "تصدير البيانات",
                    Filter = "ملف CSV|*.csv",
                    FileName = $"customer_issues_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await _loadingService.ExecuteWithLoadingAsync(async () =>
                    {
                        var cases = await _caseService.GetCasesForExportAsync();
                        await _caseService.ExportCasesToCsvAsync(cases, saveFileDialog.FileName);
                        _notificationService.ShowSuccess($"تم تصدير البيانات إلى {saveFileDialog.FileName}");
                    }, "جاري تصدير البيانات...");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowFileError("تصدير البيانات", ex);
            }
        }

        private async void PrintAllCasesButton_Click(object sender, RoutedEventArgs e)
        {
            await _loadingService.ExecuteWithLoadingAsync(async () =>
            {
                try
                {
                    var cases = await _caseService.GetAllCasesAsync();
                    var printService = _serviceProvider.GetRequiredService<IPrintService>();
                    
                    var printPreviewWindow = new PrintPreviewWindow(printService, cases.ToList());
                    printPreviewWindow.Owner = this;
                    printPreviewWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("فشل في طباعة جميع المشاكل", ex.Message, "تأكد من إعدادات الطباعة وحاول مرة أخرى");
                }
            }, "جاري تحميل المشاكل للطباعة...");
        }

        private async void ShowPrintPreview_Click(object sender, RoutedEventArgs e)
        {
            await _loadingService.ExecuteWithLoadingAsync(async () =>
            {
                try
                {
                    var printService = _serviceProvider.GetRequiredService<IPrintService>();
                    List<Case> casesToPrint;
                    
                    if (_selectedCase != null)
                    {
                        casesToPrint = new List<Case> { _selectedCase };
                    }
                    else
                    {
                        var cases = await _caseService.GetAllCasesAsync();
                        casesToPrint = cases.ToList();
                    }
                    
                    var printPreviewWindow = new PrintPreviewWindow(printService, casesToPrint);
                    printPreviewWindow.Owner = this;
                    printPreviewWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("فشل في معاينة الطباعة", ex.Message, "تأكد من إعدادات الطباعة وحاول مرة أخرى");
                }
            }, "جاري تحميل البيانات للمعاينة...");
        }

        // Case Card and Quick Action Event Handlers
        private void CaseCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Case caseItem)
            {
                SelectCase(caseItem.Id);
                e.Handled = true;
            }
        }

        private void QuickView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is int caseId)
            {
                // Select the case and show details
                SelectCase(caseId);
                _notificationService.ShowInfo("تم عرض تفاصيل المشكلة");
            }
        }

        private void QuickEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is int caseId)
            {
                // Select the case and open edit mode
                SelectCase(caseId);
                
                // Open the case detail window for editing
                try
                {
                    var selectedCase = _allCases.FirstOrDefault(c => c.Id == caseId);
                    if (selectedCase != null)
                    {
                        var caseDetailWindow = new CaseDetailWindow(_caseService, selectedCase);
                        caseDetailWindow.Owner = this;
                        caseDetailWindow.ShowDialog();
                        
                        // Refresh the list after editing
                        var _ = LoadInitialDataAsync();
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError("فشل في فتح نافذة التعديل", ex.Message, "تأكد من أن النافذة متاحة وحاول مرة أخرى");
                }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Add window closing event handler
            this.Closing += MainWindow_Closing;
        }
        
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent immediate closing and show confirmation dialog
            e.Cancel = true;
            
            var result = WpfMessageBox.Show(
                "هل أنت متأكد من أنك تريد الخروج من النظام؟\n\nسيتم حفظ جميع التغييرات تلقائياً.",
                "تأكيد الخروج",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // Save any pending changes before exiting
                try
                {
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    _notificationService.ShowWarning("تحذير: لم يتم حفظ بعض التغييرات", ex.Message);
                }
                
                // Remove the event handler to prevent infinite loop
                this.Closing -= MainWindow_Closing;
                
                // Allow the window to close
                e.Cancel = false;
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
