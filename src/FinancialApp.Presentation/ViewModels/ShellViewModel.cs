using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinancialApp.Core.Models;

namespace FinancialApp.Presentation.ViewModels
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Account> _accounts;
        private Account _selectedAccount;
        private string _selectedSymbol = "AAPL";
        private decimal _selectedSecurityPrice = 211.40m;
        private string _selectedSecurityChange = "+$1.82 (+0.87%))";
        private object _selectedSecurityChangeColor;
        private string _selectedSecurityBidAsk = "211.35 / 211.45";
        private decimal _selectedSecurityPosition = 100m;
        private string _selectedSecurityVolume = "52.3M";
        private string _selectedSecurityRiskLevel = "Moderate";
        private object _selectedSecurityRiskColor;

        public ShellViewModel()
        {
            LoadData();
            CreateCommands();
        }

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set { _accounts = value; OnPropertyChanged(); }
        }

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

        public object SelectedSecurityChangeColor
        {
            get => _selectedSecurityChangeColor;
            set { _selectedSecurityChangeColor = value; OnPropertyChanged(); }
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

        public object SelectedSecurityRiskColor
        {
            get => _selectedSecurityRiskColor;
            set { _selectedSecurityRiskColor = value; OnPropertyChanged(); }
        }

        public ICommand NavigateToDashboardCommand { get; private set; }
        public ICommand NavigateToWatchlistCommand { get; private set; }
        public ICommand NavigateToPositionsCommand { get; private set; }
        public ICommand NavigateToTradeTicketCommand { get; private set; }
        public ICommand NavigateToBlotterCommand { get; private set; }
        public ICommand NavigateToApprovalsCommand { get; private set; }
        public ICommand NavigateToRiskCommand { get; private set; }
        public ICommand NavigateToReportsCommand { get; private set; }
        public ICommand TradeSelectedSecurityCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadData()
        {
            Accounts = new ObservableCollection<Account>
            {
                new Account("ACC-001", "Primary Trading Account", "USD", 500000m),
                new Account("ACC-002", "Hedge Fund Account", "USD", 250000m)
            };

            SelectedAccount = Accounts[0];
        }

        private void CreateCommands()
        {
            NavigateToDashboardCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToWatchlistCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToPositionsCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToTradeTicketCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToBlotterCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToApprovalsCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToRiskCommand = new RelayCommand(async _ => await Task.CompletedTask);
            NavigateToReportsCommand = new RelayCommand(async _ => await Task.CompletedTask);
            TradeSelectedSecurityCommand = new RelayCommand(async _ => await Task.CompletedTask);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
