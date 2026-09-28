using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    /// <summary>
    /// Simulated portfolio service.
    /// In production, this would connect to a backend portfolio management system.
    /// </summary>
    public class SimulatedPortfolioService : IPortfolioService
    {
        private readonly Dictionary<string, Portfolio> _portfolios;

        public SimulatedPortfolioService()
        {
            _portfolios = new Dictionary<string, Portfolio>();
            InitializePortfolios();
        }

        private void InitializePortfolios()
        {
            // Create sample portfolio for ACC-001
            var portfolio = new Portfolio
            {
                Positions = new ObservableCollection<Position>
                {
                    new Position
                    {
                        Symbol = "AAPL",
                        Quantity = 100,
                        AverageCost = 208.20m,
                        CurrentPrice = 211.40m,
                        ConcentrationPercent = 2.5m,
                        Sector = "Technology",
                        CompanyName = "Apple Inc."
                    },
                    new Position
                    {
                        Symbol = "MSFT",
                        Quantity = 50,
                        AverageCost = 415.00m,
                        CurrentPrice = 420.50m,
                        ConcentrationPercent = 2.5m,
                        Sector = "Technology",
                        CompanyName = "Microsoft Corp."
                    },
                    new Position
                    {
                        Symbol = "GOOGL",
                        Quantity = 150,
                        AverageCost = 138.50m,
                        CurrentPrice = 140.30m,
                        ConcentrationPercent = 3.1m,
                        Sector = "Technology",
                        CompanyName = "Alphabet Inc."
                    },
                    new Position
                    {
                        Symbol = "AMZN",
                        Quantity = 75,
                        AverageCost = 182.00m,
                        CurrentPrice = 185.20m,
                        ConcentrationPercent = 1.4m,
                        Sector = "Consumer",
                        CompanyName = "Amazon.com Inc."
                    }
                }
            };

            RecalculatePortfolio(portfolio);
            _portfolios["ACC-001"] = portfolio;
        }

        public Task<Portfolio> GetPortfolioAsync(string accountId)
        {
            if (_portfolios.TryGetValue(accountId, out var portfolio))
            {
                return Task.FromResult(portfolio);
            }

            return Task.FromResult<Portfolio>(null);
        }

        public Task UpdatePortfolioAsync(Portfolio portfolio)
        {
            // Simulate saving portfolio
            RecalculatePortfolio(portfolio);
            return Task.CompletedTask;
        }

        public decimal CalculateTotalValue(Portfolio portfolio)
        {
            return portfolio.Positions.Sum(p => p.MarketValue);
        }

        public decimal CalculateRiskMetrics(Portfolio portfolio)
        {
            var totalValue = CalculateTotalValue(portfolio);
            var leverage = 1.68m; // Sample leverage
            return totalValue * leverage;
        }

        private void RecalculatePortfolio(Portfolio portfolio)
        {
            portfolio.TotalValue = CalculateTotalValue(portfolio);
            portfolio.TotalCostBasis = portfolio.Positions.Sum(p => p.Quantity * p.AverageCost);
            portfolio.UnrealizedPnL = portfolio.TotalValue - portfolio.TotalCostBasis;
            portfolio.DailyPnL = 24300m; // Simulated
            portfolio.BuyingPower = 420000m; // Simulated
        }
    }
}
