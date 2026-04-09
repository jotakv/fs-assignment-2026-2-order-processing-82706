# 🎓 Project Defense Q&A

This document is based on the real distributed runtime in:

- `SportsSln.sln`
- root `docker-compose.yml`
- `admin-dashboard/`

Important note:

- The repository still contains an older `SportsStore/` MVC-style project.
- The main Assignment 2 distributed platform is the API, the three workers, the Blazor frontend, the React admin dashboard, RabbitMQ, and SQL Server.

## 1. Is logging implemented? How does it work?

### What is implemented in this project

- Yes. Logging is implemented clearly.
- `SportsStore.Api` uses Serilog in `SportsStore.Api/Program.cs`.
- The API writes logs to:
  - console
  - rolling files in `SportsStore.Api/Logs/log-.txt`
  - Seq, but only if `Serilog:SeqServerUrl` is configured
- The worker services also use Serilog, but only to the console.
- The application layer adds MediatR request logging through `LoggingBehavior<TRequest, TResponse>`.

### Where logging happens

- API request pipeline:
  - `CorrelationIdMiddleware` adds or reuses `X-Correlation-Id`
  - `UseSerilogRequestLogging` logs HTTP requests
  - the exception handler logs unhandled errors
- Application layer:
  - `LoggingBehavior` logs when a command or query starts and finishes
- RabbitMQ publishers:
  - log connection details, exchange declaration, routing key, order id, and publish success/failure
- RabbitMQ consumers:
  - log consumer startup, queue binding, message consumption, deserialization problems, and processing errors
- Blazor browser storage service:
  - `SessionStorageService` logs failures when reading or writing session storage

### Example of what is logged

- Real API log entries in `SportsStore.Api/Logs/log-20260409.txt` include:
  - `Handling request GetProductsQuery`
  - `Handled request GetProductsQuery`
  - `HTTP GET /api/products responded 200`
- The RabbitMQ code also logs fields like:
  - `CorrelationId`
  - `OrderId`
  - `EventType`
  - `RoutingKey`
  - `Exchange`

### Short answer

The project does have logging. The API uses Serilog with console and file logs, the workers log to console, and MediatR logs each command and query.

### Strong answer

Logging is implemented in several layers, not only in controllers. The API uses Serilog with correlation id support, rolling log files, request logging, and exception logging. The workers add structured log fields like `OrderId`, `CorrelationId`, and `EventType` when they consume RabbitMQ messages, so one order can be followed across services.

## 2. Are you using RabbitMQ? How?

### How RabbitMQ is configured in this repo

- Yes, RabbitMQ is a central part of the distributed workflow.
- Configuration is stored in `SportsStore.Infrastructure/Options/RabbitMqOptions.cs`.
- The connection factory supports two styles:
  - a full AMQP URI
  - separate fields like host, port, username, password, virtual host, and TLS flag
- In local `appsettings.json`, the services point to a CloudAMQP server with TLS.
- In root `docker-compose.yml`, that is overridden to a local `rabbitmq` container with:
  - host `rabbitmq`
  - port `5672`
  - user `guest`
  - password `guest`
  - virtual host `/`
  - `UseTls=false`

### Exchange, queues, and routing keys

- Exchange:
  - `sportsstore.orders` as a durable topic exchange
- Queues:
  - `sportsstore.inventory`
  - `sportsstore.payment`
  - `sportsstore.shipping`
- Routing keys:
  - `order.submitted`
  - `inventory.confirmed`
  - `inventory.failed`
  - `payment.approved`
  - `payment.rejected`
  - `shipping.created`
  - `shipping.failed`

### Where messages are published

| Publisher | Event class | Routing key |
| --- | --- | --- |
| API | `OrderSubmittedIntegrationEvent` | `order.submitted` |
| Inventory worker | `InventoryConfirmedIntegrationEvent` / `InventoryFailedIntegrationEvent` | `inventory.confirmed` / `inventory.failed` |
| Payment worker | `PaymentApprovedIntegrationEvent` / `PaymentRejectedIntegrationEvent` | `payment.approved` / `payment.rejected` |
| Shipping worker | `ShippingCreatedIntegrationEvent` / `ShippingFailedIntegrationEvent` | `shipping.created` / `shipping.failed` |

