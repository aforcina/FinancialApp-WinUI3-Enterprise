using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinancialApp.Core.Interfaces
{
    public interface IMarketDataService
    {
        Task<decimal> GetCurrentPriceAsync(string symbol);
        Task<Dictionary<string, decimal>> GetMultiplePricesAsync(params string[] symbols);
        decimal GetLastKnownPrice(string symbol);
        Task<bool> UpdatePriceAsync(string symbol, decimal price);
    }

    public interface IEquityTradingService
    {
        Task<(bool Success, string Message)> ValidateBuyOrderAsync(Account account, EquityPortfolio portfolio, string symbol, decimal quantity, decimal limitPrice);
        Task<(bool Success, string Message)> ValidateSellOrderAsync(EquityPortfolio portfolio, string symbol, decimal quantity);
        Task<decimal> CalculateRequiredMarginAsync(string symbol, decimal quantity, decimal price);
        Task<decimal> CalculateBuyingPowerAsync(Account account, EquityPortfolio portfolio);
    }

    public interface IPositionService
    {
        Task<EquityPosition> GetPositionAsync(EquityPortfolio portfolio, string symbol);
        Task<List<EquityPosition>> GetAllPositionsAsync(EquityPortfolio portfolio);
        Task<PortfolioMetrics> CalculatePortfolioMetricsAsync(EquityPortfolio portfolio, Account account);
    }

    public interface IRiskManagementService
    {
        Task<List<string>> CheckRiskLimitBreachesAsync(Account account, EquityPortfolio portfolio, TradeRequest trade);
        Task<(decimal MaxBuyQuantity, decimal MaxSellQuantity)> GetAllowedTradeQuantityAsync(Account account, EquityPortfolio portfolio, string symbol, decimal price);
    }
}

public class PortfolioMetrics
{
    public decimal TotalMarketValue { get; set; }
    public decimal TotalCostBasis { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal UnrealizedGainLossPercentage { get; set; }
    public int PositionCount { get; set; }
    public Dictionary<string, decimal> ConcentrationBySymbol { get; set; }
    public Dictionary<string, decimal> ConcentrationBySector { get; set; }
}
