# Solution Components

This document defines the official components of the Assignment 2 delivery. The React admin dashboard is part of the submitted system architecture and operations workflow; it is not an optional extra.

## Integration Position

`SportsSln.sln` contains the formal `.NET` projects in the solution:

- `SportsStore.Api`
- `SportsStore.Application`
- `SportsStore.Domain`
- `SportsStore.Infrastructure`
- `SportsStore.Inventory.Worker`
- `SportsStore.Payment.Worker`
- `SportsStore.Shipping.Worker`
- `SportsStore.Blazor`
- `SportsStore.Tests`

The React admin dashboard stays in `admin-dashboard/` because it uses a Node/Vite toolchain rather than a `.NET` project system. For this repository, the academically defensible integration strategy is:

- keep the `.NET` solution technically correct
- treat `admin-dashboard/` as an official top-level runtime component
- validate it in CI
- run it in Docker Compose with the rest of the platform
- document it as the administration frontend for the same distributed system

## Official Runtime Components

| Component | Location | Technology | Role in the system |
| :--- | :--- | :--- | :--- |
| Order management API | `SportsStore.Api/` | ASP.NET Core | Stores orders, exposes shared endpoints, publishes order-processing events, and tracks workflow state |
| Inventory worker | `SportsStore.Inventory.Worker/` | .NET worker service | Consumes order events and validates inventory availability |
| Payment worker | `SportsStore.Payment.Worker/` | .NET worker service | Processes payment after inventory confirmation |
| Shipping worker | `SportsStore.Shipping.Worker/` | .NET worker service | Creates shipment data after payment approval |
| Customer portal | `SportsStore.Blazor/` | Blazor | Customer-facing UI for browsing products, checking out, and viewing orders |
| Admin dashboard | `admin-dashboard/` | React + Vite | Administration and operations UI for monitoring order status, reviewing failures, and viewing order details |
| Message broker | `docker-compose.yml` service `rabbitmq` | RabbitMQ | Coordinates the asynchronous workflow between API and workers |
| Database | `docker-compose.yml` service `db` | SQL Server | Shared data store for the distributed platform |

## How the Admin Dashboard Is Integrated

The dashboard is formally integrated in four ways:

1. Architecture
   The dashboard is one of the two required frontends in the Assignment 2 platform and shares the same backend API as the Blazor customer portal.
2. Runtime
   `docker-compose.yml` builds and runs the dashboard as the `admin-dashboard` service alongside the API, workers, RabbitMQ, and SQL Server.
3. CI
   `.github/workflows/ci.yml` installs dashboard dependencies and runs a production build so the frontend is part of the repository's automated validation.
4. Documentation
   The root `README.md` presents the dashboard as the administration frontend of the distributed solution, including local run instructions and API connectivity details.

## Component Interactions

- `SportsStore.Blazor` sends customer workflows to `SportsStore.Api`.
- `admin-dashboard` calls operational API endpoints to list orders, inspect failures, and view order details.
- `SportsStore.Api` publishes events to RabbitMQ for asynchronous processing.
- The worker services consume those events and update the system state.
- Both frontends rely on the same backend data and order lifecycle, which keeps the delivery story unified.
