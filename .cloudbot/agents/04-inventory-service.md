# Title
Inventory service

# Source user story file path
backlog/04-feat-inventory-service.md

# Story summary
Add the first real inventory worker that consumes submitted orders, validates stock availability, persists the inventory decision, and publishes an inventory outcome message.

# In-scope items
- Add an inventory worker project
- Consume `OrderSubmitted`
- Validate requested quantities against simulated stock
- Persist inventory result in `InventoryRecord`
- Publish `InventoryConfirmed` or `InventoryFailed`
- Add structured logging and unit tests for decision logic

# Out-of-scope items
- Full reservation store backed by separate infrastructure
- Retry/dead-letter/outbox behavior
- Payment or shipping consumers

# Acceptance criteria
- `OrderSubmitted` is consumed
- stock validation occurs
- `InventoryRecord` is generated/persisted
- `InventoryConfirmed` or `InventoryFailed` is published

# Technical implementation plan
1. Add inventory outcome contracts and publisher abstraction support.
2. Create worker project with RabbitMQ consumer background service.
3. Add inventory decision service and simulated stock policy.
4. Update persistence through `StoreDbContext`/orders.
5. Publish inventory outcome event after persistence.
6. Add tests and run build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Infrastructure/**
- SportsStore.Inventory.Worker/**
- SportsStore.Tests/**
- SportsSln.sln
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: implement inventory service worker

# PR plan
- PR from feat/04-inventory-service into master with consumption and publishing summary

# Risks / assumptions
- Stock validation is simulated with a deterministic in-memory policy to satisfy the assignment step without introducing a full separate inventory database yet.
