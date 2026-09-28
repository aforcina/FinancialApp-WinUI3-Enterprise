using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Interfaces
{
    public interface ITradeValidationService
    {
        Task<(bool IsValid, List<string> Errors)> ValidateTradeAsync(TradeRequest trade, Account account, Portfolio portfolio, RiskLimit riskLimit);
    }

    public interface IAuditService
    {
        Task LogActionAsync(Guid entityId, string entityType, string action, string userId, string description, string ipAddress = null);
        Task<List<AuditLog>> GetEntityAuditHistoryAsync(Guid entityId);
    }

    public interface ITradeExecutionService
    {
        Task<bool> ExecuteTradeAsync(TradeRequest tradeRequest, Account account, Portfolio portfolio, decimal executionPrice);
    }

    public interface IMarketDataService
    {
        Task<decimal> GetCurrentPriceAsync(string symbol);
        Task<Dictionary<string, decimal>> GetMultiplePricesAsync(params string[] symbols);
        decimal GetLastKnownPrice(string symbol);
    }

    public interface IApprovalWorkflowService
    {
        Task<bool> CanApproveAsync(string userId, ApprovalLevel requiredLevel);
        Task RequestApprovalAsync(TradeRequest trade, ApprovalLevel level);
        Task ApproveTradeAsync(TradeRequest trade, string userId);
        Task RejectTradeAsync(TradeRequest trade, string userId, string reason);
    }
}
