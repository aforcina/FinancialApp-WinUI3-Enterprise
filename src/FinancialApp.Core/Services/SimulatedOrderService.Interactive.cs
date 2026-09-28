using System;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public partial class SimulatedOrderService
    {
        public Task<Order> SubmitOrderAsync(string accountId, Order order)
        {
            SubmitOrderAsync(order);
            if (!_orders.TryGetValue(accountId, out var orders))
                _orders[accountId] = orders = new System.Collections.ObjectModel.ObservableCollection<Order>();
            if (!orders.Contains(order)) orders.Insert(0, order);
            return Task.FromResult(order);
        }
    }
}
