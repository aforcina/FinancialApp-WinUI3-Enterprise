using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancialApp.Core.Models
{
    public class Security
    {
        public string Symbol { get; private set; }
        public string Name { get; private set; }
        public string Sector { get; private set; }
        public decimal MarketCap { get; private set; }
        public bool IsActive { get; private set; }

        public Security(string symbol, string name, string sector, decimal marketCap)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol required.", nameof(symbol));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            if (marketCap < 0) throw new ArgumentOutOfRangeException(nameof(marketCap), "Market cap cannot be negative.");

            Symbol = symbol.Trim().ToUpperInvariant();
            Name = name.Trim();
            Sector = sector?.Trim().ToUpperInvariant() ?? "UNKNOWN";
            MarketCap = marketCap;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
    }

    public class EquityPosition
    {
        public string Symbol { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal AverageCost { get; private set; }
        public decimal CurrentPrice { get; private set; }
        public DateTime LastPriceUpdate { get; private set; }

        public EquityPosition(string symbol, decimal quantity, decimal averageCost, decimal currentPrice)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol required.", nameof(symbol));
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
            if (averageCost <= 0) throw new ArgumentOutOfRangeException(nameof(averageCost), "Average cost must be positive.");
            if (currentPrice <= 0) throw new ArgumentOutOfRangeException(nameof(currentPrice), "Price must be positive.");

            Symbol = symbol.Trim().ToUpperInvariant();
            Quantity = quantity;
            AverageCost = averageCost;
            CurrentPrice = currentPrice;
            LastPriceUpdate = DateTime.UtcNow;
        }

        public decimal MarketValue => Quantity * CurrentPrice;
        public decimal CostBasis => Quantity * AverageCost;
        public decimal UnrealizedGainLoss => MarketValue - CostBasis;
        public decimal UnrealizedGainLossPercentage => CostBasis > 0 ? (UnrealizedGainLoss / CostBasis) * 100m : 0m;

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0) throw new ArgumentOutOfRangeException(nameof(newPrice), "Price must be positive.");
            CurrentPrice = newPrice;
            LastPriceUpdate = DateTime.UtcNow;
        }

        public void UpdateQuantity(decimal newQuantity)
        {
            if (newQuantity < 0) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot be negative.");
            Quantity = newQuantity;
        }

        public void UpdateAveragePrice(decimal newAverage)
        {
            if (newAverage <= 0) throw new ArgumentOutOfRangeException(nameof(newAverage), "Average price must be positive.");
            AverageCost = newAverage;
        }
    }

    public class EquityPortfolio
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid AccountId { get; private set; }
        public List<EquityPosition> Positions { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public EquityPortfolio(Guid accountId, string name)
        {
            if (accountId == Guid.Empty) throw new ArgumentException("Account ID required.", nameof(accountId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));

            Id = Guid.NewGuid();
            AccountId = accountId;
            Name = name;
            Positions = new List<EquityPosition>();
            LastUpdated = DateTime.UtcNow;
        }

        public decimal TotalMarketValue => Positions.Sum(p => p.MarketValue);
        public decimal TotalCostBasis => Positions.Sum(p => p.CostBasis);
        public decimal TotalUnrealizedGainLoss => TotalMarketValue - TotalCostBasis;
        public decimal TotalUnrealizedGainLossPercentage => TotalCostBasis > 0 ? (TotalUnrealizedGainLoss / TotalCostBasis) * 100m : 0m;

        public EquityPosition GetPosition(string symbol)
        {
            return Positions.FirstOrDefault(p => p.Symbol == symbol.Trim().ToUpperInvariant());
        }

        public void AddOrUpdatePosition(string symbol, decimal quantity, decimal averageCost, decimal currentPrice)
        {
            var existing = GetPosition(symbol);

            if (existing == null)
            {
                if (quantity <= 0)
                    throw new InvalidOperationException("Cannot create position with non-positive quantity.");
                Positions.Add(new EquityPosition(symbol, quantity, averageCost, currentPrice));
            }
            else
            {
                var newQuantity = existing.Quantity + quantity;
                if (newQuantity < 0)
                    throw new InvalidOperationException("Position cannot go negative.");

                if (newQuantity == 0)
                {
                    Positions.Remove(existing);
                }
                else
                {
                    var newAverageCost = quantity > 0
                        ? ((existing.Quantity * existing.AverageCost) + (quantity * averageCost)) / newQuantity
                        : existing.AverageCost;

                    existing.UpdateQuantity(newQuantity);
                    existing.UpdateAveragePrice(newAverageCost);
                    existing.UpdatePrice(currentPrice);
                }
            }

            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateAllPrices(Dictionary<string, decimal> priceMap)
        {
            foreach (var position in Positions)
            {
                if (priceMap.TryGetValue(position.Symbol, out var newPrice))
                    position.UpdatePrice(newPrice);
            }
            LastUpdated = DateTime.UtcNow;
        }

        public decimal GetPositionConcentration(string symbol)
        {
            var position = GetPosition(symbol);
            if (position == null || TotalMarketValue == 0)
                return 0m;
            return (position.MarketValue / TotalMarketValue) * 100m;
        }
    }
}
