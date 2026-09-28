using System;

namespace FinancialApp.Core.Models
{
    public readonly struct Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            Amount = amount;
            Currency = currency.Trim().ToUpperInvariant();
        }

        public static Money Zero(string currency = "USD") => new Money(0m, currency);

        public Money Add(Money other)
        {
            ValidateSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            ValidateSameCurrency(other);
            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

        public Money Divide(decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException();

            return new Money(Amount / divisor, Currency);
        }

        private void ValidateSameCurrency(Money other)
        {
            if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot operate on amounts with different currencies.");
        }

        public override string ToString() => $"{Amount:N2} {Currency}";

        public bool Equals(Money other) => Amount == other.Amount && string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object obj) => obj is Money money && Equals(money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);

        public static bool operator ==(Money left, Money right) => left.Equals(right);
        public static bool operator !=(Money left, Money right) => !(left == right);
        public static Money operator +(Money left, Money right) => left.Add(right);
        public static Money operator -(Money left, Money right) => left.Subtract(right);
        public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
        public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);
    }
}
