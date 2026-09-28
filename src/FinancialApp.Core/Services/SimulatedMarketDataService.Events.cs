using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinancialApp.Core.Models;

namespace FinancialApp.Core.Services
{
    public partial class SimulatedMarketDataService
    {
        public event EventHandler<MarketDataUpdatedEventArgs> MarketDataUpdated;

        private void PublishSnapshot()
        {
            ObservableCollection<MarketData> snapshot;
            lock (_marketData)
                snapshot = new ObservableCollection<MarketData>(_marketData.Values.Select(Clone));
            MarketDataUpdated?.Invoke(this, new MarketDataUpdatedEventArgs(snapshot));
        }
    }
}
