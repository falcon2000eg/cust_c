using CustomerIssuesManager.Core.Models;
using CustomerIssuesManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfMessageBox = System.Windows.MessageBox;

namespace CustomerIssuesManager
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly IPrintService _printService;
        private readonly List<Case> _casesToPrint;

        public PrintPreviewWindow(IPrintService printService, List<Case> casesToPrint)
        {
            InitializeComponent();
            _printService = printService;
            _casesToPrint = casesToPrint;
            
            LoadPrintPreview();
        }

        private void LoadPrintPreview()
        {
            try
            {
                // Generate preview content
                var previewContent = _printService.GeneratePrintPreview(_casesToPrint);
                PrintPreviewTextBox.Text = previewContent;
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"خطأ في تحميل معاينة الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _printService.PrintCases(_casesToPrint);
                WpfMessageBox.Show("تم إرسال الملف للطباعة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 