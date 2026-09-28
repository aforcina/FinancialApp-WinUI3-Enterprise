using System;

namespace FinancialApp.Core.Models
{
    public enum OrderSide { Buy, Sell }
    public enum OrderType { Market, Limit, Stop }
    public enum OrderStatus { Draft, Submitted, Approved, Rejected, Executed, Cancelled }
    public enum ApprovalStatus { Pending, Approved, Rejected, OnHold }

    public class Order
    {
        public string OrderId { get; set; }
        public string Symbol { get; set; }
        public OrderSide Side { get; set; }
        public int Quantity { get; set; }
        public decimal? LimitPrice { get; set; }
        public decimal? StopPrice { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus Status { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public decimal ExecutedPrice { get; set; }
        public int ExecutedQuantity { get; set; }
        public string SubmittedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public string Notes { get; set; }

        public Order()
        {
            OrderId = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Draft;
            ApprovalStatus = ApprovalStatus.Pending;
        }
    }
}
