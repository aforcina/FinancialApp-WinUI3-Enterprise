using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FinancialApp.Core.Models;

namespace FinancialApp.Presentation.ViewModels
{
    public sealed class ShellViewModel : INotifyPropertyChanged
    {
        private Account _selectedAccount;
        private string _selectedSymbol = "AAPL";
        private decimal _selectedSecurityPrice;
        private string _selectedSecurityChange = "Loading...";
        private string _selectedSecurityBidAsk = "Loading...";
        private decimal _selectedSecurityPosition;
        private string _selectedSecurityVolume = "Loading...";
        private string _selectedSecurityRiskLevel = "Moderate";

        public ShellViewModel()
        {
            Accounts = new ObservableCollection<Account>
            {
                new Account("ACC-001", "Primary Trading Account", "USD", 500000m),
                new Account("ACC-002", "Hedge Fund Account", "USD", 250000m)
            };

            SelectedAccount = Accounts[0];
            NavigateToDashboardCommand = CreateNavigationCommand(typeof(Views.DashboardPage));
            NavigateToWatchlistCommand = CreateNavigationCommand(typeof(Views.WatchlistPage));
            NavigateToPositionsCommand = CreateNavigationCommand(typeof(Views.PositionsPage));
            NavigateToTradeTicketCommand = CreateNavigationCommand(typeof(Views.TradeTicketPage));
            NavigateToBlotterCommand = CreateNavigationCommand(typeof(Views.TradeBlotterPage));
            NavigateToApprovalsCommand = CreateNavigationCommand(typeof(Views.ApprovalQueuePage));
            NavigateToRiskCommand = CreateNavigationCommand(typeof(Views.RiskPage));
            NavigateToReportsCommand = CreateNavigationCommand(typeof(Views.ReportsPage));
            TradeSelectedSecurityCommand = CreateNavigationCommand(typeof(Views.TradeTicketPage));
        }

        public ObservableCollection<Account> Accounts { get; }

        public Account SelectedAccount
        {
            get => _selectedAccount;
            set { _selectedAccount = value; OnPropertyChanged(); }
        }

        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set { _selectedSymbol = value; OnPropertyChanged(); }
        }

        public decimal SelectedSecurityPrice
        {
            get => _selectedSecurityPrice;
            set { _selectedSecurityPrice = value; OnPropertyChanged(); }
        }

        public string SelectedSecurityChange
        {
            get => _selectedSecurityChange;
            set { _selectedSecurityChange = value; OnPropertyChanged(); }
        }

        public string SelectedSecurityBidAsk
        {
            get => _selectedSecurityBidAsk;
            set { _selectedSecurityBidAsk = value; OnPropertyChanged(); }
        }

        public decimal SelectedSecurityPosition
        {
            get => _selectedSecurityPosition;
            set { _selectedSecurityPosition = value; OnPropertyChanged(); }
        }

        public string SelectedSecurityVolume
        {
            get => _selectedSecurityVolume;
            set { _selectedSecurityVolume = value; OnPropertyChanged(); }
        }

        public string SelectedSecurityRiskLevel
        {
            get => _selectedSecurityRiskLevel;
            set { _selectedSecurityRiskLevel = value; OnPropertyChanged(); }
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToWatchlistCommand { get; }
        public ICommand NavigateToPositionsCommand { get; }
        public ICommand NavigateToTradeTicketCommand { get; }
        public ICommand NavigateToBlotterCommand { get; }
        public ICommand NavigateToApprovalsCommand { get; }
        public ICommand NavigateToRiskCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand TradeSelectedSecurityCommand { get; }

        public event EventHandler<Type> NavigationRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LoadSelectedSecurityAsync(FinancialApp.Core.Services.IMarketDataService marketDataService)
        {
            var quote = await marketDataService.GetMarketDataAsync(SelectedSymbol);
            if (quote == null) return;

            SelectedSecurityPrice = quote.LastPrice;
            SelectedSecurityChange = $"{quote.Change:+$0.00;-$0.00;$0.00} ({quote.ChangePercent:+0.00;-0.00;0.00}%)";
            SelectedSecurityBidAsk = $"{quote.BidPrice:C2} / {quote.AskPrice:C2}";
            SelectedSecurityVolume = quote.Volume.ToString("N0");
        }

        private ICommand CreateNavigationCommand(Type pageType)
        {
            return new RelayCommand(_ =>
            {
                NavigationRequested?.Invoke(this, pageType);
                return Task.CompletedTask;
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