### Where messages are consumed

| Consumer | Queue | Bound routing key |
| --- | --- | --- |
| `InventoryOrderSubmittedWorker` | `sportsstore.inventory` | `order.submitted` |
| `PaymentInventoryConfirmedWorker` | `sportsstore.payment` | `inventory.confirmed` |
| `ShippingPaymentApprovedWorker` | `sportsstore.shipping` | `payment.approved` |

### Queue flow

1. API saves the order and publishes `order.submitted`.
2. Inventory worker consumes it and decides success or failure.
3. If inventory is confirmed, payment worker consumes `inventory.confirmed`.
4. If payment is approved, shipping worker consumes `payment.approved`.
5. Failure events are published too, but there is no later worker subscribed to them in the current code.

### Important implementation details

- Messages are JSON.
- Publish properties set:
  - `Persistent = true`
  - `ContentType = application/json`
  - `CorrelationId`
  - `Type`
- Consumers use `autoAck: false`.
- On success they `BasicAck`.
- On processing exception they `BasicNack(..., requeue: true)`.

## 3. How do you handle `InventoryConfirmed` message?

### Real flow in this project

- The `InventoryConfirmedIntegrationEvent` is consumed by `PaymentInventoryConfirmedWorker`.
- That worker subscribes to queue `sportsstore.payment`.
- The queue is bound to routing key `inventory.confirmed`.

### What the consumer does

1. `HandleReceivedAsync` receives the raw RabbitMQ message.
2. It deserializes the JSON into `InventoryConfirmedIntegrationEvent`.
3. It pushes `CorrelationId`, `OrderId`, `CustomerId`, and `EventType` into the log context.
4. It calls `ProcessAsync`.

### What happens in `ProcessAsync`

- The worker creates a DI scope.
- It resolves:
  - `IPaymentDecisionService`
  - `StoreDbContext`
  - `IPaymentEventPublisher`
- It loads the order from SQL Server with existing `PaymentRecords`.
- It runs `PaymentDecisionService.Evaluate(...)`.

### What is persisted

- A new `PaymentRecord` is added with fields like:
  - `Provider = "SimulatedPaymentService"`
  - `ExternalPaymentId`
  - `Status = "approved"` or `"rejected"`
  - `FailureReason`
  - `ProcessedAtUtc`
- The main `Order` is also updated:
  - `Status = PaymentApproved` if approved
  - `Status = Failed` if rejected
  - `UpdatedAtUtc` is refreshed
  - `FailedAtUtc` is set on failure

### What event is published next

- If approved:
  - `PaymentApprovedIntegrationEvent`
  - routing key `payment.approved`
- If rejected:
  - `PaymentRejectedIntegrationEvent`
  - routing key `payment.rejected`

### Honest project-specific note

- The order already has a Stripe payment record when it is first saved in `CompleteCheckoutCommandHandler`.
- So the payment worker is a second, simulated back-office payment step in the distributed pipeline, not the same thing as the customer paying on Stripe.

## 4. Where does the message come in?

### Consumer logic in this codebase

The message comes into a background worker service, not into a controller.

Each worker does this in `ExecuteAsync`:

1. Create a RabbitMQ connection through `RabbitMqConnectionFactoryProvider`
2. Create a channel
3. Declare the exchange
4. Declare its queue
5. Bind the queue to one routing key
6. Start `BasicConsume` with `AsyncEventingBasicConsumer`

### Actual subscription logic

- Inventory worker:
  - queue `sportsstore.inventory`
  - routing key `order.submitted`
- Payment worker:
  - queue `sportsstore.payment`
  - routing key `inventory.confirmed`
- Shipping worker:
  - queue `sportsstore.shipping`
  - routing key `payment.approved`

### What happens after subscription

- RabbitMQ delivers the message body to `HandleReceivedAsync`.
- The worker:
  - converts bytes to text
  - deserializes JSON
  - logs the event
  - calls `ProcessAsync`
  - acknowledges the message on success

