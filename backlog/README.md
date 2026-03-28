# Backlog por ramas

Este directorio contiene una historia de usuario por archivo Markdown, siguiendo el backlog propuesto y el orden recomendado de ejecución.

## Historias incluidas

1. `00-fix-solution-stabilization.md`
2. `01-feat-order-domain-state-model.md`
3. `02-feat-api-cqrs-foundation.md`
4. `03-feat-rabbitmq-contracts-and-publisher.md`
5. `04-feat-inventory-service.md`
6. `05-feat-payment-service.md`
7. `06-feat-shipping-service.md`
8. `07-feat-order-status-tracking.md`
9. `08-feat-blazor-customer-portal.md`
10. `09-feat-react-admin-dashboard.md`
11. `10-feat-serilog-correlation-observability.md`
12. `11-chore-docker-compose-fullstack.md`
13. `12-chore-ci-tests-hardening.md`
14. `13-docs-readme-architecture-runbook.md`

## Orden recomendado de ejecución

Hazlo así, sin saltos:

1. `fix/00-solution-stabilization`
2. `feat/01-order-domain-state-model`
3. `feat/02-api-cqrs-foundation`
4. `feat/03-rabbitmq-contracts-and-publisher`
5. `feat/04-inventory-service`
6. `feat/05-payment-service`
7. `feat/06-shipping-service`
8. `feat/07-order-status-tracking`
9. `feat/08-blazor-customer-portal`
10. `feat/09-react-admin-dashboard`
11. `feat/10-serilog-correlation-observability`
12. `chore/11-docker-compose-fullstack`
13. `chore/12-ci-tests-hardening`
14. `docs/13-readme-architecture-runbook`

## Cómo trabajar cada rama sin volverte loco

Usa siempre este ciclo:

```bash
git checkout main
git pull
git checkout -b <nombre-rama>
```

Luego:

1. pegas el prompt en Codex
2. revisas los cambios
3. ejecutas:

```bash
dotnet restore
dotnet build
dotnet test
```

4. pruebas manualmente lo mínimo
5. commit
6. merge

## Mi consejo práctico más importante

No empieces por RabbitMQ todavía si la solución no está firme.

Tu siguiente paso real debería ser:

**`fix/00-solution-stabilization`**

porque si build, DI o tests están medio rotos, luego no sabrás si el problema es:

- tu arquitectura,
- RabbitMQ,
- MediatR,
- el worker,
- o simplemente una referencia mal puesta.

## Qué te puedo ir guiando contigo

Podemos hacerlo rama por rama.
La mejor siguiente pieza es que me pegues el estado actual de:

- estructura de proyectos actual
- errores de `dotnet build`
- y, si quieres, el `Program.cs` de la API principal

y te preparo **el prompt exacto para tu rama `fix/00-solution-stabilization` adaptado a tu repo actual**.
