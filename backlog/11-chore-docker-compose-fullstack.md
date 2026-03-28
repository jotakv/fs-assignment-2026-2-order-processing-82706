# 11) `chore/11-docker-compose-fullstack`

## User story

**As an evaluator, I want to run the distributed solution with Docker Compose so that I can test the platform easily.**

## Descripción

Aquí conviertes el proyecto en entregable real.

## Criterios de aceptación

- `docker-compose.yml` levanta mínimo:
  - RabbitMQ
  - API
  - DB
- idealmente también workers y frontends
- variables y puertos claros

## Tareas técnicas

- revisar Dockerfiles
- añadir servicios al compose
- health checks si puedes
- conexión entre contenedores
- documentación mínima de arranque

## Prompt para Codex

```text
Work only on branch chore/11-docker-compose-fullstack.

Goal:
Provide Docker Compose support for the distributed order processing platform.

Requirements:
- Ensure docker-compose can run at least:
  RabbitMQ, Database, Order API
- Preferably also run:
  Inventory Service, Payment Service, Shipping Service, Blazor UI, React Admin UI
- Fix Dockerfiles/startup paths as needed
- Keep configuration practical and understandable
- Do not overengineer orchestration
- Keep repository buildable and document runtime assumptions

Deliver:
- code changes
- files changed
- compose service summary
- build/startup notes
```
