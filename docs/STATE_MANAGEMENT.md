# State Management Guide

## Application State Architecture

State in a trading application spans multiple levels:

### Global Application State

State that affects the entire application:

```csharp
public class ApplicationState
{
    public User CurrentUser { get; set; }
    public Account SelectedAccount { get; set; }
    public Portfolio SelectedPortfolio { get; set; }
    public Dictionary<string, decimal> CurrentMarketPrices { get; set; }
    public DateTime LastMarketDataRefresh { get; set; }
    public bool IsMarketDataLive { get; set; }
}
```

Manage this via a singleton service:

```csharp
public interface IApplicationStateService
{
    ApplicationState GetCurrentState();
    Task SetCurrentUserAsync(User user);
    Task SetSelectedAccountAsync(Account account);
    Task UpdateMarketPricesAsync(Dictionary<string, decimal> prices);
}

public class ApplicationStateService : IApplicationStateService
{
    private readonly ApplicationState _state = new ApplicationState();
    public event EventHandler<StateChangedEventArgs> StateChanged;

    public ApplicationState GetCurrentState() => _state;

    public async Task SetCurrentUserAsync(User user)
    {
        _state.CurrentUser = user;
        StateChanged?.Invoke(this, new StateChangedEventArgs { PropertyName = nameof(ApplicationState.CurrentUser) });
        await Task.CompletedTask;
    }

    public async Task UpdateMarketPricesAsync(Dictionary<string, decimal> prices)
    {
        _state.CurrentMarketPrices = prices;
        _state.LastMarketDataRefresh = DateTime.UtcNow;
        StateChanged?.Invoke(this, new StateChangedEventArgs { PropertyName = nameof(ApplicationState.CurrentMarketPrices) });
        await Task.CompletedTask;
    }
}
```

### Feature-Level State

State specific to a feature (e.g., trade entry, position monitoring):

```csharp
public class TradeEntryFeatureState
{
    public TradeRequest DraftTrade { get; set; }
    public List<string> ValidationErrors { get; set; }
    public bool IsFormDirty { get; set; }
    public bool CanSubmit { get; set; }
}
```

Manage via feature-specific services:

```csharp
public interface ITradeEntryStateService
{
    TradeEntryFeatureState GetState();
    Task UpdateDraftTradeAsync(TradeRequest trade);
    Task ClearStateAsync();
}
```

### Local UI State

State that only affects a single ViewModel or component:

```csharp
public class TradeEntryViewModel
{
    private bool _isBusy;  // Only affects this screen
    private string _symbol;  // Local form field
    private decimal _quantity;  // Local form field
}
```

## State Persistence

For critical state, persist to disk or database:

```csharp
public class PersistenceService : IApplicationStateService
{
    private readonly IJsonSerializer _serializer;
    private readonly string _stateFilePath = "app-state.json";

    public async Task SaveStateAsync(ApplicationState state)
    {
        var json = _serializer.Serialize(state);
        await File.WriteAllTextAsync(_stateFilePath, json);
    }

    public async Task<ApplicationState> LoadStateAsync()
    {
        if (!File.Exists(_stateFilePath))
            return new ApplicationState();
        
        var json = await File.ReadAllTextAsync(_stateFilePath);
        return _serializer.Deserialize<ApplicationState>(json);
    }
}
```

## State Synchronization

When state changes, notify all interested ViewModels:

```csharp
public class ApplicationStateService : IApplicationStateService
{
    public event EventHandler<StateChangedEventArgs> StateChanged;

    public async Task SetSelectedAccountAsync(Account account)
    {
        _state.SelectedAccount = account;
        StateChanged?.Invoke(this, new StateChangedEventArgs 
        { 
            PropertyName = nameof(ApplicationState.SelectedAccount),
            NewValue = account
        });
        await Task.CompletedTask;
    }
}

public class PortfolioViewModel
{
    private readonly IApplicationStateService _stateService;

    public PortfolioViewModel(IApplicationStateService stateService)
    {
        _stateService = stateService;
        _stateService.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object sender, StateChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApplicationState.SelectedAccount))
            RefreshPortfolio();
    }
}
```

## Equity Trading State Example

For an equity trading system, manage:

1. **Market Data State**
   - Current prices for watched symbols
   - Bid/ask spreads
   - Last update timestamp

2. **Position State**
   - Holdings (symbol, quantity, average cost, market value, P&L)
   - Buying power and margin utilization
   - Position risks and concentration

3. **Order State**
   - Pending/active trades
   - Order execution status
   - Fills and partial fills

4. **Risk State**
   - Daily P&L
   - Exposure by sector/symbol
   - Margin utilization
   - Breach alerts

## Best Practices

1. **Keep state immutable** - Create new objects rather than mutating
2. **Use observables for streaming data** - Market prices, position updates
3. **Cache state strategically** - Avoid redundant service calls
4. **Invalidate state on known events** - User login, account change, market close
5. **Version state for persistence** - Support migration when schema changes
6. **Log state changes for audit** - Critical for compliance
