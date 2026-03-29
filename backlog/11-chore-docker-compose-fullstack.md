11) chore/11-docker-compose-fullstack

🧾 User story

As an evaluator, I want to run the full distributed order processing system using Docker Compose so that I can easily validate the platform, its architecture, and the event-driven workflow without manual setup.


---

🧠 Contexto

Esta historia convierte tu proyecto en un sistema ejecutable real.
No es solo “Dockerizar”, es demostrar:

arquitectura distribuida funcionando

comunicación entre servicios

event-driven workflow (RabbitMQ)

facilidad de ejecución (muy importante para evaluación)


👉 Esto impacta directamente en:

Docker Containerisation (10 marks)

Architecture & System Design (15 marks)

Integration (core del assignment)



---

🎯 Objetivo

Permitir levantar toda la plataforma con un solo comando:

docker-compose up --build

y que el evaluador pueda:

acceder a la API

ver RabbitMQ

comprobar flujo de órdenes

(ideal) ver UI(s)



---

✅ Criterios de aceptación

Mínimo (obligatorio)

Existe docker-compose.yml en la raíz

Se levantan correctamente:

RabbitMQ (con management UI)

Base de datos

Order API


Los servicios pueden comunicarse entre sí (network Docker)

La API arranca sin errores de configuración

Variables de entorno definidas claramente (no hardcoded)

No requiere pasos manuales complejos



---

Nivel medio (recomendado)

También se levantan:

Inventory Service

Payment Service

Shipping Service


Los servicios consumen/publican eventos correctamente

Health checks básicos en servicios críticos

Logs visibles desde contenedores



---

Nivel alto (diferencial / matrícula)

Se incluyen:

Blazor UI

React Admin Dashboard


URLs documentadas:

API

RabbitMQ UI

Frontends


Persistencia de datos (volumes)

Startup ordenado (depends_on + healthchecks)



---

🛠️ Tareas técnicas

1. Dockerfiles

Revisar cada proyecto:

API

Services (Inventory, Payment, Shipping)

Blazor

React (build + serve)


Asegurar:

WORKDIR correcto

dotnet publish usado

puertos expuestos




---

2. docker-compose.yml

Definir servicios:

rabbitmq (con management)

database (SQL Server / PostgreSQL / SQLite volume)

order-api

inventory-service

payment-service

shipping-service

blazor-ui (opcional pero recomendado)

react-admin (opcional)



---

3. Networking

Usar network común:


networks:
  app-network:

Usar nombres de servicio como host:

rabbitmq

database

order-api




---

4. Configuración

Mover config a env vars:

ConnectionStrings

RabbitMQ host

Ports

API base URLs


Ejemplo:

environment:
  - RabbitMQ__Host=rabbitmq
  - ConnectionStrings__Default=Server=database;...


---

5. Health checks (si puedes)

Ejemplo:

healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
  interval: 10s
  retries: 5


---

6. Volúmenes

Persistencia DB

(opcional) logs



---

7. Documentación mínima

En README:

cómo ejecutar

URLs clave:

API

RabbitMQ UI

Frontend(s)


credenciales si aplica



---

⚠️ Riesgos / errores típicos

❌ usar localhost entre contenedores → debe ser nombre del servicio

❌ puertos mal expuestos

❌ DB no accesible desde API

❌ RabbitMQ mal configurado

❌ frontend apuntando a localhost en vez de API container

❌ falta de variables de entorno



---

🤖 Prompt para Codex (versión PRO)

Work only on branch chore/11-docker-compose-fullstack.

Goal:
Provide a complete and practical Docker Compose setup to run the distributed order processing platform end-to-end for evaluation purposes.

Context:
- The system follows a distributed architecture:
  Blazor UI → API → RabbitMQ → Inventory → Payment → Shipping
- The goal is not just containerization, but making the system easy to run and validate.
- This is an integration task, not a redesign task.

In scope:
- docker-compose.yml
- Dockerfiles
- environment configuration
- inter-service communication
- startup reliability

Out of scope:
- redesigning architecture
- rewriting services
- changing CQRS/MediatR structure
- adding new business features

Requirements:

1. Core services must run:
   - RabbitMQ (with management UI)
   - Database (SQL Server / PostgreSQL / SQLite volume)
   - Order API

2. Preferably include:
   - Inventory Service
   - Payment Service
   - Shipping Service
   - Blazor UI
   - React Admin UI (if already present)

3. Networking:
   - Use Docker network
   - Services must communicate using service names (NOT localhost)

4. Configuration:
   - Move hardcoded config to environment variables
   - Ensure API can connect to DB and RabbitMQ using container names

5. Dockerfiles:
   - Fix build/publish stages if needed
   - Ensure correct ports are exposed
   - Ensure app starts correctly inside container

6. Startup reliability:
   - Use depends_on where useful
   - Add basic health checks if feasible
   - Avoid race conditions where possible

7. Developer experience:
   - System should run with:
     docker-compose up --build
   - Minimal manual steps required

8. Documentation:
   - Add minimal README section:
     - how to run
     - service URLs
     - assumptions

Constraints:
- Keep configuration simple and readable
- Do not overengineer orchestration
- Do not introduce Kubernetes or advanced infra
- Keep repository buildable
- Do not break existing CI/tests

Validation:
- All core containers start successfully
- API is reachable
- RabbitMQ UI is accessible
- No critical startup exceptions

Deliver:
1. code changes
2. files created/modified (docker-compose.yml, Dockerfiles, config)
3. list of services in compose
4. how to run the system
5. known limitations or assumptions

