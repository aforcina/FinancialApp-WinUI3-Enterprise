using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public sealed class MarketDataUpdatedEventArgs : EventArgs
    {
        public MarketDataUpdatedEventArgs(ObservableCollection<MarketData> quotes) => Quotes = quotes;
        public ObservableCollection<MarketData> Quotes { get; }
    }

    public interface IMarketDataService
    {
        event EventHandler<MarketDataUpdatedEventArgs> MarketDataUpdated;
        Task<MarketData> GetMarketDataAsync(string symbol);
        Task<ObservableCollection<MarketData>> GetWatchlistAsync();
        Task<decimal> GetCurrentPriceAsync(string symbol);
        void StartPriceUpdates();
        void StopPriceUpdates();
    }
}
