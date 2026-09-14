# Kiwbi - Architecture & Tech Stack

## 1. Stack Tecnológico
- **Backend:** .NET 10, C#.
- **Framework Web:** ASP.NET Core MVC.
- **Frontend / Interactividad:** Razor Views, HTMX (para dinamismo sin recargar página) y Alpine.js (para estados de UI simples en cliente, modales, toggles). NO se usan frameworks SPA pesados.
- **Base de Datos:** PostgreSQL.
- **ORM:** Entity Framework (EF) Core (Enfoque Code-First).
- **Autenticación:** ASP.NET Core Identity nativo (tablas en PostgreSQL).

## 2. Patrón Arquitectónico: Clean Architecture
La solución se divide estrictamente en 4 capas. **Regla de oro: Las dependencias siempre apuntan hacia el interior (Domain).**

- **1. Kiwbi.Domain:** El núcleo puro. No tiene dependencias de ningún framework externo (ni de EF Core). Contiene:
  - Entidades, Enums, Value Objects.
  - Excepciones de dominio.
  - Interfaces de Repositorios (`IRepository`).
- **2. Kiwbi.Application:** Casos de uso de negocio. Depende solo de Domain. Contiene:
  - Servicios de aplicación, DTOs, Mappers (opcional).
  - Lógica orquestadora (ej. `AssignHomeToUserUseCase`).
- **3. Kiwbi.Infrastructure:** Implementación técnica. Depende de Application y Domain. Contiene:
  - `KiwbiDbContext` (Configuraciones de EF Core, migraciones).
  - Implementación de los repositorios.
  - Servicios externos (Envío de correos para Magic Links, exportación a PDF/Excel).
  - Configuración de ASP.NET Identity.
- **4. Kiwbi.Web:** Capa de presentación MVC. Depende de Application e Infrastructure (solo para inyección de dependencias en `Program.cs`). Contiene:
  - Controllers, Views (Razor), ViewModels.
  - Middlewares, HTMX endpoints.

## 3. Reglas de Inyección de Dependencias
- Todos los servicios deben registrarse en extensiones del contenedor (ej. `DependencyInjection.cs` en cada capa).
- Los Controllers MVC solo deben interactuar con las interfaces de la capa `Application` (Servicios/Handlers), NUNCA interactúan con `DbContext` directamente.