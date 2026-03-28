# 5) `feat/05-payment-service`

## User story

**As the payment service, I want to process payment only after inventory is confirmed so that the order flow stays consistent and realistic.**

## Descripción

Segundo worker.

## Criterios de aceptación

- consume solo tras inventario correcto
- guarda `PaymentRecord`
- aprueba la mayoría
- rechaza algunas aleatoriamente
- rechaza tarjetas de prueba concretas
- publica `PaymentApproved` o `PaymentRejected`

## Tareas técnicas

- consumer de `InventoryConfirmed`
- simulación de pago
- reglas de rechazo
- persistencia del resultado
- publicación del evento

## Prompt para Codex

```text
Work only on branch feat/05-payment-service.

Goal:
Implement the Payment Service that consumes successful inventory events, simulates payment authorization, persists the result, and publishes the payment outcome.

Requirements:
- Consume InventoryConfirmed
- Persist PaymentRecord
- Approve most payments
- Randomly reject some payments in a controlled/testable way
- Reject specific test card numbers
- Publish PaymentApproved or PaymentRejected
- Add structured logging with correlation context
- Keep the build working
- Add or update tests for payment decision rules

Deliver:
- code changes
- files changed
- payment rules summary
- build/test results
```
