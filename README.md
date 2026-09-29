# Kiwbi

**Kiwbi** es una aplicación web B2B2C para el sector inmobiliario que permite a las **promotoras** gestionar y centralizar la personalización de acabados de viviendas en construcción, y a los **compradores** consultar y elegir esas opciones online antes de una fecha límite por gremio. Sustituye la gestión manual por Excel y cadenas de correo por un flujo digital trazable, con exportación final de un "Libro de Obra" agrupado por gremio para la constructora.

Este repositorio es el Trabajo Fin de Máster de Christian Granados. Ver [`LICENSE`](LICENSE) para las condiciones de uso.

- **Demo pública en producción:** https://kiwbi.onrender.com
- **Presentación del proyecto:** [Google Slides](https://docs.google.com/presentation/d/1UIgTd1C71db1TOWYfe97wVyZ5NRCqsPpyXbaDNkBB58/edit?usp=sharing)
- **Video de presentación:** [Ver video](https://docs.google.com/videos/d/1oydGH4_pvtWI_mqAyHB1dr-_C8U6ER--KCwVHUeT2TY/play?usp=sharing)
- **Documentación funcional/técnica ampliada:** carpeta [`.ai-docs/`](.ai-docs/)

| Rol | Email | Contraseña |
|---|---|---|
| Promotora | `demo@kiwbi.test` | `DemoKiwbi!2026` |
| Comprador | `buyer1@kiwbi.test` (también `buyer2`/`3`/`4`) | `DemoBuyer!2026` |

---

## 1. Descripción general

Kiwbi resuelve un problema concreto del proceso de preventa de vivienda: durante la construcción de un edificio, cada comprador debe elegir acabados (suelos, azulejos, grifería, carpintería...) dentro de unos plazos que marca cada gremio de la obra. Hoy esto se gestiona típicamente a mano, por email y hojas de cálculo, con alto riesgo de errores y pérdida de trazabilidad.

La aplicación modela dos roles:

- **Promotora (admin del tenant):** da de alta sus promociones (edificios), tipologías y viviendas; define los gremios de la obra con su fecha límite de selección; crea las personalizaciones y sus opciones (con sobrecoste y opción por defecto); invita a los compradores por email (Magic Link, sin gestión de contraseñas por su parte); revisa el progreso de elecciones por vivienda, confirma/marca como pagadas manualmente, y exporta el libro de obra en Excel/PDF agrupado por gremio.
- **Comprador:** accede mediante el enlace de invitación, ve únicamente las viviendas vinculadas a su cuenta, consulta las personalizaciones agrupadas por gremio con sus opciones y sobrecostes, y selecciona sin recargar página (HTMX). Pasada la fecha límite de un gremio, sus personalizaciones quedan bloqueadas en modo solo lectura.

El **branding** (logo y colores) de cada promotora se aplica automáticamente en su propio panel y en el portal de sus compradores.

## 2. Stack tecnológico

| Capa | Tecnología |
|---|---|
| Backend | .NET 10 / C#, ASP.NET Core MVC |
| Frontend | Razor Views + Bootstrap 5 (sistema de diseño propio), HTMX (interacciones sin recarga), vanilla JS puntual |
| Base de datos | PostgreSQL 16, EF Core (Code-First) |
| Autenticación | ASP.NET Core Identity |
| Generación de reportes | ClosedXML (Excel), QuestPDF (PDF) |
| Almacenamiento de ficheros | Cloudflare R2 (S3-compatible) en producción, disco local en desarrollo |
| Envío de correo | Brevo (SMTP) en producción, logging en desarrollo |
| Tests | xUnit, NSubstitute, FluentAssertions |
| CI/CD | GitHub Actions (build + test + `dotnet format`), CodeQL, SonarCloud |
| Contenedor / despliegue | Docker, Render (hosting), Neon (Postgres gestionado) |

## 3. Arquitectura y estructuración del repositorio

La solución sigue **Clean Architecture** en 4 capas, con las dependencias apuntando siempre hacia el dominio:

```
Kiwbi.Web  →  Kiwbi.Application  →  Kiwbi.Domain
Kiwbi.Infrastructure  →  Kiwbi.Application, Kiwbi.Domain
```

- **`Kiwbi.Domain`** — núcleo puro, sin dependencias externas: entidades, value objects, enums, excepciones de dominio e interfaces de repositorio.
- **`Kiwbi.Application`** — casos de uso (un `UseCase` por operación de negocio), DTOs, puertos (`IFileStorageService`, `IEmailSender`, `ICurrentUser`...). Depende solo de `Domain`.
- **`Kiwbi.Infrastructure`** — implementación técnica: `KiwbiDbContext`/migraciones EF Core, repositorios, adaptadores de Identity, almacenamiento (local/S3) y email (logging/SMTP), generación de reportes.
- **`Kiwbi.Web`** — capa de presentación MVC: Controllers, Views, ViewModels, componentes de vista (sidebar, branding por tenant).

```
src/
  Kiwbi.Domain/          Entidades y reglas de negocio puras
  Kiwbi.Application/     Casos de uso, DTOs, puertos
  Kiwbi.Infrastructure/  EF Core, repositorios, storage, email, reportes
  Kiwbi.Web/             Controllers, Views, wwwroot (CSS/JS del sistema de diseño)
tests/
  Kiwbi.Domain.Tests/
  Kiwbi.Application.Tests/
  Kiwbi.Web.Tests/
tools/                   Scripts de diagnóstico puntuales (no forman parte de la app)
.ai-docs/                Documentación de negocio, arquitectura y planificación por Epics
.github/workflows/       CI (build+test+format), CodeQL, SonarCloud
Dockerfile, docker-compose.yml
```

## 4. Funcionalidades implementadas

- **Gestión de la promotora:** perfil, marca (logo/colores), panel de resumen con indicadores agregados (promociones, viviendas, compradores vinculados, invitaciones pendientes).
- **Catálogo inmobiliario:** CRUD de promociones, tipologías y viviendas (con plano/imagen), estados comerciales (Disponible/Reservada/Vendida).
- **Motor de personalización:** CRUD de gremios (con fecha límite), personalizaciones asignables a toda la promoción, a una tipología o a viviendas concretas, y sus opciones (con sobrecoste y opción por defecto).
- **Onboarding del comprador:** invitación por Magic Link con expiración, aceptación con creación/vinculación de cuenta, reenvío/cancelación de invitaciones, desvinculación de un comprador de una vivienda.
- **Portal del comprador:** listado de viviendas propias, detalle con personalizaciones agrupadas por gremio, selección interactiva vía HTMX, bloqueo automático tras la fecha límite del gremio.
- **Gestión y exportación:** panel de progreso de elecciones por promoción/vivienda, confirmación y marcado de pago manual, exportación a Excel y PDF agrupada por gremio.
- **Cuenta y seguridad:** registro/login de promotora, recuperación de contraseña (promotora y comprador), landing pública con marca propia de Kiwbi.
- **Calidad y despliegue:** batería de tests automatizados (xUnit), CI en GitHub Actions, análisis estático (CodeQL + SonarCloud), contenedorización con Docker y despliegue continuo en Render.

Para el detalle epic a epic, ver [`.ai-docs/05-development-roadmap.md`](.ai-docs/05-development-roadmap.md).

## 5. Instalación y ejecución

### 5.1. Ver la demo ya desplegada

La aplicación está publicada y operativa en **https://kiwbi.onrender.com** (Render + Neon Postgres + Cloudflare R2 + Brevo). No requiere ninguna instalación.

> Nota: al estar en el plan gratuito de Render, la instancia "duerme" tras ~15 minutos de inactividad; la primera petición tras el reposo puede tardar unos segundos en responder.

### 5.2. Clonar y ejecutar en local

**Requisitos previos:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para levantar PostgreSQL fácilmente; también sirve cualquier instancia de PostgreSQL propia)

No hace falta ninguna cuenta ni credencial de la nube (Cloudflare R2, Brevo) para ejecutar la app en local: si no se configuran, la app usa automáticamente disco local para ficheros y un adaptador de logging para email.

**Pasos:**

```powershell
# 1. Clonar el repositorio
git clone https://github.com/Christian-Granados/Kiwbi.git
cd Kiwbi

# 2. Levantar PostgreSQL en un contenedor local (puerto 5433, ya configurado en appsettings.json)
docker-compose up -d

# 3. Ejecutar la aplicación (aplica las migraciones de EF Core automáticamente al arrancar)
dotnet run --project src/Kiwbi.Web

# La consola mostrará la URL real, normalmente:
#   http://localhost:5101
```

**(Opcional) Cargar datos de demo** para no partir de una base de datos vacía:

```powershell
dotnet run --project src/Kiwbi.Web -- --seed-demo
```

Esto crea un tenant "Kiwbi Demo" con 3 promociones en distintos estados de avance. Credenciales (ver detalle completo en [`.ai-docs/demo-data-guide.md`](.ai-docs/demo-data-guide.md)):

| Rol | Email | Contraseña |
|---|---|---|
| Promotora | `demo@kiwbi.test` | `DemoKiwbi!2026` |
| Comprador | `buyer1@kiwbi.test` (también `buyer2`/`3`/`4`) | `DemoBuyer!2026` |

El comando de seed es idempotente: puede ejecutarse varias veces sin dejar duplicados (borra y recrea el tenant de demo).

### 5.3. Ejecutar los tests

```powershell
dotnet test Kiwbi.slnx
```

Actualmente hay 327 tests automatizados (Domain, Application y Web) pasando en verde.

### 5.4. Modo desarrollo con recarga en caliente

```powershell
dotnet watch run --project src/Kiwbi.Web
```

## 6. Despliegue e infraestructura

| Servicio | Rol | Proveedor |
|---|---|---|
| Hosting | Contenedor Docker, auto-deploy en cada push a `main` | [Render](https://render.com) |
| Base de datos | PostgreSQL gestionado | [Neon](https://neon.tech) |
| Almacenamiento de ficheros | Planos e imágenes de logo (S3-compatible) | [Cloudflare R2](https://developers.cloudflare.com/r2/) |
| Envío de email | Invitaciones y recuperación de contraseña (SMTP) | [Brevo](https://www.brevo.com) |
| CI | Build + tests + formato en cada push/PR | GitHub Actions |
| Calidad de código | Análisis estático de seguridad y code smells | CodeQL + SonarCloud |

Todo el stack de publicación se mantiene en el plan gratuito de cada proveedor (coste $0).

## 7. Licencia

Este proyecto se publica exclusivamente con fines de evaluación académica. Ver [`LICENSE`](LICENSE) para el detalle completo de las condiciones de uso.
