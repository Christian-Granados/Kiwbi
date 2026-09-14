# Epic 01: Foundation & Promotora Tenant

## Estado

- Estado: Pendiente de aprobación técnica.
- Regla de ejecución: No se escribirá código fuente de este Epic hasta recibir la aprobación expresa de este documento.

## Objetivos

Establecer la base técnica y funcional del MVP para que una promotora pueda registrarse, autenticarse y gestionar los datos básicos de su tenant.

El Epic cubre las siguientes Features del roadmap:

- **Feature 1.1 - Setup del proyecto:** Confirmar la configuración de Clean Architecture, EF Core, PostgreSQL y ASP.NET Core Identity.
- **Feature 1.2 - Autenticación y tenant:** Permitir el alta, inicio y cierre de sesión de administradores de promotora, y enlazar cada administrador con su tenant.
- **Feature 1.3 - Dashboard de promotora:** Ofrecer un área autenticada para consultar y actualizar el perfil y branding básico de la promotora.

Quedan explícitamente fuera de alcance las promociones, tipologías, viviendas, gremios, personalizaciones, opciones, invitaciones Magic Link, compradores, elecciones y exportaciones. Se desarrollarán en los Epics 2 a 6.

## Análisis Técnico

### Modelo de dominio

El único agregado de negocio que se incorporará en este Epic es `DeveloperCompany`, nombre técnico en inglés para la entidad de negocio Promotora. Será la raíz del tenant y, en Epics posteriores, será propietaria de las promociones.

| Elemento | Tipo | Responsabilidad | Propiedades iniciales |
| --- | --- | --- | --- |
| `DeveloperCompany` | Entidad / raíz de agregado | Representa a la promotora y delimita el tenant. | `Id`, `Name`, `Branding`, `CreatedAtUtc`, `UpdatedAtUtc` |
| `Branding` | Value Object | Agrupa la configuración visual básica de la promotora. | `LogoPath` opcional, `PrimaryColor`, `SecondaryColor` opcional |
| `BrandColor` | Value Object | Garantiza una representación válida y canónica de un color de marca. | `Value` en formato hexadecimal `#RRGGBB` |

`DeveloperCompany` expondrá comportamiento explícito en vez de permitir mutaciones arbitrarias: `Create`, `Rename` y `UpdateBranding`. Las validaciones de nombre y color vivirán en el dominio. Los errores de reglas de negocio que lleguen a los flujos de aplicación se convertirán en `Result` o `Result<T>`; no se usarán excepciones como control de flujo.

### Identidad y relación con el tenant

`ApplicationUser` seguirá siendo una clase de Infrastructure que extiende `IdentityUser`; no es una entidad del dominio. Se ampliará con `DeveloperCompanyId` nullable y, de forma técnica, una navegación opcional a `DeveloperCompany`.

Durante el alta de una administradora:

1. El caso de uso crea la `DeveloperCompany`.
2. El adaptador de Identity crea el `ApplicationUser`.
3. Se vincula el usuario con `DeveloperCompanyId`.
4. Se asigna el rol `DeveloperAdmin`.
5. La operación se confirma atómicamente.

El vínculo se mantiene nullable para no imponer a los compradores futuros el mismo modelo: el Epic 4 añadirá su asociación a viviendas. Los casos de uso administrativos resolverán siempre el tenant desde el usuario autenticado, no desde un identificador enviado por navegador.

### Relaciones y esquema relacional

| Tabla | Procedencia | Columnas relevantes | Relaciones |
| --- | --- | --- | --- |
| `developer_companies` | Dominio / EF Core | `id`, `name`, `logo_path`, `primary_color`, `secondary_color`, `created_at_utc`, `updated_at_utc` | Un tenant tiene cero o más usuarios Identity asociados. |
| `AspNetUsers` | ASP.NET Core Identity | columnas estándar de Identity y `developer_company_id` nullable | Cada usuario pertenece opcionalmente a una promotora. |
| `AspNetRoles` | ASP.NET Core Identity | columnas estándar | Contendrá el rol `DeveloperAdmin`. |
| `AspNetUserRoles` | ASP.NET Core Identity | claves estándar | Asigna `DeveloperAdmin` a los usuarios administradores. |

