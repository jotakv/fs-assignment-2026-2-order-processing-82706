
## 🚀 Guía de Arquitectura para CA2: Blazor + API + CQRS

Este es el núcleo de lo que tu profesor espera que entiendas (y reutilices) para tu entrega. No se trata de inventar algo nuevo, sino de aplicar este patrón profesional.

---

### 🧠 1. Lo que REALMENTE te están diciendo
Tu profesor no quiere que inventes una arquitectura desde cero. Su mensaje es:

> 👉 *"Usa este proyecto como plantilla de referencia para una arquitectura limpia usando **Blazor + API + CQRS (MediatR)**"*

**Tu objetivo:**
1. Entender cómo se conectan las piezas.
2. Reutilizar la misma estructura en tu tarea.

---

### 🧱 2. La Arquitectura (Simplificada)
Este es el modelo mental que necesitas seguir:



* **Blazor UI (Frontend)**
    * ↓
* **API Controller**
    * ↓
* **Mediator (MediatR)**
    * ↙ &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ↘
    * **Query** (Lectura) &nbsp;&nbsp; **Command** (Escritura)
    * ↓
* **Handlers** (Lógica de negocio)
    * ↓
* **Services / Repositories**
    * ↓
* **Database**

---

### 🔍 3. ¿Qué hace cada parte? (En lenguaje sencillo)

#### 🟣 Blazor (Frontend)
* Páginas como `Index.razor`.
* Inyectas servicios en la parte superior.
* Llamas a la API para cargar datos (productos, órdenes, etc.).
* **Lo que viste:** *"injections at the top... code loading products"*. Es el patrón estándar de Blazor.

#### 🔵 API Controllers
El controlador no hace casi nada, solo reenvía la solicitud al Mediator:
```csharp
[HttpGet]
public async Task<IActionResult> GetProducts()
{
    var result = await _mediator.Send(new GetProductsQuery());
    return Ok(result);
}
```

#### 🟡 CQRS (Commands & Queries)
| Tipo | Propósito | Ejemplo |
| :--- | :--- | :--- |
| **Query** | Leer datos | `GetProductsQuery` |
| **Command** | Cambiar datos | `CreateOrderCommand` |

#### 🟢 Mediator (La "Magia")
`await _mediator.Send(new GetProductsQuery());`
El Mediator decide qué **Handler** debe ejecutar esa tarea. Ya no llamas a los servicios directamente.

#### 🔴 Handlers
Aquí es donde vive la lógica. El Handler recibe la orden del Mediator y habla con el repositorio.

#### ⚫ AutoMapper
Se usa para convertir objetos de base de datos a objetos de vista:
* `Entity` → `DTO`
* Ejemplo: `_mapper.Map<ProductDto>(product);`

---

### ⚙️ 4. Lo que TÚ necesitas aprender (Importante)
Como mencionaste que esto es nuevo para ti, este es tu **gap de conocimiento**:

🔥 **DEBES entender:**
* **MediatR (CQRS):** Cómo fluyen las consultas/comandos.
* **Flujo:** Controller → Mediator → Handler.

---

### 🧪 5. Estrategia recomendada por el profesor
Él te dio dos caminos:
* **Opción A (La más rápida ✅):** Copia esta arquitectura y adáptala.
* **Opción B:** Pídele a una IA: *"Genera una arquitectura Blazor + API + CQRS (MediatR) con AutoMapper y caching"*.

---

### 🧩 6. Conexión con tu asignación (CA2)
Para obtener los **10 puntos de "Code Quality & Architecture"**, necesitas esto. Ya tienes:
* Actualizar .NET ✅
* Logging (Serilog) ✅
* Stripe ✅
* CI (Integración Continua) ✅
* **EXTRA:** Aplicar CQRS + Mediator te garantiza la máxima nota en arquitectura.

---

### 🚀 7. Qué hacer HOY (Paso a paso)

1.  **Explora el proyecto de ejemplo:** Revisa las páginas de Blazor, los controladores y la carpeta `Features` (donde suelen estar los queries/commands).
2.  **Rastrea un flujo completo:** Mira qué pasa desde que haces clic en "Cargar productos" en Blazor hasta que llega a la base de datos.
3.  **Crea un ejemplo pequeño:** Intenta replicar un `GetOrdersQuery` y su respectivo Handler.
4.  **Integra en CA2:** Usa esta estructura para Órdenes, Pagos y Productos.

---

### 🧠 8. Insight Clave para la nota máxima
La mayoría de los estudiantes:
* ❌ Pondrán la lógica en los controladores.
* ❌ Saltarán CQRS.
* ❌ Mezclarán todo en una sola capa.

Si tú haces esto, estarás fácilmente en el **70%+ (First Class)**.

---

### 🧭 9. Sobre Aspire (Opcional)
Él mencionó que **Aspire** orquesta múltiples APIs. Eso es para sistemas distribuidos avanzados.
* Para tu tarea: **No es requerido**.
* Es un **bono extra** si decides explorarlo después.

