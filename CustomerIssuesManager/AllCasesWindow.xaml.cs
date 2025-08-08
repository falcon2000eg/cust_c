using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace CustomerIssuesManager
{
        public partial class AllCasesWindow : System.Windows.Window
    {
        private readonly ICaseService _caseService;
        public AllCasesWindow(ICaseService caseService)
        {
            this.InitializeComponent();
            _caseService = caseService;
            Loaded += AllCasesWindow_Loaded;
        }

        private async void AllCasesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var cases = await _caseService.GetAllCasesAsync();
                CasesDataGrid.ItemsSource = cases;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"خطأ في تحميل المشاكل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
