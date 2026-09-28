using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Core.Interfaces;
using FinancialApp.Core.Models;

namespace FinancialApp.Application.Services
{
    public class TradeValidationService : ITradeValidationService
    {
        public async Task<(bool IsValid, List<string> Errors)> ValidateTradeAsync(
            TradeRequest trade,
            Account account,
            Portfolio portfolio,
            RiskLimit riskLimit)
        {
            var errors = new List<string>();

            // Validate account currency match
            if (!string.Equals(trade.Currency, account.Currency, StringComparison.OrdinalIgnoreCase))
                errors.Add("Trade currency does not match account currency.");

            // Validate buy trades have sufficient cash
            if (trade.Direction == TradeDirection.Buy)
            {
                if (account.AvailableCash.Amount < trade.NotionalValue.Amount)
                    errors.Add($"Insufficient cash. Required: {trade.NotionalValue:C}, Available: {account.AvailableCash:C}");
            }

            // Validate sell trades have sufficient position
            if (trade.Direction == TradeDirection.Sell)
            {
                var currentQuantity = portfolio.TotalPositionQuantity(trade.Symbol);
                if (currentQuantity < trade.Quantity)
                    errors.Add($"Insufficient position. Trying to sell {trade.Quantity}, available {currentQuantity}");
            }

            // Validate risk limits
            if (!riskLimit.ValidateTradeSize(trade.NotionalValue))
                errors.Add($"Trade size exceeds limit of {riskLimit.MaxTradeSize:C}.");

            if (!riskLimit.IsSymbolAllowed(trade.Symbol))
                errors.Add($"Symbol {trade.Symbol} is not allowed under current risk limits.");

            // Validate price sanity (price not too far from market)
            if (trade.LimitPrice > 0 && trade.MarketPrice > 0)
            {
                var deviation = Math.Abs(trade.LimitPrice - trade.MarketPrice) / trade.MarketPrice;
                if (deviation > 0.25m) // 25% deviation threshold
                    errors.Add($"Limit price deviates significantly from market price (deviation: {deviation:P2}).");
            }

            await Task.CompletedTask;
            return (errors.Count == 0, errors);
        }
    }

    public class ApprovalWorkflowService : IApprovalWorkflowService
    {
        public async Task<bool> CanApproveAsync(string userId, ApprovalLevel requiredLevel)
        {
            // This would typically check the user's role and approval authority
            // For now, returning true for demonstration
            await Task.CompletedTask;
            return true;
        }

        public async Task RequestApprovalAsync(TradeRequest trade, ApprovalLevel level)
        {
            trade.RequestApproval(level);
            await Task.CompletedTask;
        }

        public async Task ApproveTradeAsync(TradeRequest trade, string userId)
        {
            trade.Approve(userId);
            await Task.CompletedTask;
        }

        public async Task RejectTradeAsync(TradeRequest trade, string userId, string reason)
        {
            trade.Reject(userId, reason);
            await Task.CompletedTask;
        }
    }

    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IAuditService _auditService;

        public TradeExecutionService(IPortfolioService portfolioService, IAuditService auditService)
        {
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<bool> ExecuteTradeAsync(
            TradeRequest tradeRequest,
            Account account,
            Portfolio portfolio,
            decimal executionPrice)
        {
            try
            {
                var trade = new Trade(
                    account.Id,
                    tradeRequest.Symbol,
                    tradeRequest.Quantity,
                    executionPrice,
                    account.Currency,
                    tradeRequest.Direction,
                    DateTime.UtcNow);

                trade.Approve();
                _portfolioService.ExecuteTrade(portfolio, account, trade, executionPrice);
                tradeRequest.Execute(executionPrice);

                await _auditService.LogActionAsync(
                    tradeRequest.Id,
                    "TradeRequest",
                    "Executed",
                    tradeRequest.CreatedByUserId,
                    $"Trade executed: {tradeRequest.Symbol} {tradeRequest.Quantity}@{executionPrice}");

                return true;
            }
            catch (Exception ex)
            {
                tradeRequest.MarkFailed(ex.Message);
                await _auditService.LogActionAsync(
                    tradeRequest.Id,
                    "TradeRequest",
                    "Failed",
                    tradeRequest.CreatedByUserId,
                    $"Trade execution failed: {ex.Message}");
                return false;
            }
        }
    }
}