---

### 🎯 Conclusión
No estás construyendo al azar. Estás reconstruyendo tu proyecto usando una **plantilla de arquitectura moderna y profesional**.


# Assignment Descriptor

**Module:** Full Stack Development – Semester 2, Assignment 2  
**Weighting:** See Moodle for weighting  
**Release and Submission Dates:** As per Moodle  

---

## 1. Overview

You previously upgraded a Shopping Cart application from .NET 6 to .NET 10. Your task is to extend this system into a distributed order processing platform.

When a customer checks out their cart, the system must process the order asynchronously using RabbitMQ messaging and multiple backend services.

The system must expose two separate user interfaces:

1. **Customer Portal (Blazor)** – used by customers to browse products and place orders. This requires you taking the existing MVC project and converting it to Blazor or Razor Pages (i.e. full screen interactivity rather than MVC style POST requests).
2. **Admin Dashboard (React or Angular)** – used by administrators to monitor and manage orders.

The platform must simulate a real-world event-driven order fulfillment workflow including:

- order submission
- inventory validation
- payment processing
- shipment creation
- order completion or failure

The focus of the assignment is integration, architecture, testing and system design, not UI styling.

You must have all APIs in the same solution as different projects and your GitHub Action should show testing metrics and successful builds. Failure to have a working GitHub Action will mean marking allocation is reduced by **30% per rubric item**.

---

## 2. Learning Outcomes Assessed

On successful completion of this module, students will be able to:

1. **Design full stack application architectures**  
   Design and implement distributed full stack applications integrating frontend interfaces, backend APIs, databases, and messaging infrastructure.

2. **Develop backend services using modern .NET technologies**  
   Build and extend RESTful APIs using .NET, implementing appropriate application structure, data access, and domain models.

3. **Implement asynchronous processing using messaging systems**  
   Design event-driven workflows using messaging technologies such as RabbitMQ to support asynchronous system operations.

4. **Develop and integrate multiple frontend applications**  
   Create user interfaces using modern frameworks such as Blazor and React or Angular that interact with shared backend services.

5. **Apply modern software engineering practices**  
   Apply architectural patterns, structured logging, and containerisation technologies to improve system maintainability, scalability, and deployment.

---

## 3. System Architecture Overview

The system should resemble the following event-driven workflow:

