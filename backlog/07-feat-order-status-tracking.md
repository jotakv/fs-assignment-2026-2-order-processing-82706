# 7) `feat/07-order-status-tracking`

## User story

**As a customer or admin, I want the order status to be updated after each event so that I can see the real progress of the order.**

## Descripción

Esto cierra la trazabilidad.

## Criterios de aceptación

- la API actualiza el estado del pedido tras cada evento
- `GET /api/orders/{id}/status` devuelve el estado real
- se puede llegar a `Completed` o `Failed`
- los fallos dejan rastro

## Tareas técnicas

- handlers/consumers de eventos de resultado
- transición de estados
- actualizar timestamps
- devolver estado/motivo desde API

## Prompt para Codex

```text
Work only on branch feat/07-order-status-tracking.

Goal:
Update order state transitions in the central API based on downstream event outcomes and expose accurate tracking endpoints.

Requirements:
- Listen to downstream events such as InventoryConfirmed, InventoryFailed, PaymentApproved, PaymentRejected, ShippingCreated, ShippingFailed
- Update OrderStatus appropriately
- Mark orders as Completed or Failed where appropriate
- Expose GET /api/orders/{id}/status with meaningful information
- Preserve CQRS where possible
- Add tests for order state transitions
- Keep solution buildable

Deliver:
- code changes
- files changed
- state transition summary
- build/test results
```
