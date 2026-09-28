using System;
using FinancialApp.Core.Services;

namespace FinancialApp.Presentation
{
    public sealed class TradingServices : IDisposable
    {
        public SimulatedMarketDataService MarketData { get; } = new SimulatedMarketDataService();
        public SimulatedPortfolioService Portfolio { get; } = new SimulatedPortfolioService();
        public SimulatedOrderService Orders { get; } = new SimulatedOrderService();

        public TradingServices()
        {
            MarketData.StartPriceUpdates();
        }

        public void Dispose() => MarketData.Dispose();
    }
}