### Error behavior

- Invalid JSON:
  - logged
  - acknowledged and dropped
- Runtime failure while processing:
  - logged
  - message is negatively acknowledged with `requeue: true`

### Strong exam answer

In this repository, RabbitMQ messages enter through `BackgroundService` consumers, not through HTTP endpoints. Each worker declares its own queue and binding at startup, subscribes with `AsyncEventingBasicConsumer`, deserializes the event, runs business logic, writes to SQL Server, and then acknowledges the message.

## 5. Do you use shared contracts?

### Shared message contracts

- Yes, the .NET services use shared integration event contracts.
- They are defined in `SportsStore.Application/Contracts/Messaging/`.
- Examples:
  - `OrderSubmittedIntegrationEvent`
  - `InventoryConfirmedIntegrationEvent`
  - `InventoryFailedIntegrationEvent`
  - `PaymentApprovedIntegrationEvent`
  - `PaymentRejectedIntegrationEvent`
  - `ShippingCreatedIntegrationEvent`
  - `ShippingFailedIntegrationEvent`

### Shared DTOs and commands

- The API and Blazor frontend also share application models directly.
- These are stored in:
  - `SportsStore.Application/Common/Dtos/`
  - `SportsStore.Application/Features/...`
- Examples reused by Blazor:
  - `CheckoutOrderCommand`
  - `CompleteCheckoutCommand`
  - `CheckoutSessionDto`
  - `CheckoutCompletionDto`
  - `OrderDto`
  - `OrderStatusDto`

### How services reuse them

- API controllers accept the commands and return the DTOs.
- Blazor references `SportsStore.Application` and posts those commands directly to the API.
- Workers reuse the same integration event classes through the infrastructure publishers and consumers.

### Important limitation

- The React admin dashboard does not share the C# contracts directly.
- It has its own TypeScript DTO definitions in `admin-dashboard/src/types.ts`.
- So shared contracts exist strongly inside the .NET part of the system, but not as a cross-language shared package for React.

## 6. How would you debug rising memory usage if GC is normal?

### Practical answer for this architecture

Because this project is split into API plus workers, I would first find which process is growing:

- `SportsStore.Api`
- `SportsStore.Inventory.Worker`
- `SportsStore.Payment.Worker`
- `SportsStore.Shipping.Worker`

That already narrows the problem a lot more than in one monolith.

### What I would check in this codebase

- RabbitMQ consumers:
  - each worker keeps a long-lived connection and channel
  - on processing failure, the message is requeued
  - if one bad message keeps failing, memory and logs can keep growing
- API cache:
  - `MemoryCatalogCache` uses `IMemoryCache`
  - it also tracks keys in a `ConcurrentDictionary`
  - many unique product/category/page combinations could increase memory
- Pending checkout memory:
  - `InMemoryPendingCheckoutStore` is a singleton dictionary
  - it keeps pending checkout data until expiry
  - cleanup only happens during save/get operations
- Long-lived objects:
  - singleton cache
  - singleton pending checkout store
  - RabbitMQ connections and consumers

### What I would not blame first

- The customer cart in Blazor is stored in browser session storage, not in server memory.
- Recent order ids in Blazor are also browser-side.

### Debugging steps

1. Check which service process is growing.
2. Compare RabbitMQ queue depth, unacked messages, and requeue counts.
3. Capture a heap dump from the growing process.
4. Look for retained objects from:
   - `MemoryCatalogCache`
   - `InMemoryPendingCheckoutStore`
   - message consumer callbacks
5. Correlate memory growth with:
   - heavy API traffic
   - abandoned checkout sessions
   - repeated worker failures

### Strong answer

If GC is normal but memory still rises, I would suspect retained references, not garbage collection itself. In this repository the main suspects are the singleton in-memory cache, the singleton pending checkout store, and RabbitMQ consumers requeuing failing messages. I would isolate the exact process first, then inspect heap dumps and RabbitMQ backlog metrics.

## 7. Do you use AutoMapper?

### Yes, AutoMapper is used

