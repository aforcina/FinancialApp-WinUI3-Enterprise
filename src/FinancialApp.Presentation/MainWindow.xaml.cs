using System;
using Microsoft.UI.Xaml;
using FinancialApp.Core.Services;
using FinancialApp.Presentation.ViewModels;

namespace FinancialApp.Presentation
{
    public sealed partial class MainWindow : Window
    {
        private readonly ShellViewModel _viewModel;
        private readonly SimulatedMarketDataService _marketDataService;

        public MainWindow()
        {
            InitializeComponent();

            _marketDataService = new SimulatedMarketDataService();
            _viewModel = new ShellViewModel();
            _viewModel.NavigationRequested += OnNavigationRequested;
            DataContext = _viewModel;

            _marketDataService.StartPriceUpdates();
            MainContentFrame.Navigate(typeof(Views.DashboardPage));
            _ = RefreshSelectedSecurityAsync();
        }

        private void OnNavigationRequested(object sender, Type pageType)
        {
            MainContentFrame.Navigate(pageType);
        }

        private async System.Threading.Tasks.Task RefreshSelectedSecurityAsync()
        {
            await _viewModel.LoadSelectedSecurityAsync(_marketDataService);
        }
    }
}
