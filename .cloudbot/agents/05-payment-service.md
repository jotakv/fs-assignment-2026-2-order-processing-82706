# Title
Payment service

# Source user story file path
backlog/05-feat-payment-service.md

# Story summary
Add the payment worker that reacts only to successful inventory confirmation, simulates payment authorization with deterministic rejection rules, persists payment records, and publishes payment outcomes.

# In-scope items
- Add payment worker project
- Consume `InventoryConfirmed`
- Simulate payment approval/rejection
- Reject specific test cards and some deterministic samples
- Persist `PaymentRecord`
- Publish `PaymentApproved` or `PaymentRejected`
- Add tests for payment decision rules

# Out-of-scope items
- Real payment gateway integration
- Refunds/voids
- Full anti-fraud pipeline

# Acceptance criteria
- consumes after inventory confirmation only
- stores `PaymentRecord`
- approves most payments
- rejects some deterministically
- rejects specific test card numbers
- publishes payment outcome

# Technical implementation plan
1. Add payment outcome contracts and publisher support.
2. Create payment worker project and RabbitMQ consumer.
3. Add deterministic payment decision logic and explicit test-card rejection rules.
4. Update order/payment persistence.
5. Publish approved/rejected events with correlation-aware logging.
6. Add tests and run build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Infrastructure/**
- SportsStore.Payment.Worker/**
- SportsStore.Tests/**
- SportsSln.sln
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: implement payment service worker

# PR plan
- PR from feat/05-payment-service into master with payment rules summary and validation results

# Risks / assumptions
- Payment authorization is intentionally simulated with deterministic rules to keep it testable and assignment-friendly.