- Package reference:
  - `AutoMapper.Extensions.Microsoft.DependencyInjection`
- Registration:
  - `SportsStore.Application/DependencyInjection.cs`
- Mapping profile:
  - `SportsStore.Application/Mapping/StoreMappingProfile.cs`

### How it is used in this project

- `Product -> ProductDto`
- `CreateProductCommand -> Product`
- `UpdateProductCommand -> Product`
- `CartLine -> OrderLineDto`
- `Order -> OrderDto`

### What is special in this repo

`Order -> OrderDto` is not a simple map. The profile has custom logic that:

- converts `OrderStatus` enum to string
- calculates total amount
- calculates item count
- reads the latest inventory record
- reads the latest payment record
- reads the latest shipment record
- fills summary fields like:
  - `PaymentStatus`
  - `InventoryResult`
  - `ShipmentReference`
  - `TrackingNumber`
  - `FailureReason`

### What is mapped manually

Not everything uses AutoMapper.

- `GetAdminOrderDetailsQueryHandler` builds `AdminOrderDetailsDto` manually.
- `GetOrderStatusQueryHandler` builds `OrderStatusDto` and the timeline manually.

That makes sense because those DTOs need custom timeline and history logic.

### Good speaking answer

Yes, AutoMapper is used, but not everywhere. It handles normal DTO mapping, especially products and order summaries, while the more detailed admin and timeline views are mapped manually because they need custom business logic.

## 8. What UI did you use for admin?

### Official admin UI in this repository

- The admin UI is React, not Angular.
- It lives in `admin-dashboard/`.
- It uses:
  - React 18
  - React Router
  - Vite

### Features implemented

- Orders dashboard
  - loads all orders from `GET /api/orders`
  - can filter by status
  - can refresh manually
- Failed orders page
  - loads all orders
  - filters failure states in the React client
- Order details page
  - loads `GET /api/orders/{id}/admin`
  - shows payment, inventory, shipment, failure reason, and line items

### How it connects to the API

- Local development:
  - Vite proxies `/api` to `https://localhost:7061`
- Docker:
  - Nginx serves the React build
  - Nginx proxies `/api` to the `api` container

### Limitations

- It is read-only.
- There are no create, edit, retry, or cancel actions.
- There is no dedicated React test suite at the moment.
- CI only runs `npm run test --if-present`, and `package.json` currently has no test script.
- The API has Identity configured, but the order endpoints used by the dashboard are not protected with `[Authorize]` in the current code.

### Honest exam answer

I used a React admin dashboard. It is a simple operational UI for listing orders, filtering statuses, showing failed orders, and opening one order in detail, but it is not a full admin management panel yet.

## 9. Did you use AI?

### Best honest way to answer

You should answer this honestly and carefully.

For a project like this, a balanced answer is:

- AI can help with:
  - boilerplate code
  - documentation drafts
  - explaining patterns like MediatR or RabbitMQ
  - checking test ideas
- But the real implementation still had to be verified manually because this project has many exact integration points:
  - queue names
  - routing keys
  - shared DTOs
  - EF Core entities and relations
  - Blazor checkout flow
  - React API usage

### Good answer for this codebase

I may use AI as an assistant for ideas, boilerplate, or explaining patterns, but the final code still had to be implemented and checked manually. In this project, the important parts were the exact wiring of MediatR handlers, RabbitMQ events, SQL entities, and frontend/API integration, and those had to match the real codebase.

### What not to say

- Do not say AI built the whole system for you.
- Do not claim features that are not in the repo.
- Do not hide AI use if you actually used it.

## 10. Can customers see their orders?

### Yes, but with an important limitation

- Yes, customers can see orders in the Blazor frontend.
- The current Blazor app shows recent orders from the same browser session.
- It is not a full login-based customer account history.

### How it works in this project

- After successful checkout, `OrderConfirmation.razor` calls `/api/orders/complete`.
- When the API returns the saved order id, Blazor stores that id in `RecentOrdersState`.
- `RecentOrdersState` uses browser session storage and keeps up to 10 recent order ids.
- `Orders.razor` loads those ids and calls:
  - `GET /api/orders/{id}`
  - `GET /api/orders/{id}/status`
