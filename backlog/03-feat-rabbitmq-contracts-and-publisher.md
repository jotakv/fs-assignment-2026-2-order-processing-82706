# 3) `feat/03-rabbitmq-contracts-and-publisher`

## User story

**As the checkout process, I want to publish an order event to RabbitMQ so that downstream services can process the order asynchronously.**

## Descripción

Aquí metes RabbitMQ de verdad, pero solo el primer paso: contratos y publicación.

## Criterios de aceptación

- existe una librería de contratos o equivalente
- existe un evento `OrderSubmitted`
- al hacer checkout se publica un mensaje real
- el pedido queda guardado antes de publicar
- se usa `CorrelationId`

## Tareas técnicas

- definir contratos
- definir exchange/routing key
- crear publisher
- conectar el checkout con RabbitMQ
- loguear publicación

## Prompt para Codex

```text
Work only on branch feat/03-rabbitmq-contracts-and-publisher.

Goal:
Introduce RabbitMQ messaging contracts and publish the initial order event from checkout.

Requirements:
- Add shared message contracts for distributed order processing
- Add at least OrderSubmitted event
- Include CorrelationId, OrderId, CustomerId where appropriate
- Implement RabbitMQ publisher wiring
- Publish OrderSubmitted after a successful checkout command persists the order
- Add structured logging around message publishing
- Do not implement full worker consumption yet
- Keep the build working and tests updated

Deliver:
- code changes
- files changed
- event contract summary
- build/test results
```
