# 0) `fix/00-solution-stabilization`

## User story

**As a developer, I want to stabilize the refactored solution so that I can continue implementing the distributed order platform without compounding structural errors.**

## Descripción

Esta rama no añade funcionalidad nueva. Solo deja la solución estable tras el refactor de arquitectura.

## Criterios de aceptación

- `dotnet restore` funciona
- `dotnet build` funciona
- la solución arranca sin errores críticos de DI/configuración
- los tests no fallan por referencias rotas o problemas estructurales
- no se introducen nuevas features todavía

## Tareas técnicas

- arreglar project references
- revisar `Program.cs`
- revisar DI registrations
- corregir namespaces/usings
- quitar dependencias muertas que rompan compilación
- adaptar tests dañados por el refactor

## Prompt para Codex

```text
Work only on branch fix/00-solution-stabilization.

Goal:
Stabilize the refactored solution so it builds and starts correctly before implementing more distributed order-processing features.

Context:
- The repository has already been refactored toward a cleaner architecture.
- There are still build errors, broken DI/setup issues, and incomplete migration leftovers from the old MVC-style code.
- Do not redesign the whole solution.
- Do not add RabbitMQ, React, Docker, or new business features yet unless strictly required to fix compilation/startup.

In scope:
- solution/project references
- Program.cs / startup wiring
- dependency injection
- broken namespaces/usings
- compile-time issues
- minimal cleanup of legacy code only when it blocks build
- test project references if broken due to refactor

Out of scope:
- new services
- RabbitMQ workflow
- frontend redesign
- Docker changes
- README changes

Requirements:
- Keep the current architecture direction
- Preserve CQRS/MediatR/AutoMapper structure if already present
- Fix the build incrementally
- Update broken tests instead of deleting them without replacement
- Run dotnet restore, dotnet build, and dotnet test
- Summarize all remaining known gaps after stabilization

Deliver:
1. code changes
2. files changed
3. remaining technical gaps
4. build/test results
```