- `OrderDetails.razor` shows:
  - shipping address
  - totals
  - payment time
  - shipment state
  - tracking timeline

### Important honesty note

- There is an API endpoint `GET /api/customers/{customerId}/orders`.
- But the current Blazor UI does not use it.
- So customer visibility is session-based in the frontend, not customer-account-based.

### Strong answer

Yes, customers can view their orders in the Blazor app, but the current implementation is based on recent order ids stored in browser session storage. There is also a customer-orders API endpoint, but the UI does not yet provide a full authenticated order-history screen.

## 11. Do you store customer orders?

### Yes, the orders are stored in SQL Server

The main store database contains:

- `Orders`
- `Customers`
- `CartLine`
- `OrderItems`
- `InventoryRecords`
- `PaymentRecords`
- `ShipmentRecords`

### How orders relate to customers

- `Order` has:
  - `CustomerId`
  - navigation property `Customer`
- `Customer` has:
  - `CustomerId`
  - `Name`
  - `Email`
  - navigation collection `Orders`
- This relationship is configured in `StoreDbContext` with:
  - one customer to many orders
  - foreign key on `Order.CustomerId`

### What is stored for one order

The order stores more than one row of data:

- main order record and address fields
- customer reference
- `CartLine` records
- `OrderItem` records with quantity, unit price, and line total
- inventory history
- payment history
- shipment history

### How orders are created

`CompleteCheckoutCommandHandler` creates:

- a new `Order`
- a new `Customer`
- `OrderItem` entries
- `CartLine` entries
- a Stripe payment record
- placeholder inventory and shipment records

Then it saves everything through `IOrderRepository`.

### How queries retrieve them

- `EfOrderRepository.DetailedOrdersQuery()` uses `Include(...)` for:
  - customer
  - lines and products
  - items and products
  - inventory records
  - payment records
  - shipment records
- That is why the API can return rich DTOs for customer screens and admin screens.

### Honest limitation

- The current checkout flow creates a new `Customer` object on each completed checkout.
- It does not try to reuse an existing customer by email.
- So the relation exists, but it behaves more like guest-order storage than a full account system.

## 12. Explain the full order flow end-to-end

### Real step-by-step flow in this repository

1. The customer opens the Blazor storefront.
   - `Home.razor` loads products through `CatalogApiClient`.
   - The API serves them from `GET /api/products`.

2. The customer adds products to the cart.
   - `CartState` stores the cart in browser session storage.

3. The customer opens `/checkout` and enters shipping details.
   - `Checkout.razor` builds `CheckoutOrderCommand`.
   - Blazor sends it to `POST /api/orders/checkout`.

4. The API starts checkout, but does not save the final order yet.
   - `CheckoutOrderCommandHandler` validates the cart.
   - It loads products from the repository.
   - It creates a `PendingCheckout` object.
   - That object is stored in `InMemoryPendingCheckoutStore`.
   - Then it asks Stripe to create a checkout session.

5. The browser is redirected to Stripe.
   - Stripe handles the customer payment page.

6. Stripe returns the browser to `/checkout/complete`.
   - Blazor gets `session_id` and `pendingCheckoutId` from the query string.
   - `OrderConfirmation.razor` calls `POST /api/orders/complete`.

7. The API now verifies payment and creates the real order.
   - `CompleteCheckoutCommandHandler` first checks whether that Stripe session already created an order.
   - If yes, it returns the existing order and avoids duplication.
   - If no, it asks Stripe for the checkout session.
   - The session must be `paid`.
   - Then the API loads the pending checkout and products.
   - It creates:
     - `Order`
     - `Customer`
     - `OrderItem` list
     - `CartLine` list
     - initial Stripe `PaymentRecord`
     - placeholder `InventoryRecord` and `ShipmentRecord`
   - Then it saves the order to SQL Server.

8. After saving, the API publishes the first event.
   - `RabbitMqOrderEventPublisher` publishes `OrderSubmittedIntegrationEvent`
   - exchange: `sportsstore.orders`
   - routing key: `order.submitted`

