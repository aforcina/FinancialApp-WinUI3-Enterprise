using System;
using System.Collections.Generic;

namespace FinancialApp.Core.Models
{
    public class RiskLimit
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public string Name { get; private set; }
        public Money MaxTradeSize { get; private set; }
        public Money MaxDailyExposure { get; private set; }
        public decimal MaxSinglePositionQuantity { get; private set; }
        public List<string> AllowedSymbols { get; private set; }
        public bool IsActive { get; private set; }

        public RiskLimit(
            Guid accountId,
            string name,
            Money maxTradeSize,
            Money maxDailyExposure,
            decimal maxSinglePositionQuantity,
            List<string> allowedSymbols = null)
        {
            if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

            Id = Guid.NewGuid();
            AccountId = accountId;
            Name = name;
            MaxTradeSize = maxTradeSize;
            MaxDailyExposure = maxDailyExposure;
            MaxSinglePositionQuantity = maxSinglePositionQuantity;
            AllowedSymbols = allowedSymbols ?? new List<string>();
            IsActive = true;
        }

        public bool ValidateTradeSize(Money tradeNotional)
        {
            return tradeNotional.Amount <= MaxTradeSize.Amount;
        }

        public bool IsSymbolAllowed(string symbol)
        {
            if (AllowedSymbols.Count == 0) return true; // No restrictions
            return AllowedSymbols.Contains(symbol.Trim().ToUpperInvariant());
        }
    }
}
