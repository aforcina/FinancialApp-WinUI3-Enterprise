using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FinancialApp.Application.Services;
using FinancialApp.Core.Models;

namespace FinancialApp.Presentation.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute ?? (_ => true);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter)
        {
            _ = ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!CanExecute(parameter)) return;
            await _executeAsync(parameter);
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class PortfolioViewModel : INotifyPropertyChanged
    {
        private readonly IPortfolioService _portfolioService;
        private readonly Account _account;
        private readonly Portfolio _portfolio;

        public PortfolioViewModel(IPortfolioService portfolioService, Account account, Portfolio portfolio)
        {
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));

            TradeCommand = new RelayCommand(async _ => await SubmitTradeAsync());
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());

            LoadPortfolio();
        }

        public ObservableCollection<string> Positions { get; } = new ObservableCollection<string>();

        private string _symbol = "MSFT";
        public string Symbol
        {
            get => _symbol;
            set { _symbol = value; OnPropertyChanged(); }
        }

        private decimal _quantity = 100;
        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        private decimal _price = 420.25m;
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private decimal _currentPrice = 430.50m;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set { _currentPrice = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand TradeCommand { get; }
        public ICommand RefreshCommand { get; }

        public decimal AvailableCash => _account.AvailableCash.Amount;

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadPortfolio()
        {
            Positions.Clear();
            foreach (var position in _portfolio.Positions)
            {
                Positions.Add($"{position.Symbol}: {position.Quantity} shares @ {position.AveragePrice:C}");
            }

            OnPropertyChanged(nameof(AvailableCash));
            StatusMessage = "Portfolio loaded.";
        }

        private async Task SubmitTradeAsync()
        {
            try
            {
                var trade = new Trade(_account.Id, Symbol, Quantity, Price, _account.Currency, TradeDirection.Buy, DateTime.UtcNow);
                trade.Approve();

                _portfolioService.ExecuteTrade(_portfolio, _account, trade, CurrentPrice);
                LoadPortfolio();
                StatusMessage = $"Trade executed successfully for {trade.Symbol}.";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }

            await Task.CompletedTask;
        }

        private async Task RefreshAsync()
        {
            var summary = _portfolioService.GetSummary(_portfolio, _account, CurrentPrice);
            StatusMessage = $"Portfolio summary refreshed. Cash: {summary.TotalCash:C}, Positions: {summary.PositionCount}, MV: {summary.TotalMarketValue:C}";
            await Task.CompletedTask;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
