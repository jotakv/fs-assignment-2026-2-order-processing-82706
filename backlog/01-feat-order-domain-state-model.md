# 1) `feat/01-order-domain-state-model`

## User story

**As the order processing system, I want a clear order domain and status lifecycle so that I can track the workflow from checkout to completion or failure.**

## Descripción

Aquí cierras el modelo de dominio mínimo que pide el assignment.

## Criterios de aceptación

- existen entidades mínimas:
  - `Product`
  - `Customer`
  - `Order`
  - `OrderItem`
  - `InventoryRecord`
  - `PaymentRecord`
  - `ShipmentRecord`
- existe un `OrderStatus` claro
- las relaciones están bien definidas
- las migraciones se aplican correctamente

## Tareas técnicas

- revisar `Order`
- añadir tablas de inventario, pago y envío si faltan
- definir estados del pedido
- persistir timestamps y referencias relevantes
- crear/actualizar migraciones

## Prompt para Codex

```text
Work only on branch feat/01-order-domain-state-model.

Goal:
Complete the minimum domain and persistence model needed for distributed order processing.

Requirements:
- Ensure the solution has clear entities for Product, Customer, Order, OrderItem, InventoryRecord, PaymentRecord, ShipmentRecord
- Add or normalize OrderStatus lifecycle:
  Submitted, InventoryPending, InventoryConfirmed, InventoryFailed,
  PaymentPending, PaymentApproved, PaymentFailed,
  ShippingPending, ShippingCreated, Completed, Failed
- Keep the schema manageable
- Add/update EF Core configurations and migrations
- Preserve compatibility with the current codebase as much as possible
- Do not implement RabbitMQ consumers yet
- Keep solution buildable and tests updated

Deliver:
- code changes
- migration details
- files changed
- build/test results
```