EF Core configurará una clave foránea opcional desde `AspNetUsers.developer_company_id` a `developer_companies.id`, con borrado restringido. `Branding` se persistirá como propiedades de la tabla del agregado para evitar una tabla artificial para un Value Object sin identidad.

`LogoPath` almacenará una ruta o clave de almacenamiento, nunca el archivo binario. La infraestructura de carga concreta se introducirá solo si es necesaria para completar el branding de este Epic; su contrato quedará aislado de la capa Domain.

## Impacto en Arquitectura

### Kiwbi.Domain

Se crearán:

- `DeveloperCompany` en un módulo `Developers`.
- Los Value Objects `Branding` y `BrandColor`.
- `IDeveloperCompanyRepository`, con consultas específicas del agregado que no encajan en el repositorio genérico actual.

El proyecto no referenciará EF Core ni Identity.

### Kiwbi.Application

Se crearán contratos y casos de uso organizados por intención:

- `RegisterDeveloper`.
- `GetCurrentDeveloperProfile`.
- `UpdateDeveloperProfile`.
- `UpdateDeveloperBranding`.
- Contratos de entrada/salida (commands y DTOs) por caso de uso.
- `ICurrentUser` para identificar al usuario autenticado.
- `IAccountProvisioningService` como puerto para crear y administrar usuarios Identity.
- `IUnitOfWork` para confirmar operaciones transaccionales sin depender de EF Core.

Todos los casos de uso retornarán `Result` o `Result<T>`, recibirán `CancellationToken` y serán registrados desde `AddApplicationServices`.

### Kiwbi.Infrastructure

Se implementarán los adaptadores técnicos:

- Ampliación de `ApplicationUser` con el vínculo al tenant.
- Configuración de EF Core para `DeveloperCompany` en `Persistence/Configurations`.
- Registro `DbSet<DeveloperCompany>` en `KiwbiDbContext`.
- `DeveloperCompanyRepository` e implementación de `IUnitOfWork` basados en EF Core.
- `IdentityAccountProvisioningService`, que encapsula `UserManager<ApplicationUser>`, `RoleManager<IdentityRole>` y la asignación de roles.
- Registro de todos los adaptadores desde `AddInfrastructureServices`.
- Migración inicial o evolutiva de EF Core que incluya Identity y el tenant.

### Kiwbi.Web

Se crearán los elementos MVC necesarios:

- `AccountController` para registro, inicio y cierre de sesión.
- `DeveloperProfileController`, protegido por la política o rol `DeveloperAdmin`.
- ViewModels específicos de presentación; los controllers no expondrán entidades de dominio ni usarán `KiwbiDbContext`.
- Vistas Razor en español para acceso, perfil y branding.
- Configuración de middleware, rutas y autorización necesaria para Identity.

## Plan de Acción (Step-by-Step)

### Feature 1.1 - Setup del proyecto

- [ ] Revisar las referencias entre proyectos y confirmar la dirección `Web -> Application/Infrastructure -> Domain`.
- [ ] Confirmar los paquetes de EF Core, Npgsql e Identity, junto con la referencia compartida de ASP.NET Core necesaria en Infrastructure.
- [ ] Validar `DefaultConnection` para PostgreSQL y registrar `KiwbiDbContext` con `UseNpgsql`.
- [ ] Configurar ASP.NET Core Identity con `ApplicationUser`, `IdentityRole`, Entity Framework stores y proveedores de token.
- [ ] Configurar autenticación por cookies, autorización y rutas de acceso/denegación apropiadas.
- [ ] Crear y aplicar la migración inicial de PostgreSQL, verificando la creación de tablas Identity.

### Feature 1.2 - Autenticación básica y tenant

