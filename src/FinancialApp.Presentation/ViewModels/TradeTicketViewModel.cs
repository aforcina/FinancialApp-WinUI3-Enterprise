using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FinancialApp.Core.Models;
using FinancialApp.Core.Services;

namespace FinancialApp.Presentation.ViewModels
{
    public sealed class TradeTicketViewModel : INotifyPropertyChanged
    {
        private readonly IMarketDataService _marketDataService;
        private readonly IOrderService _orderService;
        private readonly string _accountId;

        public TradeTicketViewModel(IMarketDataService marketDataService, IOrderService orderService, string accountId)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _accountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            SideOptions = new ObservableCollection<OrderSide> { OrderSide.Buy, OrderSide.Sell };
            OrderTypeOptions = new ObservableCollection<OrderType> { OrderType.Market, OrderType.Limit, OrderType.Stop };
            GetQuoteCommand = new RelayCommand(async _ => await GetQuoteAsync(), _ => !IsBusy);
            SubmitOrderCommand = new RelayCommand(async _ => await SubmitAsync(), _ => !IsBusy);
            ClearCommand = new RelayCommand(async _ => await ClearAsync(), _ => !IsBusy);
            Recalculate();
        }

        public ObservableCollection<OrderSide> SideOptions { get; }
        public ObservableCollection<OrderType> OrderTypeOptions { get; }
        public ICommand GetQuoteCommand { get; }
        public ICommand SubmitOrderCommand { get; }
        public ICommand ClearCommand { get; }

        private string _symbol = "AAPL";
        public string Symbol { get => _symbol; set { _symbol = value?.Trim().ToUpperInvariant(); OnPropertyChanged(); } }
        private OrderSide _side = OrderSide.Buy;
        public OrderSide Side { get => _side; set { _side = value; OnPropertyChanged(); } }
        private OrderType _orderType = OrderType.Limit;
        public OrderType OrderType { get => _orderType; set { _orderType = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLimitPriceVisible)); } }
        private int _quantity = 100;
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); Recalculate(); } }
        private decimal _limitPrice = 211.40m;
        public decimal LimitPrice { get => _limitPrice; set { _limitPrice = value; OnPropertyChanged(); Recalculate(); } }
        private decimal _stopPrice;
        public decimal StopPrice { get => _stopPrice; set { _stopPrice = value; OnPropertyChanged(); } }
        private decimal _currentPrice;
        public decimal CurrentPrice { get => _currentPrice; private set { _currentPrice = value; OnPropertyChanged(); Recalculate(); } }
        private decimal _estimatedCost;
        public decimal EstimatedCost { get => _estimatedCost; private set { _estimatedCost = value; OnPropertyChanged(); } }
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; private set { _isBusy = value; OnPropertyChanged(); } }
        private string _statusMessage = "Ready to enter an order.";
        public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; OnPropertyChanged(); } }
        public bool IsLimitPriceVisible => OrderType == OrderType.Limit;

        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();
        public ObservableCollection<string> ValidationErrors { get => _validationErrors; }

        private void Recalculate() => EstimatedCost = Quantity > 0 ? Quantity * (OrderType == OrderType.Market && CurrentPrice > 0 ? CurrentPrice : LimitPrice) : 0m;

        private async Task GetQuoteAsync()
        {
            IsBusy = true;
            try
            {
                var quote = await _marketDataService.GetMarketDataAsync(Symbol);
                if (quote == null) { ValidationErrors.Add($"Unknown symbol: {Symbol}"); StatusMessage = "Quote not found."; return; }
                CurrentPrice = quote.LastPrice;
                if (OrderType == OrderType.Limit) LimitPrice = quote.LastPrice;
                StatusMessage = $"Quote updated · {Symbol} {quote.LastPrice:C2}";
            }
            finally { IsBusy = false; }
        }

        private async Task SubmitAsync()
        {
            ValidationErrors.Clear();
            if (string.IsNullOrWhiteSpace(Symbol)) ValidationErrors.Add("Symbol is required.");
            if (Quantity <= 0) ValidationErrors.Add("Quantity must be greater than zero.");
            if (OrderType == OrderType.Limit && LimitPrice <= 0) ValidationErrors.Add("Limit price must be greater than zero.");
            if (ValidationErrors.Count > 0) { StatusMessage = "Correct the validation issues before submitting."; return; }

            IsBusy = true;
            try
            {
                var order = new Order { Symbol = Symbol, Side = Side, Quantity = Quantity, OrderType = OrderType, LimitPrice = OrderType == OrderType.Limit ? LimitPrice : null, StopPrice = OrderType == OrderType.Stop ? StopPrice : null, SubmittedBy = "Demo Trader" };
                await _orderService.CreateOrderAsync(order);
                await _orderService.SubmitOrderAsync(_accountId, order);
                StatusMessage = $"Order {order.OrderId} submitted for approval.";
                await ClearAsync();
            }
            catch (Exception ex) { ValidationErrors.Add(ex.Message); StatusMessage = "Order submission failed."; }
            finally { IsBusy = false; }
        }

        private Task ClearAsync()
        {
            Symbol = "AAPL"; Side = OrderSide.Buy; OrderType = OrderType.Limit; Quantity = 100; LimitPrice = 211.40m; StopPrice = 0; ValidationErrors.Clear(); StatusMessage = "Ready to enter an order."; return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
