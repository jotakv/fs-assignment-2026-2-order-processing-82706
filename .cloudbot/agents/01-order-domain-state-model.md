# Title
Order domain state model

# Source user story file path
backlog/01-feat-order-domain-state-model.md

# Story summary
Introduce the minimum order-processing domain entities and explicit order status lifecycle in the modern Domain/Infrastructure stack, preserving current checkout compatibility.

# In-scope items
- Add Customer, OrderItem, InventoryRecord, PaymentRecord, ShipmentRecord entities
- Add explicit OrderStatus lifecycle
- Extend Order with customer, item, status, timestamps, and related records
- Register new DbSets and EF Core relationships in StoreDbContext
- Add migration for the modern persistence model
- Add focused tests for the order lifecycle model

# Out-of-scope items
- RabbitMQ/event consumers
- Full microservice orchestration
- UI/dashboard changes
- Refactoring the legacy SportsStore MVC persistence layer

# Acceptance criteria
- Product, Customer, Order, OrderItem, InventoryRecord, PaymentRecord, ShipmentRecord exist in the active modern model
- OrderStatus is explicit and usable
- Relationships are defined in EF Core
- Migrations are present for the model changes

# Technical implementation plan
1. Add missing domain entities and an OrderStatus enum.
2. Extend Order while keeping existing checkout fields intact.
3. Update StoreDbContext with DbSets and fluent configuration.
4. Update repository include graphs if required.
5. Add tests covering lifecycle defaults/relationships.
6. Generate an EF Core migration from the active startup project.
7. Run restore/build/test.

# Files likely to change
- SportsStore.Domain/**
- SportsStore.Infrastructure/Persistence/**
- SportsStore.Tests/**
- SportsStore/Migrations/**

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: implement order domain state model

# PR plan
- PR from feat/01-order-domain-state-model into master
- Include story path, implementation notes, validation results, assumptions, and deferred legacy-model concerns

# Risks / assumptions
- The repository contains a legacy SportsStore MVC model and a newer Domain/Application/Infrastructure stack; this story is implemented in the newer stack used by the API/tests.
- Generated migrations may land under the legacy startup project because that is where existing migrations live.
- Code build/tests are green, but EF migration generation is currently blocked in the environment because `dotnet-ef` is not installed as a CLI tool.
