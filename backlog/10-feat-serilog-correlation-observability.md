# 10) `feat/10-serilog-correlation-observability`

## User story

**As a developer/evaluator, I want structured logs with correlation data so that I can trace the full lifecycle of each order.**

## Descripción

Esto te da puntos y además te ayuda a depurar todo.

## Criterios de aceptación

- Serilog en API y servicios
- logs en submit/publish/consume/payment/shipping/errors
- incluye `CorrelationId`
- incluye `OrderId`, `EventType`, `ServiceName`

## Tareas técnicas

- enriquecer logs
- propagar correlation id
- revisar sinks/config
- añadir logs útiles en cada servicio

## Prompt para Codex

```text
Work only on branch feat/10-serilog-correlation-observability.

Goal:
Implement structured logging with Serilog across the order API and worker services, including correlation-aware tracing of order flow.

Requirements:
- Add or complete Serilog configuration across API and worker services
- Log key actions:
  order submission, message publishing, message consumption, inventory validation, payment outcome, shipping creation, errors/exceptions
- Include contextual fields:
  OrderId, CustomerId where available, EventType, ServiceName, CorrelationId
- Ensure CorrelationId is propagated across message flow
- Keep logging structured and useful, not noisy
- Keep solution buildable

Deliver:
- code changes
- files changed
- logging summary
- build/test results
```
