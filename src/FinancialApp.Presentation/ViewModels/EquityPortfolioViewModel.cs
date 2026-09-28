using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FinancialApp.Application.Services;
using FinancialApp.Core.Interfaces;
using FinancialApp.Core.Models;

namespace FinancialApp.Presentation.ViewModels
{
    public class EquityPortfolioViewModel : INotifyPropertyChanged
    {
        private readonly IPositionService _positionService;
        private readonly IMarketDataService _marketDataService;
        private readonly IEquityTradingService _tradingService;
        private readonly IRiskManagementService _riskService;
        private readonly EquityPortfolio _portfolio;
        private readonly Account _account;

        public EquityPortfolioViewModel(
            IPositionService positionService,
            IMarketDataService marketDataService,
            IEquityTradingService tradingService,
            IRiskManagementService riskService,
            EquityPortfolio portfolio,
            Account account)
        {
            _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _tradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
            _riskService = riskService ?? throw new ArgumentNullException(nameof(riskService));
            _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
            _account = account ?? throw new ArgumentNullException(nameof(account));

            RefreshCommand = new RelayCommand(async _ => await RefreshPortfolioAsync(), _ => !IsBusy);
            UpdatePricesCommand = new RelayCommand(async _ => await UpdatePricesAsync(), _ => !IsBusy);
            ClosePositionCommand = new RelayCommand(async _ => await CloseSelectedPositionAsync(), CanClosePosition);
        }

        private ObservableCollection<EquityPosition> _positions = new ObservableCollection<EquityPosition>();
        public ObservableCollection<EquityPosition> Positions
        {
            get => _positions;
            set { _positions = value; OnPropertyChanged(); }
        }

        private EquityPosition _selectedPosition;
        public EquityPosition SelectedPosition
        {
            get => _selectedPosition;
            set { _selectedPosition = value; OnPropertyChanged(); }
        }

        private decimal _totalMarketValue;
        public decimal TotalMarketValue
        {
            get => _totalMarketValue;
            set { _totalMarketValue = value; OnPropertyChanged(); }
        }

        private decimal _totalCostBasis;
        public decimal TotalCostBasis
        {
            get => _totalCostBasis;
            set { _totalCostBasis = value; OnPropertyChanged(); }
        }

        private decimal _totalUnrealizedGainLoss;
        public decimal TotalUnrealizedGainLoss
        {
            get => _totalUnrealizedGainLoss;
            set { _totalUnrealizedGainLoss = value; OnPropertyChanged(); }
        }

        private decimal _totalUnrealizedGainLossPercentage;
        public decimal TotalUnrealizedGainLossPercentage
        {
            get => _totalUnrealizedGainLossPercentage;
            set { _totalUnrealizedGainLossPercentage = value; OnPropertyChanged(); }
        }

        private decimal _buyingPower;
        public decimal BuyingPower
        {
            get => _buyingPower;
            set { _buyingPower = value; OnPropertyChanged(); }
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

        public decimal AvailableCash => _account.AvailableCash.Amount;
        public string AccountName => _account.Name;

        public ICommand RefreshCommand { get; }
        public ICommand UpdatePricesCommand { get; }
        public ICommand ClosePositionCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CanClosePosition(object obj) => SelectedPosition != null && !IsBusy;

        public async Task RefreshPortfolioAsync()
        {
            IsBusy = true;
            try
            {
                var positions = await _positionService.GetAllPositionsAsync(_portfolio);
                Positions.Clear();
                foreach (var pos in positions)
                    Positions.Add(pos);

                var metrics = await _positionService.CalculatePortfolioMetricsAsync(_portfolio, _account);
                TotalMarketValue = metrics.TotalMarketValue;
                TotalCostBasis = metrics.TotalCostBasis;
                TotalUnrealizedGainLoss = metrics.UnrealizedGainLoss;
                TotalUnrealizedGainLossPercentage = metrics.UnrealizedGainLossPercentage;
                BuyingPower = await _tradingService.CalculateBuyingPowerAsync(_account, _portfolio);

                StatusMessage = $"Portfolio refreshed. {Positions.Count} positions. Total P&L: {TotalUnrealizedGainLoss:C2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing portfolio: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdatePricesAsync()
        {
            IsBusy = true;
            try
            {
                var symbols = Positions.Select(p => p.Symbol).ToArray();
                if (symbols.Length == 0)
                {
                    StatusMessage = "No positions to update.";
                    return;
                }

                var priceMap = await _marketDataService.GetMultiplePricesAsync(symbols);
                _portfolio.UpdateAllPrices(priceMap);

                await RefreshPortfolioAsync();
                StatusMessage = $"Prices updated for {priceMap.Count} symbols.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating prices: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CloseSelectedPositionAsync()
        {
            if (SelectedPosition == null) return;

            IsBusy = true;
            try
            {
                // Create a sell trade for the entire position
                var sellTrade = new TradeRequest(
                    _account.Id,
                    SelectedPosition.Symbol,
                    SelectedPosition.Quantity,
                    SelectedPosition.CurrentPrice * 0.99m, // 1% haircut for market impact
                    SelectedPosition.CurrentPrice,
                    _account.Currency,
                    TradeDirection.Sell,
                    "system",
                    ApprovalLevel.Auto);

                StatusMessage = $"Sell order created for {SelectedPosition.Symbol}: {SelectedPosition.Quantity} shares";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error closing position: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
