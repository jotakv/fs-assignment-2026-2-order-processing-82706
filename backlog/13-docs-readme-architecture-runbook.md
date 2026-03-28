# 13) `docs/13-readme-architecture-runbook`

## User story

**As an evaluator, I want a clear README so that I can understand the system architecture, event flow, and how to run the solution.**

## Descripción

Cierra la entrega y también te ayuda para el report.

## Criterios de aceptación

- arquitectura explicada
- flujo de eventos explicado
- cómo ejecutar local
- cómo ejecutar con Docker
- responsabilidades por servicio
- supuestos y limitaciones

## Tareas técnicas

- README estructurado
- diagrama Mermaid si puedes
- pasos de ejecución
- credenciales/datos de prueba
- ejemplo de pedido exitoso y fallido

## Prompt para Codex

```text
Work only on branch docs/13-readme-architecture-runbook.

Goal:
Write a high-quality README that explains the distributed order processing platform and how to run it.

Requirements:
- Explain the system architecture
- Explain the event flow from checkout to completion/failure
- Describe each service responsibility
- Add local run instructions
- Add Docker Compose run instructions
- Add assumptions and limitations
- Add test/demo notes such as sample failure cases and successful order flow
- Keep the README aligned with the actual codebase

Deliver:
- README changes
- summary of new sections
```
