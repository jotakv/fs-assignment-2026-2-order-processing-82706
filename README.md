# SportsStore Assignment 2 Distributed Platform

This repository is the formal Assignment 2 delivery for a distributed order-processing platform built around .NET services, RabbitMQ messaging, SQL Server storage, a Blazor customer portal, and a React admin dashboard.

The React dashboard in `admin-dashboard/` is an official system component, not a side experiment. It is part of the documented architecture, included in Docker Compose, and validated in GitHub Actions alongside the .NET solution.

## Delivery Position

The `.NET` projects are represented in `SportsSln.sln`:

- `SportsStore.Api`
- `SportsStore.Inventory.Worker`
- `SportsStore.Payment.Worker`
- `SportsStore.Shipping.Worker`
- `SportsStore.Blazor`
- supporting application, domain, infrastructure, and test projects

The React admin dashboard remains in `admin-dashboard/` because Vite/Node applications are not native `.csproj` projects. Instead of forcing it into the `.sln`, this repository integrates it formally through:

- root-level documentation
- `docker-compose.yml` as part of the runtime platform
- GitHub Actions CI validation
- shared API contracts and operational workflows

That makes the dashboard part of the delivered system while keeping the project structure technically correct.

## Official Solution Components

| Component | Technology | Responsibility |
| :--- | :--- | :--- |
| `SportsStore.Api` | ASP.NET Core | Central order management API, event publication, state transitions, and endpoints shared by both frontends |
| `SportsStore.Inventory.Worker` | .NET worker service | Inventory validation for submitted orders |
| `SportsStore.Payment.Worker` | .NET worker service | Payment processing for orders that pass inventory checks |
| `SportsStore.Shipping.Worker` | .NET worker service | Shipment creation for paid orders |
| `SportsStore.Blazor` | Blazor | Customer-facing storefront and checkout interface |
| `admin-dashboard` | React + Vite | Administration and operations frontend for monitoring orders and failures |
| `rabbitmq` | RabbitMQ | Message broker coordinating the asynchronous workflow |
| `db` | SQL Server | Shared persistence for orders, products, identity, and processing records |

Additional component detail is documented in [docs/solution-components.md](docs/solution-components.md).

## Architecture

Both frontends are first-class clients of the same distributed backend:

```text
Blazor Customer Portal  --->  SportsStore.Api  <---  React Admin Dashboard
                                   |
                                   v
                                RabbitMQ
                           /        |        \
                          v         v         v
                   Inventory   Payment   Shipping
                     Worker     Worker     Worker
                                   |
                                   v
                               SQL Server
```

### Event Flow

1. The customer checks out through the Blazor portal.
2. `SportsStore.Api` stores the order and publishes the next RabbitMQ event.
3. Inventory, payment, and shipping workers process the order asynchronously.
4. Order state is updated as each worker completes or fails.
5. The React admin dashboard reads the operational endpoints from the API to monitor the current system state, failed orders, and order details.

## Running the Full Platform with Docker Compose

The repository root contains the Assignment 2 runtime entry point: `docker-compose.yml`.

### Prerequisites

- Docker Desktop or Docker Engine with Docker Compose v2
- A free local SQL Server port (`1433`)
- A free RabbitMQ Management port (`15672`)
- Optional Stripe test keys for end-to-end checkout flows

### Start the platform

```bash
docker compose up --build
```

Stop the environment with:

```bash
docker compose down
```

Remove containers and persisted data with:

```bash
docker compose down -v
```

### Runtime services and ports

| Service | Compose service | Host access |
| :--- | :--- | :--- |
| Order API | `api` | `http://localhost:7061` |
| Blazor customer portal | `blazor` | `http://localhost:7049` |
| React admin dashboard | `admin-dashboard` | `http://localhost:5174` |
| RabbitMQ AMQP | `rabbitmq` | `localhost:5672` |
| RabbitMQ Management UI | `rabbitmq` | `http://localhost:15672` |
| SQL Server | `db` | `localhost:1433` |

### Compose integration notes

- The admin dashboard is built from `admin-dashboard/Dockerfile`.
- In Docker, the dashboard is served by Nginx and proxies `/api` to the `api` container.
- The Docker Compose setup is the formal distributed runtime for Assignment 2.
- The older Docker assets under `SportsStore/` belong to the legacy MVC project and are not the primary runtime path for this submission.

## Local Development

### .NET solution

Open `SportsSln.sln` in Visual Studio or run the required projects from the repository root with `dotnet run`.

For a full local distributed workflow outside Docker, start at least:

- `SportsStore.Api`
- `SportsStore.Inventory.Worker`
- `SportsStore.Payment.Worker`
- `SportsStore.Shipping.Worker`
- `SportsStore.Blazor`

### React admin dashboard

The admin dashboard is the official administration and operations frontend for Assignment 2.

Run it locally with:

```bash
cd admin-dashboard
npm install
npm run dev
```

Build it locally with:

```bash
cd admin-dashboard
npm run build
```

The Vite dev server runs on `http://localhost:5174`.

### API connectivity

- By default, local dashboard development uses the Vite `/api` proxy targeting `https://localhost:7061`.
- Override the proxy target with `VITE_API_PROXY_TARGET`.
- If the dashboard is hosted without the Vite proxy, set `VITE_API_BASE_URL` to the API base URL.
- In Docker Compose, Nginx proxies `/api` directly to the `api` service, so the dashboard remains part of the same runtime platform.

## CI Validation

GitHub Actions now validates the full submission rather than only the .NET projects.

The CI workflow performs:

- `.NET` restore, build, and test execution for `SportsSln.sln`
- coverage report generation and GitHub Pages publication for backend test coverage
- `admin-dashboard` dependency installation with `npm ci`
- dashboard tests if a `test` script is defined
- dashboard production build with `npm run build`

There is currently no dedicated React test suite in `admin-dashboard/`, so the formal frontend validation is the install and production build step.

## Assumptions and Limitations

- Stripe keys are not committed. Without valid Stripe configuration, checkout session creation will fail even though the platform can still start.
- The React admin dashboard is intentionally not added as a fake project inside `SportsSln.sln`; the correct integration path for a Vite application is repository, CI, Docker, and documentation integration.
- GitHub Pages deployment in CI publishes the backend coverage report, not the dashboard itself.

## Documentation

- [docs/solution-components.md](docs/solution-components.md) describes each official runtime component and how the dashboard is formally integrated into the submitted solution.
