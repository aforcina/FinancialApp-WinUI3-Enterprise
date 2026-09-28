using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    /// <summary>
    /// Service interface for market data.
    /// Simulates real-time price data from a market data provider.
    /// </summary>
    public interface IMarketDataService
    {
        Task<MarketData> GetMarketDataAsync(string symbol);
        Task<ObservableCollection<MarketData>> GetWatchlistAsync();
        Task<decimal> GetCurrentPriceAsync(string symbol);
        void StartPriceUpdates();
        void StopPriceUpdates();
    }
}
