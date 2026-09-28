using System;
using System.Collections.Generic;
using System.Linq;
using FinancialApp.Core.Models;

namespace FinancialApp.Application.Services
{
    public class PortfolioSummary
    {
        public decimal TotalMarketValue { get; set; }
        public decimal TotalCash { get; set; }
        public int PositionCount { get; set; }
    }

    public interface IPortfolioService
    {
        void ExecuteTrade(Portfolio portfolio, Account account, Trade trade, decimal currentPrice);
        PortfolioSummary GetSummary(Portfolio portfolio, Account account, decimal currentPricePerPosition);
    }

    public class PortfolioService : IPortfolioService
    {
        public void ExecuteTrade(Portfolio portfolio, Account account, Trade trade, decimal currentPrice)
        {
            if (portfolio == null) throw new ArgumentNullException(nameof(portfolio));
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (trade == null) throw new ArgumentNullException(nameof(trade));

            if (trade.AccountId != account.Id)
                throw new InvalidOperationException("Trade does not belong to the selected account.");

            if (!trade.IsApproved)
                throw new InvalidOperationException("Trade must be approved before execution.");

            if (string.Equals(trade.Currency, account.Currency, StringComparison.OrdinalIgnoreCase) == false)
                throw new InvalidOperationException("Trade and account currencies must match.");

            var notional = trade.NotionalValue;
            if (trade.Direction == TradeDirection.Buy && account.AvailableCash.Amount < notional.Amount)
                throw new InvalidOperationException("Account does not have sufficient cash for this trade.");

            if (trade.Direction == TradeDirection.Sell)
            {
                var currentQuantity = portfolio.TotalPositionQuantity(trade.Symbol);
                if (currentQuantity < trade.Quantity)
                    throw new InvalidOperationException("Cannot sell more than the available quantity.");
            }

            if (trade.Direction == TradeDirection.Buy)
            {
                account.Withdraw(notional);
                portfolio.AddOrUpdatePosition(trade.Symbol, trade.Quantity, currentPrice);
            }
            else
            {
                account.Deposit(notional);
                portfolio.AddOrUpdatePosition(trade.Symbol, -trade.Quantity, currentPrice);
            }
        }

        public PortfolioSummary GetSummary(Portfolio portfolio, Account account, decimal currentPricePerPosition)
        {
            var totalMarketValue = portfolio.Positions.Sum(p => p.Quantity * currentPricePerPosition);

            return new PortfolioSummary
            {
                TotalCash = account.AvailableCash.Amount,
                TotalMarketValue = totalMarketValue,
                PositionCount = portfolio.Positions.Count
            };
        }
    }
}
