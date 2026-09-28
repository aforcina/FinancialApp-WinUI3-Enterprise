using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FinancialApp.Application.Services;
using FinancialApp.Core.Interfaces;
using FinancialApp.Core.Models;

namespace FinancialApp.Presentation.ViewModels
{
    public class EquityTradeEntryViewModel : INotifyPropertyChanged
    {
        private readonly IEquityTradingService _tradingService;
        private readonly IMarketDataService _marketDataService;
        private readonly IRiskManagementService _riskService;
        private readonly IApprovalWorkflowService _approvalService;
        private readonly Account _account;
        private readonly EquityPortfolio _portfolio;
        private readonly string _currentUserId;

        public EquityTradeEntryViewModel(
            IEquityTradingService tradingService,
            IMarketDataService marketDataService,
            IRiskManagementService riskService,
            IApprovalWorkflowService approvalService,
            Account account,
            EquityPortfolio portfolio,
            string currentUserId)
        {
            _tradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _riskService = riskService ?? throw new ArgumentNullException(nameof(riskService));
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
            _currentUserId = currentUserId ?? throw new ArgumentNullException(nameof(currentUserId));

            SubmitOrderCommand = new RelayCommand(async _ => await SubmitOrderAsync(), _ => !IsBusy && CanSubmit);
            GetQuoteCommand = new RelayCommand(async _ => await GetMarketQuoteAsync(), _ => !IsBusy);
            ClearFormCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => !IsBusy);

            Direction = TradeDirection.Buy;
            LoadDefaults();
        }

        private string _symbol = "AAPL";
        public string Symbol
        {
            get => _symbol;
            set { _symbol = value?.ToUpperInvariant(); OnPropertyChanged(); }
        }

        private decimal _quantity = 100m;
        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _limitPrice = 150.00m;
        public decimal LimitPrice
        {
            get => _limitPrice;
            set { _limitPrice = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _currentPrice = 150.00m;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set { _currentPrice = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _bidPrice = 149.95m;
        public decimal BidPrice
        {
            get => _bidPrice;
            set { _bidPrice = value; OnPropertyChanged(); }
        }

        private decimal _askPrice = 150.05m;
        public decimal AskPrice
        {
            get => _askPrice;
            set { _askPrice = value; OnPropertyChanged(); }
        }

        private TradeDirection _direction = TradeDirection.Buy;
        public TradeDirection Direction
        {
            get => _direction;
            set { _direction = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _notionalValue;
        public decimal NotionalValue
        {
            get => _notionalValue;
            set { _notionalValue = value; OnPropertyChanged(); }
        }

        private decimal _estimatedCommission;
        public decimal EstimatedCommission
        {
            get => _estimatedCommission;
            set { _estimatedCommission = value; OnPropertyChanged(); }
        }

        private decimal _totalCost;
        public decimal TotalCost
        {
            get => _totalCost;
            set { _totalCost = value; OnPropertyChanged(); }
        }

        private decimal _buyingPower;
        public decimal BuyingPower
        {
            get => _buyingPower;
            set { _buyingPower = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();
        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set { _validationErrors = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private bool _canSubmit = true;
        public bool CanSubmit
        {
            get => _canSubmit;
            set { _canSubmit = value; OnPropertyChanged(); }
        }

        public decimal AvailableCash => _account.AvailableCash.Amount;
        public string AccountName => _account.Name;

        public ICommand SubmitOrderCommand { get; }
        public ICommand GetQuoteCommand { get; }
        public ICommand ClearFormCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadDefaults()
        {
            StatusMessage = "Ready to submit order.";
            ValidationErrors.Clear();
            RecalculateNotional();
        }

        private void RecalculateNotional()
        {
            NotionalValue = Quantity * CurrentPrice;
            EstimatedCommission = NotionalValue * 0.001m; // 0.1% commission
            TotalCost = NotionalValue + EstimatedCommission;
        }

        private async Task GetMarketQuoteAsync()
        {
            IsBusy = true;
            try
            {
                var price = await _marketDataService.GetCurrentPriceAsync(Symbol);
                CurrentPrice = price;
                BidPrice = price * 0.9999m;
                AskPrice = price * 1.0001m;
                StatusMessage = $"Quote received for {Symbol}: {CurrentPrice:C2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error getting quote: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SubmitOrderAsync()
        {
            IsBusy = true;
            ValidationErrors.Clear();

            try
            {
                // Validate based on direction
                if (Direction == TradeDirection.Buy)
                {
                    var (isValid, message) = await _tradingService.ValidateBuyOrderAsync(
                        _account, _portfolio, Symbol, Quantity, LimitPrice);
                    if (!isValid)
                    {
                        ValidationErrors.Add(message);
                        StatusMessage = "Buy order validation failed.";
                        return;
                    }
                }
                else
                {
                    var (isValid, message) = await _tradingService.ValidateSellOrderAsync(
                        _portfolio, Symbol, Quantity);
                    if (!isValid)
                    {
                        ValidationErrors.Add(message);
                        StatusMessage = "Sell order validation failed.";
                        return;
                    }
                }

                // Check risk limits
                var trade = new TradeRequest(
                    _account.Id, Symbol, Quantity, LimitPrice, CurrentPrice, _account.Currency,
                    Direction, _currentUserId, ApprovalLevel.Manager);

                var breaches = await _riskService.CheckRiskLimitBreachesAsync(_account, _portfolio, trade);
                if (breaches.Count > 0)
                {
                    foreach (var breach in breaches)
                        ValidationErrors.Add(breach);
                    StatusMessage = "Order rejected due to risk limit breaches.";
                    return;
                }

                // Submit order
                trade.Submit();
                await _approvalService.RequestApprovalAsync(trade, ApprovalLevel.Manager);

                StatusMessage = $"Order submitted for approval. ID: {trade.Id:N}";
                await ClearFormAsync();
            }
            catch (Exception ex)
            {
                ValidationErrors.Add($"Error: {ex.Message}");
                StatusMessage = "Failed to submit order.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClearFormAsync()
        {
            Symbol = "AAPL";
            Quantity = 100m;
            LimitPrice = 150.00m;
            CurrentPrice = 150.00m;
            Direction = TradeDirection.Buy;
            ValidationErrors.Clear();
            StatusMessage = "Form cleared.";
            await Task.CompletedTask;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
