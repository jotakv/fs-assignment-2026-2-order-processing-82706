# 6) `feat/06-shipping-service`

## User story

**As the shipping service, I want to create shipping details after payment approval so that successful orders can move toward completion.**

## Descripción

Tercer worker.

## Criterios de aceptación

- consume `PaymentApproved`
- genera referencia de envío
- estima fecha
- guarda `ShipmentRecord`
- publica `ShippingCreated` o `ShippingFailed`

## Tareas técnicas

- consumer de aprobación de pago
- generación de tracking/shipment reference
- persistencia
- publicación del evento

## Prompt para Codex

```text
Work only on branch feat/06-shipping-service.

Goal:
Implement the Shipping Service that consumes approved payments, creates shipping information, stores shipment data, and publishes the shipping outcome.

Requirements:
- Consume PaymentApproved
- Generate shipment reference
- Estimate dispatch date
- Persist ShipmentRecord
- Publish ShippingCreated or ShippingFailed
- Add structured logging with OrderId and CorrelationId
- Keep solution buildable
- Add or update tests for shipment creation logic

Deliver:
- code changes
- files changed
- shipping flow summary
- build/test results
```
