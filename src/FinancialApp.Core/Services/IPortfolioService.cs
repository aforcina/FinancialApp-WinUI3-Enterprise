using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    /// <summary>
    /// Service interface for portfolio management.
    /// </summary>
    public interface IPortfolioService
    {
        Task<Portfolio> GetPortfolioAsync(string accountId);
        Task UpdatePortfolioAsync(Portfolio portfolio);
        decimal CalculateTotalValue(Portfolio portfolio);
        decimal CalculateRiskMetrics(Portfolio portfolio);
    }
}
