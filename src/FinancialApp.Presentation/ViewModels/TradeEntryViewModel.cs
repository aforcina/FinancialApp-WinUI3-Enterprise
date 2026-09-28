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
    public class TradeEntryViewModel : INotifyPropertyChanged
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ITradeValidationService _validationService;
        private readonly IApprovalWorkflowService _approvalService;
        private readonly Account _account;
        private readonly Portfolio _portfolio;
        private readonly RiskLimit _riskLimit;
        private readonly string _currentUserId;

        public TradeEntryViewModel(
            IPortfolioService portfolioService,
            ITradeValidationService validationService,
            IApprovalWorkflowService approvalService,
            Account account,
            Portfolio portfolio,
            RiskLimit riskLimit,
            string currentUserId)
        {
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
            _riskLimit = riskLimit ?? throw new ArgumentNullException(nameof(riskLimit));
            _currentUserId = currentUserId ?? throw new ArgumentNullException(nameof(currentUserId));

            SubmitTradeCommand = new RelayCommand(async _ => await SubmitTradeAsync(), _ => !IsBusy);
            ClearFormCommand = new RelayCommand(async _ => await ClearFormAsync(), _ => !IsBusy);
            UpdateMarketPriceCommand = new RelayCommand(async _ => await UpdateMarketPriceAsync(), _ => !IsBusy);

            LoadDefaults();
        }

        private string _symbol = "MSFT";
        public string Symbol
        {
            get => _symbol;
            set { _symbol = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _quantity = 100m;
        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _limitPrice = 430.50m;
        public decimal LimitPrice
        {
            get => _limitPrice;
            set { _limitPrice = value; OnPropertyChanged(); RecalculateNotional(); }
        }

        private decimal _marketPrice = 428.75m;
        public decimal MarketPrice
        {
            get => _marketPrice;
            set { _marketPrice = value; OnPropertyChanged(); RecalculateNotional(); }
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

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();
        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set { _validationErrors = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public decimal AvailableCash => _account.AvailableCash.Amount;
        public string AccountNumber => _account.AccountNumber;
        public string AccountName => _account.Name;

        public ICommand SubmitTradeCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand UpdateMarketPriceCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadDefaults()
        {
            StatusMessage = "Ready to enter trade.";
            ValidationErrors.Clear();
        }

        private void RecalculateNotional()
        {
            NotionalValue = Quantity * MarketPrice;
        }

        private async Task UpdateMarketPriceAsync()
        {
            IsBusy = true;
            try
            {
                // In a real scenario, this would fetch from market data service
                // For now, we'll simulate with a small random variation
                var variation = (decimal)(new Random().NextDouble() - 0.5) * 2m;
                MarketPrice += variation;
                StatusMessage = $"Market price updated: {Symbol} @ {MarketPrice:C2}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SubmitTradeAsync()
        {
            IsBusy = true;
            ValidationErrors.Clear();

            try
            {
                // Create trade request
                var tradeRequest = new TradeRequest(
                    _account.Id,
                    Symbol,
                    Quantity,
                    LimitPrice,
                    MarketPrice,
                    _account.Currency,
                    Direction,
                    _currentUserId,
                    ApprovalLevel.Auto);

                // Validate trade
                var (isValid, errors) = await _validationService.ValidateTradeAsync(
                    tradeRequest,
                    _account,
                    _portfolio,
                    _riskLimit);

                if (!isValid)
                {
                    foreach (var error in errors)
                        ValidationErrors.Add(error);
                    StatusMessage = "Trade validation failed. Please review errors.";
                    return;
                }

                // Submit and request approval
                tradeRequest.Submit();
                await _approvalService.RequestApprovalAsync(tradeRequest, ApprovalLevel.Auto);

                StatusMessage = $"Trade submitted successfully. ID: {tradeRequest.Id:N}";
                await ClearFormAsync();
            }
            catch (Exception ex)
            {
                ValidationErrors.Add($"Error: {ex.Message}");
                StatusMessage = "Failed to submit trade.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClearFormAsync()
        {
            Symbol = "MSFT";
            Quantity = 100m;
            LimitPrice = 430.50m;
            MarketPrice = 428.75m;
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
