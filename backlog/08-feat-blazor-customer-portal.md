# 8) `feat/08-blazor-customer-portal`

## User story

**As a customer, I want a Blazor portal to browse products, place orders, and track them through the API.**

## Descripción

Aquí cierras el frontend cliente.

## Criterios de aceptación

- listado de productos
- detalle
- carrito
- checkout
- confirmación
- mis pedidos
- tracking
- consume API, no DbContext directo

## Tareas técnicas

- páginas Blazor
- servicios `HttpClient`
- manejo de cart state
- integración con checkout y tracking

## Prompt para Codex

```text
Work only on branch feat/08-blazor-customer-portal.

Goal:
Complete the customer-facing Blazor portal so customers can browse products, manage a cart, checkout, and track orders through the API.

Requirements:
- Build or complete pages for:
  Product Listing, Product Details, Shopping Cart, Checkout, Order Confirmation, My Orders, Order Tracking
- The Blazor app must consume the API via HttpClient
- Do not use direct EF Core DbContext access from the Blazor project
- Reuse existing business behavior where possible
- Keep the UI functional rather than heavily styled
- Keep solution buildable
- Add/update tests where reasonable

Deliver:
- code changes
- files changed
- page summary
- build/test results
```
