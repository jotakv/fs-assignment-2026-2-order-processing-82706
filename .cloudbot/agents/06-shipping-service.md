# Title
Shipping service

# Source user story file path
backlog/06-feat-shipping-service.md

# Story summary
Add the shipping worker that reacts to approved payments, creates shipment details, persists shipment data, and publishes the shipping outcome.

# In-scope items
- Add shipping worker project
- Consume `PaymentApproved`
- Generate shipment reference and tracking data
- Estimate dispatch date
- Persist `ShipmentRecord`
- Publish `ShippingCreated` or `ShippingFailed`
- Add tests for shipment creation logic

# Out-of-scope items
- Carrier API integrations
- Label printing
- Delivery tracking callbacks

# Acceptance criteria
- consumes `PaymentApproved`
- generates shipping reference
- estimates date
- stores `ShipmentRecord`
- publishes `ShippingCreated` or `ShippingFailed`

# Technical implementation plan
1. Add shipping outcome contracts and publisher support.
2. Create shipping worker project and RabbitMQ consumer.
3. Add deterministic shipment creation logic.
4. Update shipment persistence and order status.
5. Publish created/failed event with structured logging.
6. Add tests and run build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Infrastructure/**
- SportsStore.Shipping.Worker/**
- SportsStore.Tests/**
- SportsSln.sln
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: implement shipping service worker

# PR plan
- PR from feat/06-shipping-service into master with shipping flow summary and validation results

# Risks / assumptions
- Shipment creation is intentionally simulated and deterministic for the assignment step.