9. Inventory worker processes the order.
   - `InventoryOrderSubmittedWorker` consumes `order.submitted`.
   - It runs `InventoryDecisionService`.
   - The rule is simulated and deterministic:
     - an item succeeds only if requested quantity is less than or equal to `Math.Abs(productId % 5) + 1`
   - The worker stores an `InventoryRecord`.
   - It updates the order status to:
     - `InventoryConfirmed`
     - or `Failed`
   - Then it publishes:
     - `inventory.confirmed`
     - or `inventory.failed`

10. Payment worker processes confirmed inventory.
   - `PaymentInventoryConfirmedWorker` consumes `inventory.confirmed`.
   - It runs `PaymentDecisionService`.
   - The live worker flow mainly uses a deterministic order-id rule:
     - most orders are approved
     - some are rejected when `orderId % 10` falls into the failure branch
   - It stores a `PaymentRecord`.
   - It updates the order status to:
     - `PaymentApproved`
     - or `Failed`
   - Then it publishes:
     - `payment.approved`
     - or `payment.rejected`

11. Shipping worker processes approved payment.
   - `ShippingPaymentApprovedWorker` consumes `payment.approved`.
   - It runs `ShipmentCreationService`.
   - Shipping is also simulated:
     - it fails when `orderId % 20 == 0`
   - It stores a `ShipmentRecord`.
   - It updates the order status to:
     - `Completed`
     - or `Failed`
   - On success it also sets:
     - `CompletedAtUtc`
     - `Shipped = true`
   - Then it publishes:
     - `shipping.created`
     - or `shipping.failed`

12. The frontends read the final state from the API.
   - Blazor uses:
     - `GET /api/orders/{id}`
     - `GET /api/orders/{id}/status`
   - React admin uses:
     - `GET /api/orders`
     - `GET /api/orders/{id}/admin`

### Final state

The order finishes in one of these practical states:

- `Completed`
- `Failed`

The exact failure stage is kept in the history tables and timeline details, even though the main order status is often the generic `Failed`.

### Very important honest note

This repo has one unusual but real detail:

- Stripe payment is already confirmed before the order is first saved.
- `CompleteCheckoutCommandHandler` even saves the new order with `Status = PaymentApproved`.
- After that, the asynchronous workers still run inventory, simulated payment, and shipping.

So if you explain the flow in an exam, say it honestly:

This implementation uses Stripe to confirm the checkout first, then it starts the distributed back-office pipeline with RabbitMQ.

## 13. What architecture pattern is used?

### 1. Event-driven architecture

Yes, this project clearly uses event-driven processing.

- The API publishes `order.submitted`.
- Workers react to events from RabbitMQ.
- Each worker publishes the next event for the next stage.

This is not just theory. It is implemented with:

- one RabbitMQ topic exchange
- three worker queues
- explicit routing keys
- background consumers

### 2. Layered or clean-like architecture

The project is structured in layers:

- `SportsStore.Domain`
- `SportsStore.Application`
- `SportsStore.Infrastructure`
- entry points:
  - `SportsStore.Api`
  - workers
  - `SportsStore.Blazor`

It is close to clean architecture because:

- domain entities are separated
- use cases live in the application layer
- infrastructure handles external systems

But it is not a perfect textbook clean architecture, because:

- workers use `StoreDbContext` directly
- all services share one SQL Server database

So the best honest answer is:

This is a layered, clean-like architecture with event-driven processing.

### 3. CQRS with MediatR

Yes, CQRS is used in the API and application layer.

- Commands change state:
  - `CheckoutOrderCommand`
  - `CompleteCheckoutCommand`
  - `MarkOrderShippedCommand`
  - product create/update/delete commands
- Queries read data:
  - `GetProductsQuery`
  - `GetOrdersQuery`
  - `GetOrderByIdQuery`
  - `GetOrderStatusQuery`
  - `GetAdminOrderDetailsQuery`
  - `GetCustomerOrdersQuery`

