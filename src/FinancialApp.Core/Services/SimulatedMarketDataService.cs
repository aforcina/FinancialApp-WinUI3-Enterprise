using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public class SimulatedMarketDataService : IMarketDataService, IDisposable
    {
        private readonly Dictionary<string, MarketData> _marketData = new Dictionary<string, MarketData>(StringComparer.OrdinalIgnoreCase);
        private readonly Random _random = new Random();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _priceUpdateTask;
        private const int UpdateIntervalMs = 2000;

        public SimulatedMarketDataService() => InitializeMarketData();

        private void InitializeMarketData()
        {
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
                var currentPrice = stock.BasePrice + (decimal)(_random.NextDouble() * 4 - 2);
                _marketData[stock.Symbol] = new MarketData
                {
                    Symbol = stock.Symbol, LastPrice = currentPrice,
                    BidPrice = currentPrice - 0.05m, AskPrice = currentPrice + 0.05m,
                    Change = currentPrice - stock.BasePrice,
                    ChangePercent = ((currentPrice - stock.BasePrice) / stock.BasePrice) * 100,
                    Volume = _random.Next(10000000, 100000000), CompanyName = stock.Name,
                    Sector = stock.Sector, DayHigh = stock.BasePrice + 5,
                    DayLow = stock.BasePrice - 5, MarketCap = 1000000000000,
                    LastUpdate = DateTime.UtcNow
                };
            }
        }

        public Task<MarketData> GetMarketDataAsync(string symbol)
        {
            if (!_marketData.TryGetValue(symbol?.Trim() ?? string.Empty, out var data))
                return Task.FromResult<MarketData>(null);
            return Task.FromResult(Clone(data));
        }

        public Task<ObservableCollection<MarketData>> GetWatchlistAsync()
        {
            return Task.FromResult(new ObservableCollection<MarketData>(_marketData.Values.Select(Clone)));
        }

        public Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            return Task.FromResult(_marketData.TryGetValue(symbol ?? string.Empty, out var data) ? data.LastPrice : 0m);
        }

        public void StartPriceUpdates()
        {
            if (_priceUpdateTask != null && !_priceUpdateTask.IsCompleted) return;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            _priceUpdateTask = UpdatePricesAsync(_cancellationTokenSource.Token);
        }

        public void StopPriceUpdates() => _cancellationTokenSource?.Cancel();

        private async Task UpdatePricesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(UpdateIntervalMs, cancellationToken);
                    lock (_marketData)
                    {
                        foreach (var data in _marketData.Values)
                        {
                            var movement = (decimal)(_random.NextDouble() * 0.5 - 0.25);
                            var newPrice = Math.Max(0.01m, data.LastPrice + movement);
                            data.LastPrice = newPrice;
                            data.BidPrice = Math.Max(0.01m, newPrice - 0.05m);
                            data.AskPrice = newPrice + 0.05m;
                            data.Change = newPrice - data.LastPrice + movement;
                            data.ChangePercent = data.Change / newPrice * 100m;
                            data.Volume = _random.Next(10000000, 100000000);
                            data.LastUpdate = DateTime.UtcNow;
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private static MarketData Clone(MarketData data) => new MarketData
        {
            Symbol = data.Symbol, LastPrice = data.LastPrice, BidPrice = data.BidPrice,
            AskPrice = data.AskPrice, Change = data.Change, ChangePercent = data.ChangePercent,
            Volume = data.Volume, CompanyName = data.CompanyName, Sector = data.Sector,
            DayHigh = data.DayHigh, DayLow = data.DayLow, MarketCap = data.MarketCap,
            LastUpdate = data.LastUpdate
        };

        public void Dispose()
        {
            StopPriceUpdates();
            _cancellationTokenSource?.Dispose();
        }
    }
}
