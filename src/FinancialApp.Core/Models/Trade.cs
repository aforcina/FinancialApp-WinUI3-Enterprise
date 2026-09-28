using System;

namespace FinancialApp.Core.Models
{
    public enum TradeDirection
    {
        Buy = 1,
        Sell = 2
    }

    public class Trade
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public string Symbol { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal Price { get; private set; }
        public string Currency { get; private set; }
        public TradeDirection Direction { get; private set; }
        public DateTime TradeDate { get; private set; }
        public bool IsApproved { get; private set; }

        public Trade(Guid accountId, string symbol, decimal quantity, decimal price, string currency, TradeDirection direction, DateTime tradeDate)
        {
            if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol is required.", nameof(symbol));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));

            Id = Guid.NewGuid();
            AccountId = accountId;
            Symbol = symbol.Trim().ToUpperInvariant();
            Quantity = quantity;
            Price = price;
            Currency = currency.Trim().ToUpperInvariant();
            Direction = direction;
            TradeDate = tradeDate;
            IsApproved = false;
        }

        public Money NotionalValue => new Money(Quantity * Price, Currency);

        public void Approve() => IsApproved = true;

        public void Reject() => IsApproved = false;
    }
}
