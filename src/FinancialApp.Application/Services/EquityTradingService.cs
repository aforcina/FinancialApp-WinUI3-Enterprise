using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Core.Interfaces;
using FinancialApp.Core.Models;

namespace FinancialApp.Application.Services
{
    public class EquityTradingService : IEquityTradingService
    {
        private const decimal MarginMultiplier = 2m; // 2:1 margin for equities
        private const decimal PriceDeviation = 0.25m; // 25% price sanity check

        public async Task<(bool Success, string Message)> ValidateBuyOrderAsync(
            Account account,
            EquityPortfolio portfolio,
            string symbol,
            decimal quantity,
            decimal limitPrice)
        {
            if (quantity <= 0)
                return (false, "Quantity must be positive.");

            if (limitPrice <= 0)
                return (false, "Price must be positive.");

            var requiredCash = quantity * limitPrice;
            if (account.AvailableCash.Amount < requiredCash)
                return (false, $"Insufficient cash. Required: ${requiredCash:F2}, Available: ${account.AvailableCash.Amount:F2}");

            await Task.CompletedTask;
            return (true, "Validation passed.");
        }

        public async Task<(bool Success, string Message)> ValidateSellOrderAsync(
            EquityPortfolio portfolio,
            string symbol,
            decimal quantity)
        {
            var position = portfolio.GetPosition(symbol);
            if (position == null)
                return (false, $"No position in {symbol} to sell.");

            if (position.Quantity < quantity)
                return (false, $"Cannot sell {quantity}. Available: {position.Quantity}");

            await Task.CompletedTask;
            return (true, "Validation passed.");
        }

        public async Task<decimal> CalculateRequiredMarginAsync(string symbol, decimal quantity, decimal price)
        {
            var notional = quantity * price;
            var margin = notional / MarginMultiplier;
            await Task.CompletedTask;
            return margin;
        }

        public async Task<decimal> CalculateBuyingPowerAsync(Account account, EquityPortfolio portfolio)
        {
            var marginRequired = portfolio.Positions.Sum(p => p.MarketValue / MarginMultiplier);
            var availableMargin = (account.AvailableCash.Amount * MarginMultiplier) - marginRequired;
            await Task.CompletedTask;
            return availableMargin > 0 ? availableMargin : account.AvailableCash.Amount;
        }
    }

    public class PositionService : IPositionService
    {
        public async Task<EquityPosition> GetPositionAsync(EquityPortfolio portfolio, string symbol)
        {
            await Task.CompletedTask;
            return portfolio.GetPosition(symbol);
        }

        public async Task<List<EquityPosition>> GetAllPositionsAsync(EquityPortfolio portfolio)
        {
            await Task.CompletedTask;
            return portfolio.Positions.ToList();
        }

        public async Task<PortfolioMetrics> CalculatePortfolioMetricsAsync(EquityPortfolio portfolio, Account account)
        {
            var metrics = new PortfolioMetrics
            {
                TotalMarketValue = portfolio.TotalMarketValue,
                TotalCostBasis = portfolio.TotalCostBasis,
                UnrealizedGainLoss = portfolio.TotalUnrealizedGainLoss,
                UnrealizedGainLossPercentage = portfolio.TotalUnrealizedGainLossPercentage,
                PositionCount = portfolio.Positions.Count,
                ConcentrationBySymbol = new Dictionary<string, decimal>(),
                ConcentrationBySector = new Dictionary<string, decimal>()
            };

            foreach (var position in portfolio.Positions)
            {
                var concentration = portfolio.GetPositionConcentration(position.Symbol);
                metrics.ConcentrationBySymbol[position.Symbol] = concentration;
            }

            await Task.CompletedTask;
            return metrics;
        }
    }

    public class RiskManagementService : IRiskManagementService
    {
        private const decimal MaxSinglePositionConcentration = 0.20m; // 20% max
        private const decimal MaxTotalLeverage = 2.0m; // 2x leverage

        public async Task<List<string>> CheckRiskLimitBreachesAsync(
            Account account,
            EquityPortfolio portfolio,
            TradeRequest trade)
        {
            var breaches = new List<string>();

            // Check position concentration after trade
            var tradeValue = trade.Quantity * trade.MarketPrice;
            var portfolioValue = portfolio.TotalMarketValue + tradeValue;
            var newConcentration = portfolioValue > 0 ? tradeValue / portfolioValue : 0m;

            if (newConcentration > MaxSinglePositionConcentration)
                breaches.Add($"Position concentration would exceed {MaxSinglePositionConcentration:P0}.");

            // Check total leverage
            if (trade.Direction == TradeDirection.Buy)
            {
                var totalPortfolioValue = portfolio.TotalMarketValue + tradeValue;
                var totalLeverage = totalPortfolioValue / account.AvailableCash.Amount;
                if (totalLeverage > MaxTotalLeverage)
                    breaches.Add($"Total leverage would exceed {MaxTotalLeverage}x.");
            }

            await Task.CompletedTask;
            return breaches;
        }

        public async Task<(decimal MaxBuyQuantity, decimal MaxSellQuantity)> GetAllowedTradeQuantityAsync(
            Account account,
            EquityPortfolio portfolio,
            string symbol,
            decimal price)
        {
            var position = portfolio.GetPosition(symbol);
            var maxSellQuantity = position?.Quantity ?? 0m;

            var availableCash = account.AvailableCash.Amount;
            var maxBuyQuantity = price > 0 ? availableCash / price : 0m;

            // Respect concentration limits
            var maxConcentrationValue = portfolio.TotalMarketValue * MaxSinglePositionConcentration;
            var maxBuyByConcentration = price > 0 ? maxConcentrationValue / price : 0m;
            maxBuyQuantity = Math.Min(maxBuyQuantity, maxBuyByConcentration);

            await Task.CompletedTask;
            return (maxBuyQuantity, maxSellQuantity);
        }
    }
}
