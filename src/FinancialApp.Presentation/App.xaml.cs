using Microsoft.UI.Xaml;
using FinancialApp.Presentation;

namespace FinancialApp.Presentation
{
    public partial class App : Application
    {
        public static TradingServices Services { get; private set; }

        public App()
        {
            InitializeComponent();
            Services = new TradingServices();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var window = new MainWindow();
            window.Activate();
        }
    }
}
