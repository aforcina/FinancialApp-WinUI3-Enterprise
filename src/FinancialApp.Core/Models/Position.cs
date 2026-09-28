namespace FinancialApp.Core.Models
{
    public class Position
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketValue => Quantity * CurrentPrice;
        public decimal UnrealizedPnL => MarketValue - (Quantity * AverageCost);
        public decimal UnrealizedPnLPercent => AverageCost > 0 ? (UnrealizedPnL / (Quantity * AverageCost)) * 100 : 0;
        public decimal ConcentrationPercent { get; set; }
        public string Sector { get; set; }
        public string CompanyName { get; set; }
    }
}
