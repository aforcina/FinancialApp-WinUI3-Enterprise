# MVVM Implementation Guide

## Core Principles

The MVVM pattern cleanly separates UI concerns from business logic. This guide covers implementation for trading applications.

### ViewModel Responsibilities

1. **State Management**
   - UI state (busy, selection, validation)
   - Business state (selected portfolio, filters, trade form)
   - Derived properties (notional value, margin requirement, available cash)

2. **User Interaction**
   - Commands for user actions (buy, sell, approve, reject)
   - Validation of user inputs
   - Feedback through status messages and error lists

3. **Service Orchestration**
   - Calling application services
   - Handling async operations
   - Error handling and user notification

### Command Pattern

Use `ICommand` for all user actions. This provides:
- Proper async/await support
- CanExecute gating for role-based access
- Consistent error handling

```csharp
public class RelayCommand : ICommand
{
    private readonly Func<object, Task> _executeAsync;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute ?? (_ => true);
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => _canExecute(parameter);

    public void Execute(object parameter)
    {
        _ = ExecuteAsync(parameter);
    }

    public async Task ExecuteAsync(object parameter)
    {
        if (!CanExecute(parameter)) return;
        await _executeAsync(parameter);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

### Data Binding

- **One-way binding** for read-only displays (portfolio summary, market data)
- **Two-way binding** for editable forms (trade entry)
- **ObservableCollection** for lists (positions, pending trades)
- **INotifyPropertyChanged** for individual property updates

### Validation

Validation should occur at multiple levels:

1. **Input validation** - In the ViewModel, as user types
2. **Business logic validation** - In the Application Service
3. **Domain validation** - In the Domain Model

```csharp
private async Task SubmitTradeAsync()
{
    ValidationErrors.Clear();
    
    // Quick input validation
    if (Quantity <= 0)
        ValidationErrors.Add("Quantity must be positive");
    
    if (ValidationErrors.Count > 0)
        return;
    
    // Service-level validation
    var (isValid, errors) = await _validationService.ValidateTradeAsync(...);
    
    if (!isValid)
    {
        foreach (var error in errors)
            ValidationErrors.Add(error);
        return;
    }
    
    // Execute
    await _approvalService.RequestApprovalAsync(...);
}
```

## Trading-Specific Patterns

### Equity Trade Entry

The trade entry ViewModel should:
- Display current market price and bid/ask spread
- Calculate notional value in real-time
- Show margin impact and buying power
- Validate price reasonableness
- Gate submission on validation

### Position Monitoring

The position ViewModel should:
- Display holdings with real-time P&L
- Show position risks (concentration, leverage)
- Allow quick order entry for selected position
- Refresh market prices on demand

### Approval Workflows

The approval ViewModel should:
- Display pending trades in workflow queue
- Show audit trail and rejection reasons
- Gate approval/reject commands on user role
- Log all actions for compliance

## Error Handling

All ViewModels should handle errors consistently:

```csharp
private async Task SubmitTradeAsync()
{
    IsBusy = true;
    try
    {
        // Do work
    }
    catch (ValidationException ex)
    {
        StatusMessage = "Validation failed.";
        foreach (var error in ex.Errors)
            ValidationErrors.Add(error);
    }
    catch (InsufficientFundsException)
    {
        StatusMessage = "Insufficient cash for this trade.";
    }
    catch (Exception ex)
    {
        StatusMessage = $"Error: {ex.Message}";
    }
    finally
    {
        IsBusy = false;
    }
}
```

## Testing

ViewModels must be testable:
- No WinUI3 dependencies
- All services injected
- State easily inspectable
- Commands easily invoked

```csharp
[Fact]
public async Task SubmitTrade_WithValidInput_RequestsApproval()
{
    var mockService = new Mock<IApprovalWorkflowService>();
    var vm = new TradeEntryViewModel(mockService.Object, ...);
    
    vm.Symbol = "AAPL";
    vm.Quantity = 100;
    
    await vm.SubmitTradeCommand.ExecuteAsync(null);
    
    mockService.Verify(s => s.RequestApprovalAsync(It.IsAny<TradeRequest>()), Times.Once);
}
```
