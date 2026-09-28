using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public class SimulatedPortfolioService : IPortfolioService
    {
        private readonly Dictionary<string, Portfolio> _portfolios = new Dictionary<string, Portfolio>();

        public SimulatedPortfolioService()
        {
            var portfolio = new Portfolio
            {
                Positions = new ObservableCollection<Position>
                {
                    new Position { Symbol = "AAPL", Quantity = 100, AverageCost = 208.20m, CurrentPrice = 211.40m, ConcentrationPercent = 2.5m, Sector = "Technology", CompanyName = "Apple Inc." },
                    new Position { Symbol = "MSFT", Quantity = 50, AverageCost = 415.00m, CurrentPrice = 420.50m, ConcentrationPercent = 2.5m, Sector = "Technology", CompanyName = "Microsoft Corp." },
                    new Position { Symbol = "GOOGL", Quantity = 150, AverageCost = 138.50m, CurrentPrice = 140.30m, ConcentrationPercent = 3.1m, Sector = "Technology", CompanyName = "Alphabet Inc." },
                    new Position { Symbol = "AMZN", Quantity = 75, AverageCost = 182.00m, CurrentPrice = 185.20m, ConcentrationPercent = 1.4m, Sector = "Consumer", CompanyName = "Amazon.com Inc." }
                }
            };
            RecalculatePortfolio(portfolio);
            _portfolios["ACC-001"] = portfolio;
        }

        public Task<Portfolio> GetPortfolioAsync(string accountId) => Task.FromResult(_portfolios.TryGetValue(accountId, out var portfolio) ? portfolio : null);

        public Task UpdatePortfolioAsync(Portfolio portfolio)
        {
            RecalculatePortfolio(portfolio);
            return Task.CompletedTask;
        }

        public decimal CalculateTotalValue(Portfolio portfolio) => portfolio?.Positions.Sum(p => p.MarketValue) ?? 0m;
        public decimal CalculateRiskMetrics(Portfolio portfolio) => CalculateTotalValue(portfolio) * 1.68m;

        private static void RecalculatePortfolio(Portfolio portfolio)
        {
            portfolio.TotalValue = portfolio.Positions.Sum(p => p.MarketValue);
            portfolio.TotalCostBasis = portfolio.Positions.Sum(p => p.Quantity * p.AverageCost);
            portfolio.UnrealizedPnL = portfolio.TotalValue - portfolio.TotalCostBasis;
            portfolio.DailyPnL = 24300m;
            portfolio.BuyingPower = 420000m;
        }
    }
}
