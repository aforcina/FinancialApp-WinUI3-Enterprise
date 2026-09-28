using Microsoft.UI.Xaml;
using FinancialApp.Presentation.ViewModels;

namespace FinancialApp.Presentation
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new ShellViewModel();
            MainContentFrame.Navigate(typeof(Views.DashboardPage));
        }
    }
}
