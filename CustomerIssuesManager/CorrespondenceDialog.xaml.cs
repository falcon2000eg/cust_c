using System.Windows;
using CustomerIssuesManager.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerIssuesManager
{
    public partial class CorrespondenceDialog : Window
    {
        public string Sender { get; private set; } = string.Empty;
        public string MessageContent { get; private set; } = string.Empty;
        private readonly ICaseService? _caseService;

        public CorrespondenceDialog()
        {
            InitializeComponent();
            Loaded += CorrespondenceDialog_Loaded;
            
            // الحصول على خدمة الحالات من مزود الخدمات
            _caseService = ((App)System.Windows.Application.Current).Services.GetRequiredService<ICaseService>();
        }

        private async void CorrespondenceDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SenderTextBox.Focus();
            
            // عرض الرقم السنوي التالي
            await LoadNextYearlyNumber();
        }

        private async Task LoadNextYearlyNumber()
        {
            try
            {
                if (_caseService != null)
                {
                    var nextNumber = await _caseService.GetNextYearlySequenceNumberAsync(DateTime.Now.Year);
                    NextYearlyNumberTextBlock.Text = nextNumber;
                }
                else
                {
                    NextYearlyNumberTextBlock.Text = "غير متاح";
                }
            }
            catch
            {
                NextYearlyNumberTextBlock.Text = "خطأ في التحميل";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SenderTextBox.Text))
            {
                System.Windows.MessageBox.Show("يرجى إدخال اسم المرسل", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                SenderTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(MessageContentTextBox.Text))
            {
                System.Windows.MessageBox.Show("يرجى إدخال محتوى المراسلة", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
                MessageContentTextBox.Focus();
                return;
            }

            Sender = SenderTextBox.Text.Trim();
            MessageContent = MessageContentTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 