```text
Customer Checkout
    │
    ▼
Order Management API
    │
    ▼
RabbitMQ Event Published
    │
    ▼
Inventory Service
    │
    ▼
Payment Service
    │
    ▼
Shipping Service
    │
    ▼
Order Completed / Failed
````

Each step in the workflow should publish events and update order status.

---

## 4. Core System Components

Students must implement the following components.

### 4.1 Order Management API (.NET 10)

This API acts as the central system entry point.

**Responsibilities include:**

* creating orders from the shopping cart
* storing order information
* publishing events to RabbitMQ
* exposing endpoints used by both frontends
* tracking order progress
* updating order state when downstream services complete processing

**Example Endpoints**

* `POST /api/orders/checkout`
* `GET /api/orders`
* `GET /api/orders/{id}`
* `GET /api/orders/{id}/status`
* `GET /api/products`
* `GET /api/customers/{id}/orders`

---

### 4.2 RabbitMQ Messaging

RabbitMQ must be used to coordinate asynchronous order processing.

Students must define message contracts shared between services.

The solution now reads broker settings from the `RabbitMq` configuration section in the API and worker `appsettings.json` files. For local development, keep the tracked JSON structure and override the secret with `RabbitMq__Password` or the full AMQPS connection string with `RabbitMq__Uri`. When `RabbitMq__Uri` is provided it takes precedence over the individual host, port, username, password, and virtual host fields.

**Example Events**

* `OrderSubmitted`
* `InventoryCheckRequested`
* `InventoryCheckCompleted`
* `PaymentProcessingRequested`
* `PaymentProcessed`
* `ShippingRequested`
* `ShippingCreated`
* `OrderCompleted`
* `OrderFailed`

Each stage of the order pipeline should produce an event.

---

### 4.3 Inventory Service

Consumes order submission events and verifies stock availability.

**Responsibilities:**

* check requested product quantities
* simulate stock reservation
* publish success or failure event

**Possible outcomes:**

* `InventoryConfirmed`
* `InventoryFailed`

---

### 4.4 Payment Service

Simulates payment processing.

**Responsibilities:**

* receive inventory success event
* simulate payment authorization
* publish payment outcome

**Example behaviour:**

* approve most payments
* randomly reject some payments
* reject specific test card numbers

**Possible outcomes:**

* `PaymentApproved`
* `PaymentRejected`

---

### 4.5 Shipping Service

Creates shipment information for successful orders.

**Responsibilities:**

* receive payment approval
* generate shipment reference
* estimate dispatch date
* publish shipping event

**Possible outcomes:**

* `ShippingCreated`
* `ShippingFailed`

---

## 5. Order State Model

Orders should move through defined states.

**Example lifecycle:**

* `Cart`
* `Submitted`
* `InventoryPending`
* `InventoryConfirmed`
* `InventoryFailed`
* `PaymentPending`
* `PaymentApproved`
* `PaymentFailed`
* `ShippingPending`
* `ShippingCreated`
* `Completed`
* `Failed`

Students may adapt this model if necessary but must clearly track state transitions.

---

## 6. Frontend Applications

Two separate frontends must be developed.

### 6.1 Customer Portal – Blazor

This application represents the customer-facing interface.

**Minimum features:**

* view products
* add products to cart
* view cart
* checkout
* view previous orders
* track order status

**Example pages:**

* Product Listing
* Product Details
* Shopping Cart
* Checkout
* Order Confirmation
* My Orders
* Order Tracking

---

### 6.2 Admin Dashboard – React or Angular

This application represents the administration and operations interface.

**Minimum features:**

* list all orders
* filter orders by status
* view order details
* view payment status
* view inventory results
* identify failed orders

**Example pages:**

* Orders Dashboard
* Orders Table
* Order Details
* Failed Orders

The admin interface should expose more operational data than the customer portal.

---

## 7. Logging Requirements

Students must implement **Serilog structured logging**.

Important actions must be logged, including:

* order submission
* message publishing
* message consumption
* inventory validation
* payment outcome
* shipping creation
* errors and exceptions

Logs should include contextual data such as:

* `OrderId`
* `CustomerId`
* `EventType`
* `ServiceName`
* `CorrelationId`

---

## 8. Docker Requirements

The full solution must run with at least two services using Docker Compose. One of these services needs to be **RabbitMQ**.

**Minimum services expected (in bold):**

* Order API
* **RabbitMQ**
* Database
* Inventory Service
* Payment Service
* Shipping Service
* Blazor UI
* React/Angular UI

Students must provide a `docker-compose.yml` file that launches the entire system.

---

## 9. Database

Students may use one of the following:

* SQL Server
* PostgreSQL
* SQLite

**Minimum required entities:**

* Products
* Customers
* Orders
* OrderItems
* InventoryRecords
* PaymentRecords
* ShipmentRecords

The schema should be kept manageable.

---

## 10. Documentation Requirements

Students must include a `README` explaining:

* system architecture
* event flow
* how to run the solution
* service responsibilities
* assumptions and limitations

---

## 11. Assessment Breakdown

| Category                        |         Marks |
| ------------------------------- | ------------: |
| Architecture and System Design  |            15 |
| RabbitMQ Event Workflow         |            15 |
| Backend API Implementation      |            10 |
| Blazor Customer Application     |            10 |
| React/Angular Admin Application |            10 |
| Logging with Serilog            |            10 |
| Docker Containerisation         |            10 |
| Documentation                   |            10 |
| CQRS with MediatR               |            15 |
| AutoMapper Usage                |             5 |
| **Total**                       | **100 marks** |

---

## 12. Advanced Architecture Marks (20 Marks)

Students can earn up to 20 marks for implementing additional architectural patterns.

Without these enhancements, the maximum achievable mark will realistically be **80%**.

### 12.1 CQRS with MediatR (15 Marks)

Students may implement **Command Query Responsibility Segregation (CQRS)** using **MediatR**.

Marks will be awarded where:

* commands and queries are clearly separated
* controllers remain thin
* business logic resides in handlers
* handlers encapsulate domain operations
* the architecture improves maintainability

**Example commands:**

* `CheckoutOrderCommand`
* `CancelOrderCommand`
* `ProcessInventoryResultCommand`
* `ProcessPaymentResultCommand`
* `CreateShipmentCommand`

**Example queries:**

* `GetOrderByIdQuery`
* `GetOrdersQuery`
* `GetCustomerOrdersQuery`
* `GetOrdersByStatusQuery`
* `GetDashboardSummaryQuery`

Full marks require a clear CQRS structure with meaningful separation of responsibilities.

---

### 12.2 AutoMapper Usage (5 Marks)

Students may use **AutoMapper** for object mapping between:

* entities
* DTOs
* API responses
* message contracts

Marks will be awarded where:

* DTO mapping is handled cleanly
* mapping profiles are organised properly
* controllers are not cluttered with manual mapping code

AutoMapper should be used where appropriate, not excessively.

## React admin dashboard

A separate React admin dashboard now lives in `admin-dashboard/`.

Run it locally with:

```bash
cd admin-dashboard
npm install
npm run dev
```

Build it with:

```bash
cd admin-dashboard
npm run build
```

By default it calls the API at `https://localhost:5001/`. Override with `VITE_API_BASE_URL` if needed.
