using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    /// <summary>
    /// Service interface for order management and execution.
    /// </summary>
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> SubmitOrderAsync(Order order);
        Task<Order> ApproveOrderAsync(Order order, string approvedBy);
        Task<Order> RejectOrderAsync(Order order, string reason);
        Task<Order> ExecuteOrderAsync(Order order);
        Task<ObservableCollection<Order>> GetOrdersAsync(string accountId);
        Task<Order> GetOrderAsync(string orderId);
    }
}
