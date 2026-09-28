using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancialApp.Core.Models
{
    public class Position
    {
        public string Symbol { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal AveragePrice { get; private set; }

        public Position(string symbol, decimal quantity, decimal averagePrice)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol is required.", nameof(symbol));
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
            if (averagePrice <= 0) throw new ArgumentOutOfRangeException(nameof(averagePrice), "Average price must be greater than zero.");

            Symbol = symbol.Trim().ToUpperInvariant();
            Quantity = quantity;
            AveragePrice = averagePrice;
        }

        public void Update(decimal quantityDelta, decimal newAveragePrice)
        {
            Quantity += quantityDelta;
            AveragePrice = newAveragePrice;
        }
    }

    public class Portfolio
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid AccountId { get; private set; }
        public List<Position> Positions { get; private set; }

        public Portfolio(Guid accountId, string name)
        {
            if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Portfolio name is required.", nameof(name));

            Id = Guid.NewGuid();
            AccountId = accountId;
            Name = name;
            Positions = new List<Position>();
        }

        public void AddOrUpdatePosition(string symbol, decimal quantityDelta, decimal averagePrice)
        {
            var existing = Positions.FirstOrDefault(p => p.Symbol == symbol.Trim().ToUpperInvariant());

            if (existing == null)
            {
                if (quantityDelta <= 0)
                    throw new InvalidOperationException("Cannot create a position with a non-positive quantity.");

                Positions.Add(new Position(symbol, quantityDelta, averagePrice));
                return;
            }

            var updatedQuantity = existing.Quantity + quantityDelta;
            if (updatedQuantity < 0)
                throw new InvalidOperationException("Position cannot go negative.");

            if (updatedQuantity == 0)
            {
                Positions.Remove(existing);
                return;
            }

            existing.Update(quantityDelta, averagePrice);
        }

        public decimal TotalPositionQuantity(string symbol)
        {
            var position = Positions.FirstOrDefault(p => p.Symbol == symbol.Trim().ToUpperInvariant());
            return position?.Quantity ?? 0m;
        }
    }
}
