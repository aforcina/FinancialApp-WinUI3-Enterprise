namespace FinancialApp.Core.Models
{
    public class MarketData
    {
        public string Symbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public long Volume { get; set; }
        public string CompanyName { get; set; }
        public string Sector { get; set; }
        public decimal DayHigh { get; set; }
        public decimal DayLow { get; set; }
        public long MarketCap { get; set; }
        public DateTime LastUpdate { get; set; }

        public MarketData()
        {
            LastUpdate = DateTime.UtcNow;
        }
    }
}
