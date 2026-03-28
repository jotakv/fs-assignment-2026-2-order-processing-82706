# Title
Order status tracking

# Source user story file path
backlog/07-feat-order-status-tracking.md

# Story summary
Complete order traceability by exposing meaningful tracking information from the API and making downstream workers leave consistent completed/failed state transitions with failure traces.

# In-scope items
- Improve order status API output
- Add failure reason / last event trace
- Add lightweight status timeline from persisted records
- Ensure completed/failed terminal states are applied consistently
- Add tests for tracking projection and transitions

# Out-of-scope items
- Real event replay/history store
- UI work
- Rebuilding worker architecture

# Acceptance criteria
- API updates/exposes order state after each event
- `GET /api/orders/{id}/status` returns real state
- orders can reach `Completed` or `Failed`
- failures leave a visible trace

# Technical implementation plan
1. Extend status DTO with failure/timeline details.
2. Map status query from persisted inventory/payment/shipment records.
3. Tighten worker state transitions for completed/failed terminal states.
4. Add tests for status projection and terminal transitions.
5. Run restore/build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Domain/**
- SportsStore.Infrastructure/**
- SportsStore.Payment.Worker/**
- SportsStore.Shipping.Worker/**
- SportsStore.Tests/**
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: complete order status tracking

# PR plan
- PR from feat/07-order-status-tracking into master with state transition summary and validation results

# Risks / assumptions
- Tracking is derived from persisted records already stored on the order rather than from a separate audit/event-store subsystem.
