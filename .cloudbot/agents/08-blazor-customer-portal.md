# Title
Blazor customer portal

# Source user story file path
backlog/08-feat-blazor-customer-portal.md

# Story summary
Complete the customer-facing Blazor portal by wiring the existing product/cart/checkout experience to richer order tracking through the API, keeping the app API-driven and functional.

# In-scope items
- Complete/finish customer pages already present
- Add API-based order status tracking in the portal
- Improve recent orders and order details views
- Keep cart state and checkout flow working through HttpClient services
- Add/update tests where reasonable

# Out-of-scope items
- Heavy visual redesign
- Direct DbContext usage from Blazor
- Admin dashboard work

# Acceptance criteria
- product listing/detail/cart/checkout/confirmation exist
- my orders exists
- tracking exists
- app consumes API only

# Technical implementation plan
1. Extend `OrdersApiClient` to fetch order status.
2. Enrich order confirmation/recent orders/order details pages with tracking data.
3. Keep all interactions through HttpClient-backed services.
4. Add focused client tests for API behavior.
5. Run restore/build/test.

# Files likely to change
- SportsStore.Blazor/**
- SportsStore.Tests/**
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: complete blazor customer portal

# PR plan
- PR from feat/08-blazor-customer-portal into master with page/API summary and validation results

# Risks / assumptions
- Most portal pages already exist; this story focuses on completing the missing API-driven tracking/user flow rather than replacing the portal structure.
