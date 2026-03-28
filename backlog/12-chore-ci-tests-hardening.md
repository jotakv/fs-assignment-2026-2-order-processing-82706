# 12) `chore/12-ci-tests-hardening`

## User story

**As a student, I want a stable CI pipeline with builds and tests so that I avoid rubric penalties and demonstrate engineering quality.**

## Descripción

Esto es crítica absoluta por la penalización.

## Criterios de aceptación

- GitHub Actions verde
- build correcto
- test correcto
- resultados visibles
- rutas actualizadas tras el refactor

## Tareas técnicas

- revisar `.github/workflows/ci.yml`
- ajustar solución/proyectos
- asegurar `restore/build/test`
- publicar artefactos o trx si quieres

## Prompt para Codex

```text
Work only on branch chore/12-ci-tests-hardening.

Goal:
Harden the CI workflow so GitHub Actions builds and tests the current refactored distributed solution correctly.

Requirements:
- Update workflow paths/projects after the architecture refactor
- Run dotnet restore, dotnet build, and dotnet test
- Keep test output visible and useful
- Preferably publish TRX or similar test artifacts
- Fix issues caused by project renames, solution changes, or broken paths
- Do not introduce unrelated refactors

Deliver:
- code changes
- files changed
- CI summary
- expected pipeline behavior
```
