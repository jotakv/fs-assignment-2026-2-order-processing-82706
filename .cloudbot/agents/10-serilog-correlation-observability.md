# Title
Serilog correlation observability

# Source user story file path
backlog/10-feat-serilog-correlation-observability.md

# Story summary
Improve structured observability across the API and worker services so each order can be traced end-to-end with correlation-aware logs.

# In-scope items
- Complete Serilog setup consistency across API/workers
- Propagate correlation id through request and message flow
- Add structured logs for submit/publish/consume/inventory/payment/shipping/errors
- Include contextual fields: ServiceName, EventType, CorrelationId, OrderId, CustomerId where available

# Out-of-scope items
- Full tracing backend/OpenTelemetry rollout
- External log infrastructure setup beyond existing sinks

# Acceptance criteria
- Serilog active in API and services
- useful logs in submit/publish/consume/payment/shipping/errors
- CorrelationId included
- OrderId/EventType/ServiceName included

# Technical implementation plan
1. Add correlation middleware/helpers for API request context.
2. Standardize worker Serilog bootstrap and enrichers.
3. Add structured logs in checkout completion, publishers, and worker consumers/decision points.
4. Ensure message consumers push correlation/order/service context into LogContext.
5. Run restore/build/test.

# Files likely to change
- SportsStore.Api/**
- SportsStore.Application/**
- SportsStore.Infrastructure/**
- SportsStore.Inventory.Worker/**
- SportsStore.Payment.Worker/**
- SportsStore.Shipping.Worker/**
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: improve serilog correlation observability

# PR plan
- PR from feat/10-serilog-correlation-observability into master with logging summary and validation results

# Risks / assumptions
- Observability is implemented via structured logs and correlation propagation, not a full distributed tracing stack.
