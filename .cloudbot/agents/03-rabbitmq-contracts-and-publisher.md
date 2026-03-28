# Title
RabbitMQ contracts and publisher

# Source user story file path
backlog/03-feat-rabbitmq-contracts-and-publisher.md

# Story summary
Introduce shared order-submission contracts and publish the first real asynchronous order event after checkout persistence, using RabbitMQ wiring and correlation-aware logging.

# In-scope items
- Add an `OrderSubmitted` contract
- Add exchange/routing-key options
- Add a publisher abstraction and RabbitMQ implementation
- Publish after successful order persistence
- Include correlation id, order id, and customer id where available
- Add tests around publish behavior

# Out-of-scope items
- Worker consumers
- Retry/outbox implementation
- Full saga/workflow orchestration

# Acceptance criteria
- Shared contract exists
- `OrderSubmitted` event exists
- Checkout publishes after save
- Save occurs before publish
- CorrelationId is included

# Technical implementation plan
1. Add messaging contracts and publisher abstraction.
2. Add RabbitMQ options/configuration.
3. Implement RabbitMQ publisher with structured logging.
4. Hook checkout completion to publish after repository save.
5. Add tests verifying publish-after-save semantics.
6. Run restore/build/test.

# Files likely to change
- SportsStore.Application/**
- SportsStore.Infrastructure/**
- SportsStore.Tests/**
- .cloudbot/agents/execution-log.md

# Validation steps
- dotnet restore SportsSln.sln
- dotnet build SportsSln.sln -c Debug --no-restore
- dotnet test SportsSln.sln --no-build

# Commit plan
- feat: add rabbitmq contracts and publisher

# PR plan
- PR from feat/03-rabbitmq-contracts-and-publisher into master with contract summary and validation notes

# Risks / assumptions
- RabbitMQ connectivity may not exist in local test execution, so tests should mock publisher dependencies rather than require a live broker.
