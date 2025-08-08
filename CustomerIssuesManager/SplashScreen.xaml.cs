using System.Windows;

namespace CustomerIssuesManager
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        public void SetStatus(string status)
        {
            StatusText.Text = status;
        }
    }
}
