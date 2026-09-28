namespace FinancialApp.Core.Models
{
    public class Account
    {
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }

        public Account(string accountId, string name, string currency, decimal balance)
        {
            AccountId = accountId;
            Name = name;
            Currency = currency;
            Balance = balance;
        }
    }
}
