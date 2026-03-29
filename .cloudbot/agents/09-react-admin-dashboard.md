# Title
React admin dashboard

# Source user story file path
backlog/09-feat-react-admin-dashboard.md

# Story summary
Create the separate React admin dashboard to monitor distributed order operations, using the existing API only.

# In-scope items
- Add a React app inside the repo
- Orders dashboard/table
- Filter by status
- Order details with operational fields
- Failed orders screen
- Minimal API enrichments if needed for operational detail
- Document how to run the React app

# Out-of-scope items
- Full design system
- Backend redesign
- Authentication/authorization hardening

# Acceptance criteria
- order list exists
- status filters exist
- details show payment/inventory/shipping/failure data
- failed orders screen exists
- app consumes API only

# Technical implementation plan
1. Add operational order detail DTO if current API shape is too thin.
2. Add lightweight admin-focused API endpoint(s) via existing stack.
3. Create React app with Vite.
4. Implement dashboard, table, details, and failed orders screens.
5. Wire filters and API calls.
6. Add tests where reasonable and run build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Api/**
- SportsStore.Tests/**
- admin-dashboard/**
- README / docs if needed
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build
- npm install
- npm run build

# Commit plan
- feat: implement react admin dashboard

# PR plan
- PR from feat/09-react-admin-dashboard into master with run instructions and page summary

# Risks / assumptions
- The React app will be kept simple and functional, likely as a separate folder served independently during development.
