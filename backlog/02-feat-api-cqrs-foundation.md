# 2) `feat/02-api-cqrs-foundation`

## User story

**As a frontend consumer, I want a clean Order Management API with CQRS so that both UIs can use consistent endpoints without direct data access coupling.**

## Descripción

Aquí dejas la base fuerte de la API central.

## Criterios de aceptación

- controllers finos
- MediatR usado en commands/queries
- AutoMapper configurado
- caching en productos
- endpoints mínimos funcionando

## Tareas técnicas

- crear queries:
  - `GetProductsQuery`
  - `GetOrderByIdQuery`
  - `GetOrdersQuery`
  - `GetCustomerOrdersQuery`
  - `GetOrdersByStatusQuery`
- crear commands:
  - `CheckoutOrderCommand`
  - `CancelOrderCommand` si lo necesitas
- añadir profiles de AutoMapper
- añadir `IMemoryCache` para catálogo

## Prompt para Codex

```text
Work only on branch feat/02-api-cqrs-foundation.

Goal:
Complete the Order Management API foundation using CQRS with MediatR, AutoMapper, and in-memory caching for read-heavy catalog endpoints.

Requirements:
- Keep controllers thin
- Add or complete queries:
  GetProductsQuery, GetOrderByIdQuery, GetOrdersQuery, GetCustomerOrdersQuery, GetOrdersByStatusQuery
- Add or complete commands:
  CheckoutOrderCommand, CancelOrderCommand if appropriate
- Add AutoMapper profiles for entities, DTOs, and API responses
- Add IMemoryCache for product list and product details
- Add cache invalidation when products change
- Expose endpoints:
  GET /api/products
  GET /api/orders
  GET /api/orders/{id}
  GET /api/orders/{id}/status
  GET /api/customers/{id}/orders
  POST /api/orders/checkout
- Keep build and tests green

Deliver:
- code changes
- files changed
- endpoint summary
- build/test results
```
