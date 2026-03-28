# Title
API CQRS foundation

# Source user story file path
backlog/02-feat-api-cqrs-foundation.md

# Story summary
Complete the missing order-management API foundation pieces around CQRS queries and thin controllers while preserving the existing MediatR, AutoMapper, and catalog caching approach.

# In-scope items
- Add customer-orders and orders-by-status queries
- Expose order status and customer order endpoints
- Keep controllers thin and routed through MediatR
- Reuse existing AutoMapper and caching setup
- Add focused tests for new queries/endpoints

# Out-of-scope items
- New UI work
- Distributed messaging consumers
- Large refactors to existing product CQRS/caching code

# Acceptance criteria
- Controllers remain thin
- MediatR backs commands/queries
- AutoMapper remains configured
- Product caching stays active
- Minimum endpoints function, including customer and order status reads

# Technical implementation plan
1. Extend order repository contract with customer/status query methods.
2. Implement new MediatR queries and DTOs as needed.
3. Add thin API endpoints for order status and customer orders.
4. Add tests for new handlers/controllers.
5. Run restore/build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Infrastructure/**
- SportsStore.Api/Controllers/**
- SportsStore.Tests/**
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: complete api cqrs foundation

# PR plan
- PR from feat/02-api-cqrs-foundation into master with implementation notes and validation results

# Risks / assumptions
- Existing product CQRS/caching baseline is already present; this story closes the most obvious missing order-management pieces instead of rewriting the whole API surface.
