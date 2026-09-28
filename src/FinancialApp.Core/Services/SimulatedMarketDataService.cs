using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public class SimulatedMarketDataService : IMarketDataService, IDisposable
    {
        private readonly System.Collections.Generic.Dictionary<string, MarketData> _marketData = new System.Collections.Generic.Dictionary<string, MarketData>(StringComparer.OrdinalIgnoreCase);
        private readonly Random _random = new Random();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _priceUpdateTask;
        private const int UpdateIntervalMs = 2000;
        public event EventHandler<MarketDataUpdatedEventArgs> MarketDataUpdated;

        public SimulatedMarketDataService() => InitializeMarketData();
        private void InitializeMarketData()
        {
            var stocks = new[] { new { S="AAPL",N="Apple Inc.",P=211.40m,Sector="Technology" }, new { S="MSFT",N="Microsoft Corp.",P=420.50m,Sector="Technology" }, new { S="GOOGL",N="Alphabet Inc.",P=140.30m,Sector="Technology" }, new { S="AMZN",N="Amazon.com Inc.",P=185.20m,Sector="Consumer" }, new { S="NVDA",N="NVIDIA Corp.",P=875.45m,Sector="Technology" }, new { S="TSLA",N="Tesla Inc.",P=245.80m,Sector="Automotive" }, new { S="META",N="Meta Platforms Inc.",P=481.50m,Sector="Technology" }, new { S="JPM",N="JPMorgan Chase",P=198.75m,Sector="Financial" } };
            foreach (var stock in stocks)
            {
                var price = stock.P + (decimal)(_random.NextDouble() * 4 - 2);
                _marketData[stock.S] = new MarketData { Symbol=stock.S, CompanyName=stock.N, Sector=stock.Sector, LastPrice=price, BidPrice=price-.05m, AskPrice=price+.05m, Change=price-stock.P, ChangePercent=(price-stock.P)/stock.P*100m, Volume=_random.Next(10000000,100000000), DayHigh=stock.P+5, DayLow=stock.P-5, MarketCap=1000000000000, LastUpdate=DateTime.UtcNow };
            }
        }
        public Task<MarketData> GetMarketDataAsync(string symbol) => Task.FromResult(_marketData.TryGetValue(symbol?.Trim() ?? "", out var d) ? Clone(d) : null);
        public Task<ObservableCollection<MarketData>> GetWatchlistAsync() => Task.FromResult(new ObservableCollection<MarketData>(_marketData.Values.Select(Clone)));
        public Task<decimal> GetCurrentPriceAsync(string symbol) => Task.FromResult(_marketData.TryGetValue(symbol ?? "", out var d) ? d.LastPrice : 0m);
        public void StartPriceUpdates() { if (_priceUpdateTask != null && !_priceUpdateTask.IsCompleted) return; _cancellationTokenSource = new CancellationTokenSource(); _priceUpdateTask = UpdatePricesAsync(_cancellationTokenSource.Token); }
        public void StopPriceUpdates() => _cancellationTokenSource?.Cancel();
        private async Task UpdatePricesAsync(CancellationToken token)
        {
            try { while (true) { await Task.Delay(UpdateIntervalMs, token); lock (_marketData) foreach (var d in _marketData.Values) { var movement=(decimal)(_random.NextDouble()*.5-.25); var old=d.LastPrice; d.LastPrice=Math.Max(.01m,old+movement); d.BidPrice=d.LastPrice-.05m; d.AskPrice=d.LastPrice+.05m; d.Change=d.LastPrice-(d.LastPrice-movement); d.ChangePercent=d.Change/d.LastPrice*100m; d.Volume=_random.Next(10000000,100000000); d.LastUpdate=DateTime.UtcNow; } PublishSnapshot(); } } catch (OperationCanceledException) { }
        }
        private static MarketData Clone(MarketData d) => new MarketData { Symbol=d.Symbol,CompanyName=d.CompanyName,Sector=d.Sector,LastPrice=d.LastPrice,BidPrice=d.BidPrice,AskPrice=d.AskPrice,Change=d.Change,ChangePercent=d.ChangePercent,Volume=d.Volume,DayHigh=d.DayHigh,DayLow=d.DayLow,MarketCap=d.MarketCap,LastUpdate=d.LastUpdate };
        private void PublishSnapshot() { ObservableCollection<MarketData> snapshot; lock (_marketData) snapshot=new ObservableCollection<MarketData>(_marketData.Values.Select(Clone)); MarketDataUpdated?.Invoke(this,new MarketDataUpdatedEventArgs(snapshot)); }
        public void Dispose() { StopPriceUpdates(); _cancellationTokenSource?.Dispose(); }
    }
}
