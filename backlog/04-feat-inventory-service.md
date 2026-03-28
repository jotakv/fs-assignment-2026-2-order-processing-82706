# 4) `feat/04-inventory-service`

## User story

**As the inventory service, I want to consume submitted orders and validate stock so that the system can decide whether the order can continue.**

## Descripción

Primer worker real.

## Criterios de aceptación

- consume `OrderSubmitted`
- valida stock
- genera `InventoryRecord`
- publica `InventoryConfirmed` o `InventoryFailed`

## Tareas técnicas

- crear service/worker
- crear consumer
- validar cantidades
- simular reserva
- persistir resultado
- publicar outcome

## Prompt para Codex

```text
Work only on branch feat/04-inventory-service.

Goal:
Implement the Inventory Service that consumes submitted orders, validates stock, stores the result, and publishes an inventory outcome event.

Requirements:
- Create an Inventory Service/worker project if it does not already exist
- Consume OrderSubmitted from RabbitMQ
- Validate requested product quantities
- Simulate stock reservation
- Persist InventoryRecord results
- Publish InventoryConfirmed or InventoryFailed
- Add structured logging with OrderId, EventType, ServiceName, CorrelationId
- Keep solution buildable
- Add or update tests for inventory decision logic

Deliver:
- code changes
- files changed
- consumption/publishing summary
- build/test results
```
