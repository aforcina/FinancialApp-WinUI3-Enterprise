using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public class SimulatedOrderService : IOrderService
    {
        private readonly Dictionary<string, ObservableCollection<Order>> _orders = new Dictionary<string, ObservableCollection<Order>>();
        private readonly Random _random = new Random();

        public SimulatedOrderService() => InitializeOrders();

        private void InitializeOrders()
        {
            _orders["ACC-001"] = new ObservableCollection<Order>
            {
                new Order { Symbol = "AAPL", Side = OrderSide.Buy, Quantity = 100, OrderType = OrderType.Limit, LimitPrice = 210.50m, Status = OrderStatus.Executed, ApprovalStatus = ApprovalStatus.Approved, ExecutedPrice = 210.40m, ExecutedQuantity = 100, SubmittedBy = "John Smith", ApprovedBy = "Manager A", ExecutedAt = DateTime.UtcNow.AddHours(-2), Notes = "Standard purchase order" },
                new Order { Symbol = "MSFT", Side = OrderSide.Sell, Quantity = 50, OrderType = OrderType.Limit, LimitPrice = 421.00m, Status = OrderStatus.Submitted, ApprovalStatus = ApprovalStatus.Pending, SubmittedBy = "Jane Doe", Notes = "Pending approval" },
                new Order { Symbol = "GOOGL", Side = OrderSide.Buy, Quantity = 50, OrderType = OrderType.Market, Status = OrderStatus.Draft, ApprovalStatus = ApprovalStatus.Pending, SubmittedBy = "John Smith", Notes = "Draft order" }
            };
        }

        public Task<Order> CreateOrderAsync(Order order)
        {
            order.Status = OrderStatus.Draft;
            order.ApprovalStatus = ApprovalStatus.Pending;
            return Task.FromResult(order);
        }

        public Task<Order> SubmitOrderAsync(Order order)
        {
            order.Status = OrderStatus.Submitted;
            order.CreatedAt = DateTime.UtcNow;
            return Task.FromResult(order);
        }

        public Task<Order> ApproveOrderAsync(Order order, string approvedBy)
        {
            order.Status = OrderStatus.Approved;
            order.ApprovalStatus = ApprovalStatus.Approved;
            order.ApprovedBy = approvedBy;
            return Task.FromResult(order);
        }

        public Task<Order> RejectOrderAsync(Order order, string reason)
        {
            order.Status = OrderStatus.Rejected;
            order.ApprovalStatus = ApprovalStatus.Rejected;
            order.Notes = reason;
            return Task.FromResult(order);
        }

        public Task<Order> ExecuteOrderAsync(Order order)
        {
            order.Status = OrderStatus.Executed;
            order.ExecutedQuantity = order.Quantity;
            order.ExecutedPrice = order.LimitPrice ?? (decimal)(_random.NextDouble() * 100 + 50);
            order.ExecutedAt = DateTime.UtcNow;
            return Task.FromResult(order);
        }

        public Task<ObservableCollection<Order>> GetOrdersAsync(string accountId) =>
            Task.FromResult(_orders.TryGetValue(accountId, out var orders) ? orders : new ObservableCollection<Order>());

        public Task<Order> GetOrderAsync(string orderId)
        {
            var order = _orders.Values.SelectMany(items => items).FirstOrDefault(item => item.OrderId == orderId);
            return Task.FromResult(order);
        }
    }
}
