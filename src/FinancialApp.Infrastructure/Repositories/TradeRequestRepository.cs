using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Infrastructure.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
    }

    public class TradeRequestRepository : IRepository<TradeRequest>
    {
        private readonly List<TradeRequest> _trades = new List<TradeRequest>();

        public async Task<TradeRequest> GetByIdAsync(Guid id)
        {
            var trade = _trades.Find(t => t.Id == id);
            await Task.CompletedTask;
            return trade;
        }

        public async Task<List<TradeRequest>> GetAllAsync()
        {
            await Task.CompletedTask;
            return new List<TradeRequest>(_trades);
        }

        public async Task<TradeRequest> AddAsync(TradeRequest entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _trades.Add(entity);
            await Task.CompletedTask;
            return entity;
        }

        public async Task<TradeRequest> UpdateAsync(TradeRequest entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var existing = await GetByIdAsync(entity.Id);
            if (existing != null)
            {
                _trades.Remove(existing);
                _trades.Add(entity);
            }
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var trade = await GetByIdAsync(id);
            if (trade != null)
                _trades.Remove(trade);
        }
    }
}