- [ ] Crear el módulo de dominio `Developers` con `DeveloperCompany`, `Branding` y `BrandColor`.
- [ ] Definir invariantes: nombre requerido, colores hexadecimales válidos y branding consistente.
- [ ] Crear `IDeveloperCompanyRepository` y los puertos `ICurrentUser`, `IAccountProvisioningService` e `IUnitOfWork`.
- [ ] Ampliar `ApplicationUser` con `DeveloperCompanyId` y configurar la relación EF Core con borrado restringido.
- [ ] Crear la configuración EF Core, `DbSet<DeveloperCompany>` y la implementación de repositorio.
- [ ] Implementar el adaptador de Identity para crear cuentas y garantizar el rol `DeveloperAdmin`.
- [ ] Implementar `RegisterDeveloper` como operación transaccional que cree tenant, cuenta y asignación de rol.
- [ ] Implementar los casos de uso de inicio y cierre de sesión mediante los puertos de Application.
- [ ] Registrar contratos, casos de uso y adaptadores en las extensiones de inyección de dependencias existentes.
- [ ] Generar y aplicar una migración que incorpore `developer_companies` y `developer_company_id`.

### Feature 1.3 - Dashboard, perfil y branding

- [ ] Implementar `GetCurrentDeveloperProfile` resolviendo el tenant a partir de `ICurrentUser`.
- [ ] Implementar `UpdateDeveloperProfile` y `UpdateDeveloperBranding` con autorización implícita por tenant.
- [ ] Crear ViewModels MVC para registro, login y perfil, separados de los DTOs de Application cuando el formato de la vista lo requiera.
- [ ] Crear `AccountController` y las vistas de registro, login y logout.
- [ ] Crear `DeveloperProfileController` y la vista de dashboard/perfil protegida para `DeveloperAdmin`.
- [ ] Añadir la edición de logo por referencia de almacenamiento y de colores de marca, aplicando validación de servidor.
- [ ] Confirmar que ningún formulario permite seleccionar o modificar el identificador de tenant.
- [ ] Verificar manualmente el flujo completo: registro, login, visualización y actualización del perfil, logout y acceso protegido.

### Cierre del Epic

- [ ] Ejecutar los tests unitarios y una compilación completa de la solución.
- [ ] Validar la migración contra PostgreSQL de desarrollo.
- [ ] Revisar que los controllers solo dependan de contratos de Application.
- [ ] Actualizar este documento con los checks completados y cualquier decisión técnica aprobada durante la implementación.
- [ ] Proponer actualización de `README.md` si cambian los requisitos de instalación, migración o configuración.

## Consideraciones de Testing y Notas de la IA

### Testing

- Usar xUnit, FluentAssertions y NSubstitute.
- Priorizar tests de Domain para las invariantes de `DeveloperCompany`, `Branding` y `BrandColor`.
- Priorizar tests de Application para verificar el alta transaccional, errores de duplicidad provenientes de Identity, resolución del tenant actual y actualización limitada al tenant autenticado.
- Usar sustitutos para `IDeveloperCompanyRepository`, `IAccountProvisioningService`, `ICurrentUser` e `IUnitOfWork`.
- No usar el proveedor In-Memory de EF Core como sustituto de PostgreSQL. Si se requieren tests de Infrastructure, crear pruebas de integración con PostgreSQL mediante Testcontainers en un trabajo posterior y explícito.
- Añadir una prueba de autorización o integración MVC para confirmar que una administradora no puede acceder al perfil de otra promotora cuando exista una superficie de navegación que lo permita.

### Notas de la IA

- El código, los nombres de tablas y el modelo técnico se escribirán en inglés; las vistas Razor y mensajes de UI se escribirán en español.
- La separación de tenants debe aplicarse desde el primer caso de uso: el identificador de promotora se deriva del principal autenticado y nunca es una entrada confiable del cliente.
- No mezclar `IdentityUser` con el dominio. Identity permanece en Infrastructure y se accede desde Application a través de interfaces.
- Mantener los métodos asíncronos y propagar `CancellationToken` en accesos a datos e I/O.
- El repositorio genérico existente puede mantenerse para operaciones comunes, pero las necesidades de consulta del tenant deben expresarse en un repositorio específico y no forzarse en una abstracción genérica.
- El aprovisionamiento de tenant, usuario y rol debe ejecutarse de forma atómica para evitar cuentas sin promotora o promotoras sin administradora.
- Antes de programar este Epic, el usuario debe aprobar este documento. Tras la aprobación, cada bloque se implementará y validará siguiendo el checklist anterior.
