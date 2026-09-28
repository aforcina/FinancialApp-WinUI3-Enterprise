using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FinancialApp.Core.Models;
using FinancialApp.Core.Services;
using Microsoft.UI.Dispatching;

namespace FinancialApp.Presentation.ViewModels
{
    public sealed class WatchlistViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IMarketDataService _marketDataService;
        private readonly DispatcherQueue _dispatcherQueue;

        public WatchlistViewModel(IMarketDataService marketDataService, DispatcherQueue dispatcherQueue)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
            _marketDataService.MarketDataUpdated += OnMarketDataUpdated;
            _ = LoadAsync();
        }

        public ObservableCollection<MarketData> Quotes { get; } = new ObservableCollection<MarketData>();

        private string _statusMessage = "Loading market data...";
        public string StatusMessage
        {
            get => _statusMessage;
            private set { _statusMessage = value; OnPropertyChanged(); }
        }

        private async Task LoadAsync()
        {
            var quotes = await _marketDataService.GetWatchlistAsync();
            _dispatcherQueue.TryEnqueue(() => ReplaceQuotes(quotes));
        }

        private void OnMarketDataUpdated(object sender, MarketDataUpdatedEventArgs e)
        {
            _dispatcherQueue.TryEnqueue(() => ReplaceQuotes(e.Quotes));
        }

        private void ReplaceQuotes(ObservableCollection<MarketData> quotes)
        {
            Quotes.Clear();
            foreach (var quote in quotes)
                Quotes.Add(quote);
            StatusMessage = $"Live quotes · {Quotes.Count} symbols · Updated {DateTime.Now:HH:mm:ss}";
        }

        public void Dispose() => _marketDataService.MarketDataUpdated -= OnMarketDataUpdated;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
