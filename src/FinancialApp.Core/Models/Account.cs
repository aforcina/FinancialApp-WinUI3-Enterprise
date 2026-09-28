using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancialApp.Core.Models
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string AccountNumber { get; private set; }
        public string Name { get; private set; }
        public string Currency { get; private set; }
        public Money AvailableCash { get; private set; }

        public Account(string accountNumber, string name, string currency, decimal initialCash = 0m)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) throw new ArgumentException("Account number is required.", nameof(accountNumber));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

            Id = Guid.NewGuid();
            AccountNumber = accountNumber;
            Name = name;
            Currency = currency.Trim().ToUpperInvariant();
            AvailableCash = new Money(initialCash, Currency);
        }

        public void Deposit(Money amount)
        {
            EnsureSameCurrency(amount);
            AvailableCash = AvailableCash.Add(amount);
        }

        public void Withdraw(Money amount)
        {
            EnsureSameCurrency(amount);

            if (AvailableCash.Amount < amount.Amount)
                throw new InvalidOperationException("Insufficient funds for withdrawal.");

            AvailableCash = AvailableCash.Subtract(amount);
        }

        private void EnsureSameCurrency(Money amount)
        {
            if (!string.Equals(amount.Currency, Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Currency mismatch in account transaction.");
        }
    }
}