Controllers do not contain business logic. They send commands and queries through MediatR.

### Important scope note

- CQRS is not used inside the workers.
- The workers use background services, direct EF Core access, and small decision services.

## 14. Why is this better than a monolith?

### Practical advantages in this implementation

- The API does not need to do inventory, payment, and shipping in one blocking HTTP request.
- Inventory, payment, and shipping are separated into different worker services.
- If shipping logic changes, it can change without rewriting the whole API.
- If one worker fails, the storefront and API can still run.
- The React admin dashboard can monitor orders without being mixed into the customer UI.

### Example from this repo

In a monolith, checkout might:

1. save the order
2. check stock
3. process payment
4. create shipment
5. return to the browser only after all of that

In this repository:

1. the API saves the order and publishes an event
2. workers continue the heavy steps asynchronously
3. the frontends can later read the updated state

### Why this is good for scaling

- If inventory becomes busy, only the inventory worker needs more scale.
- If admin traffic increases, the React dashboard still calls the same API without changing the worker pipeline.

### Honest trade-off

It is not automatically better in every way.

- A distributed system is harder to debug.
- Eventual consistency appears because updates happen in steps.
- This implementation still uses one shared SQL Server database, so it is not a fully independent microservice database design.

### Strong answer

It is better than a monolith here because the slow business stages are decoupled. The API stays focused on accepting requests and publishing events, while specialized workers process inventory, payment, and shipping in the background. The trade-off is extra complexity and shared-database coupling.

## 15. Key strengths of this implementation

### Real strengths from the code

- Clear service separation
  - API, three workers, customer UI, admin UI
- Real asynchronous messaging
  - RabbitMQ exchange, queues, routing keys, consumers, and publishers are all implemented
- Shared application contracts on the .NET side
  - DTOs, commands, and integration events are reused across projects
- Good operational visibility
  - orders keep inventory, payment, and shipment history
  - the API exposes status and admin detail endpoints
- Structured logging
  - correlation ids, order ids, routing keys, and event types are logged
- CQRS in the API
  - controllers stay thin
  - application handlers hold business logic
- Dockerized platform
  - API, workers, Blazor, admin dashboard, RabbitMQ, and SQL Server run together
- CI validation
  - .NET build and tests
  - coverage report generation
  - React install and production build
- Test coverage across layers
  - 34 automated tests passed in my verification run
  - tests cover API controllers, application handlers, domain behavior, infrastructure, and Blazor API client logic
- A small resilience improvement already exists
  - `CompleteCheckoutCommandHandler` checks `StripeSessionId` before creating a new order, so repeated completion calls do not create duplicates

### Honest limitations to mention if asked

- No dedicated React test suite yet
- No dead-letter queue or outbox implementation
- Shared database across services
- Customer order history in Blazor is session-based, not account-based
- Some enum statuses exist but are not used as full persisted workflow states

# 🧠 Quick Answers (for exam)

## Architecture

This project is a distributed platform with a layered design. `SportsStore.Api` is the center, `SportsStore.Application` contains CQRS handlers, `SportsStore.Domain` contains entities, `SportsStore.Infrastructure` handles SQL Server, RabbitMQ, Stripe, and caching, and the workers process messages in the background.

## RabbitMQ

RabbitMQ is used as the message broker for the order pipeline. The code uses one topic exchange called `sportsstore.orders`, with durable queues for inventory, payment, and shipping, and routing keys like `order.submitted`, `inventory.confirmed`, and `payment.approved`.

## CQRS

CQRS is implemented with MediatR in the API and application layer. Commands change state, like checkout and product updates, while queries return DTOs for product lists, order details, status tracking, and admin views.

## Flow

The real flow is: Blazor starts checkout, Stripe confirms payment, the API saves the order, then RabbitMQ triggers inventory, payment, and shipping workers one after another. Each worker updates SQL Server and publishes the next event.

## Services

The services are specialized. The API accepts requests and publishes events, the inventory worker validates stock, the payment worker performs a simulated back-office payment decision, the shipping worker creates shipment data, Blazor is the customer UI, and React is the admin UI.
