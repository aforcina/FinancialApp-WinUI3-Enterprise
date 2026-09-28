# Architecture Blueprint

## High-level design

For a financial enterprise application, the architecture should strongly separate business logic from UI logic and infrastructure concerns.

### Recommended layers

1. Presentation Layer
   - WinUI3 views
   - ViewModels
   - Commands and data binding
   - Validation feedback

2. Application Layer
   - Use cases and orchestration
   - Business workflow execution
   - Transaction coordination
   - DTO mapping

3. Domain Layer
   - Aggregates and entities
   - Value objects
   - Business rules and invariants

4. Infrastructure Layer
   - Database access
   - REST/SDK integrations
   - File storage
   - Security and audit log services

## Why this matters in finance

Financial applications require:

- precise money handling (`decimal`, not `double`)
- strong validation for trade eligibility and approval
- auditable workflows and immutable logs
- performance for portfolio or market data screens
- role-based access and compliance controls

## Example responsibilities

- `PortfolioService` manages portfolio changes and validation
- `ViewModel` handles UI state, selection, search, and command execution
- `Repository` persists domain state to database or APIs
- `SecurityService` retrieves real-time market data or pricing providers

## Architecture principles

- Keep `ViewModel` free of database logic
- Keep business rules in the domain layer
- Use dependency injection consistently
- Prefer explicit, testable interfaces
- Use async patterns for long-running operations and I/O
