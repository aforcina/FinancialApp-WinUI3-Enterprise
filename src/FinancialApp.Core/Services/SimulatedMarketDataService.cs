using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    /// <summary>
    /// Simulated market data service.
    /// In production, this would connect to Bloomberg, Reuters, or other data providers.
    /// For the demo, we simulate realistic price movements.
    /// </summary>
    public class SimulatedMarketDataService : IMarketDataService
    {
        private readonly Dictionary<string, MarketData> _marketData;
        private readonly Random _random;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _priceUpdateTask;
        private const int UPDATE_INTERVAL_MS = 2000; // Update prices every 2 seconds

        public SimulatedMarketDataService()
        {
            _marketData = new Dictionary<string, MarketData>();
            _random = new Random();
            InitializeMarketData();
        }

        private void InitializeMarketData()
        {
            // Initialize with sample stocks
            var stocks = new[]
            {
                new { Symbol = "AAPL", Name = "Apple Inc.", BasePrice = 211.40m, Sector = "Technology" },
                new { Symbol = "MSFT", Name = "Microsoft Corp.", BasePrice = 420.50m, Sector = "Technology" },
                new { Symbol = "GOOGL", Name = "Alphabet Inc.", BasePrice = 140.30m, Sector = "Technology" },
                new { Symbol = "AMZN", Name = "Amazon.com Inc.", BasePrice = 185.20m, Sector = "Consumer" },
                new { Symbol = "NVDA", Name = "NVIDIA Corp.", BasePrice = 875.45m, Sector = "Technology" },
                new { Symbol = "TSLA", Name = "Tesla Inc.", BasePrice = 245.80m, Sector = "Automotive" },
                new { Symbol = "META", Name = "Meta Platforms Inc.", BasePrice = 481.50m, Sector = "Technology" },
                new { Symbol = "JPM", Name = "JPMorgan Chase", BasePrice = 198.75m, Sector = "Financial" }
            };

            foreach (var stock in stocks)
            {
                var change = (decimal)(_random.NextDouble() * 4 - 2); // -2 to +2
                var currentPrice = stock.BasePrice + change;

                _marketData[stock.Symbol] = new MarketData
                {
                    Symbol = stock.Symbol,
                    LastPrice = currentPrice,
                    BidPrice = currentPrice - 0.05m,
                    AskPrice = currentPrice + 0.05m,
                    Change = change,
                    ChangePercent = (change / stock.BasePrice) * 100,
                    Volume = (long)(_random.Next(10000000, 100000000)),
                    CompanyName = stock.Name,
                    Sector = stock.Sector,
                    DayHigh = stock.BasePrice + 5,
                    DayLow = stock.BasePrice - 5,
                    MarketCap = 1000000000000, // 1 trillion
                    LastUpdate = DateTime.UtcNow
                };
            }
        }

        public Task<MarketData> GetMarketDataAsync(string symbol)
        {
            if (_marketData.TryGetValue(symbol, out var data))
            {
                return Task.FromResult(new MarketData
                {
                    Symbol = data.Symbol,
                    LastPrice = data.LastPrice,
                    BidPrice = data.BidPrice,
                    AskPrice = data.AskPrice,
                    Change = data.Change,
                    ChangePercent = data.ChangePercent,
                    Volume = data.Volume,
                    CompanyName = data.CompanyName,
                    Sector = data.Sector,
                    DayHigh = data.DayHigh,
                    DayLow = data.DayLow,
                    MarketCap = data.MarketCap,
                    LastUpdate = data.LastUpdate
                });
            }

            return Task.FromResult<MarketData>(null);
        }

        public Task<ObservableCollection<MarketData>> GetWatchlistAsync()
        {
            var watchlist = new ObservableCollection<MarketData>();
            foreach (var data in _marketData.Values)
            {
                watchlist.Add(data);
            }
            return Task.FromResult(watchlist);
        }

        public Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            if (_marketData.TryGetValue(symbol, out var data))
            {
                return Task.FromResult(data.LastPrice);
            }
            return Task.FromResult(0m);
        }

        public void StartPriceUpdates()
        {
            if (_priceUpdateTask != null && !_priceUpdateTask.IsCompleted)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _priceUpdateTask = UpdatePricesAsync(_cancellationTokenSource.Token);
        }

        public void StopPriceUpdates()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task UpdatePricesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(UPDATE_INTERVAL_MS, cancellationToken);

                    // Simulate price movements (small random walk)
                    foreach (var symbol in _marketData.Keys.ToList())
                    {
                        var data = _marketData[symbol];
                        var priceChange = (decimal)(_random.NextDouble() * 0.5 - 0.25); // -0.25 to +0.25
                        var newPrice = Math.Max(data.LastPrice + priceChange, 0.01m);

                        data.LastPrice = newPrice;
                        data.BidPrice = newPrice - 0.05m;
                        data.AskPrice = newPrice + 0.05m;
                        data.Change = newPrice - (newPrice - priceChange); // Show movement
                        data.ChangePercent = (data.Change / newPrice) * 100;
                        data.Volume = (long)(_random.Next(10000000, 100000000));
                        data.LastUpdate = DateTime.UtcNow;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
