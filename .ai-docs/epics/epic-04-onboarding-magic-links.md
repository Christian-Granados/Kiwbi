# Epic 04: Onboarding B2B2C (Magic Links)

## Estado

- Estado: Propuesta pendiente de aprobación del usuario. No implementado.
- Depende de: Epic 1 (Foundation & Promotora Tenant) y Epic 2 (Real Estate Core), ya implementados. Reutiliza `HousingUnit`, `ICurrentUser`, `IUnitOfWork`, `IAuthenticationService`, el patrón `Result` y el estilo de `ApplicationUser`/roles Identity de Epic 1.

## Objetivos

Permitir que una promotora invite por correo electrónico a un comprador para vincularlo a una Vivienda concreta, y que el comprador acepte esa invitación (Magic Link), quede registrado en Identity con el rol `Buyer` y vinculado a esa Vivienda, sin depender todavía de la experiencia visual del comprador (Epic 5).

El Epic cubre las siguientes Features del roadmap:

- **Feature 4.1 - Generación de token y envío de email:** La promotora asigna un correo a una Vivienda; se crea una invitación pendiente con un token de un solo uso y se "envía" el Magic Link (con un adaptador de email de desarrollo en esta fase).
- **Feature 4.2 - Aceptación del comprador:** El comprador abre el Magic Link, confirma su acceso con una contraseña (nueva o existente), queda registrado en Identity con el rol `Buyer` y su cuenta queda vinculada a la Vivienda mediante un enlace independiente del token de invitación.

Quedan explícitamente fuera de alcance: el dashboard y la experiencia visual del comprador (Epic 5), la visualización de personalizaciones y la elección de opciones, el envío de correo real por SMTP (se documenta como sustitución futura del adaptador de desarrollo) y cualquier flujo de "desvincular" un comprador ya asignado a una Vivienda (se deja para un Epic posterior de gestión, si se necesita).

## Decisiones confirmadas por el usuario

1. **Envío de email:** en este Epic se implementa el puerto `IEmailSender` en Application y **un adaptador de desarrollo** en Infrastructure que registra el enlace del Magic Link en el log (`ILogger`) en vez de enviarlo por SMTP real. La sustitución por un proveedor SMTP real (o un servicio transaccional tipo SendGrid) queda para un trabajo posterior explícito, sin cambios en Application.
2. **Relación Vivienda-Comprador:** se modela como **N:M completo**: una Vivienda puede tener varios Compradores (p. ej. una pareja) y, a diferencia de lo que hacía `ApplicationUser.DeveloperCompanyId` en Epic 1 (FK simple 1:1), un mismo Comprador podría en el futuro estar vinculado a más de una Vivienda. Se modela con una entidad de enlace propia (`HousingUnitBuyer`), no con una columna en `ApplicationUser`.
3. **Expiración del Magic Link:** el token de invitación caduca a los **7 días** de su creación (o de su último reenvío).
4. **Reenvío y cancelación:** la promotora puede reenviar (regenerando token y fecha de caducidad) o cancelar una invitación mientras esté en estado `Pending`, cubierto en este mismo Epic.

## Análisis Técnico

### Modelo de dominio

Se crea el módulo `Kiwbi.Domain.Onboarding` con dos raíces de agregado independientes, ambas ancladas a una `HousingUnit` de Epic 2 por `HousingUnitId` (mismo criterio que Epic 3 con `HousingPromotion`/`HousingTypology`/`HousingUnit`: no hay navegación de colección en `HousingUnit` hacia estas entidades, la relación se resuelve desde el lado del módulo nuevo):

| Elemento | Tipo | Responsabilidad | Propiedades iniciales |
| --- | --- | --- | --- |
| `BuyerInvitation` | Entidad / raíz | Invitación pendiente enviada a un correo para una Vivienda concreta; controla el ciclo de vida del Magic Link. | `Id`, `HousingUnitId`, `Email`, `Token`, `Status` (enum), `CreatedAtUtc`, `UpdatedAtUtc`, `ExpiresAtUtc`, `AcceptedAtUtc` (opcional), `AcceptedByUserId` (opcional) |
| `BuyerInvitationStatus` | Enum | Ciclo de vida de la invitación. | `Pending`, `Accepted`, `Cancelled` |
| `HousingUnitBuyer` | Entidad / raíz | Enlace confirmado entre un Comprador (Identity) y una Vivienda; se crea al aceptar una invitación. | `Id`, `HousingUnitId`, `BuyerUserId` (string, id de Identity), `CreatedAtUtc` |

Comportamiento explícito, replicando el estilo de `TradeCategory`/`Customization` (Epic 3):

- `BuyerInvitation.Create(housingUnitId, email)`: genera internamente un token aleatorio criptográficamente seguro (`RandomNumberGenerator`, sin dependencias de framework) y fija `ExpiresAtUtc = CreatedAtUtc + 7 días`. Nace en `Pending`.
- `BuyerInvitation.Resend()`: solo permitido en `Pending` (con o sin haber expirado ya); regenera `Token` y recalcula `ExpiresAtUtc` a 7 días desde el reenvío.
- `BuyerInvitation.Cancel()`: solo permitido en `Pending`; pasa a `Cancelled`.
- `BuyerInvitation.MarkAccepted(acceptedByUserId, utcNow)`: solo permitido en `Pending` y no expirada; pasa a `Accepted`, fija `AcceptedAtUtc`/`AcceptedByUserId`.
- `BuyerInvitation.IsExpired(DateTime utcNow)`: `Status == Pending && utcNow > ExpiresAtUtc` (mismo patrón que `TradeCategory.IsExpired`, sin persistir un estado `Expired` separado).
- `HousingUnitBuyer.Create(housingUnitId, buyerUserId)`: entidad de enlace inmutable, sin métodos de mutación adicionales en este Epic.

Invariantes de dominio (lanzan `DomainException`, convertidas a `Result`/`Result<T>` en Application):

- `BuyerInvitation`: `Email` obligatorio y con formato válido (`System.Net.Mail.MailAddress` o comprobación equivalente en Domain, coherente con que el `[EmailAddress]` de las ViewModels de Web ya valida el formato en el borde de entrada, pero el dominio no debe confiar únicamente en la capa de presentación); `Resend`/`Cancel`/`MarkAccepted` lanzan si el estado actual no lo permite (transición inválida).
- `HousingUnitBuyer`: `HousingUnitId` y `BuyerUserId` obligatorios.

La pertenencia de la `HousingUnit` de una invitación a una promoción del tenant autenticado se resuelve en los casos de uso de Application (cargando `HousingUnit` -> `HousingPromotion` -> `DeveloperCompanyId`, mismo criterio que Epic 2/3), no como invariante del agregado.

### Relaciones y esquema relacional

| Tabla | Procedencia | Columnas relevantes | Relaciones |
| --- | --- | --- | --- |
| `buyer_invitations` | Dominio / EF Core | `id`, `housing_unit_id`, `email`, `token`, `status`, `created_at_utc`, `updated_at_utc`, `expires_at_utc`, `accepted_at_utc` (nullable), `accepted_by_user_id` (nullable) | FK obligatoria a `housing_units.id` (borrado restringido). Índice único en `token`. Índice no único en `(housing_unit_id, email)` para listar/buscar duplicados pendientes. |
| `home_buyer_assignments` *(tabla `housing_unit_buyers`)* | Dominio / EF Core | `id`, `housing_unit_id`, `buyer_user_id`, `created_at_utc` | FK obligatoria a `housing_units.id` (borrado restringido); FK obligatoria a `AspNetUsers.id` (borrado restringido). Índice único `(housing_unit_id, buyer_user_id)` para impedir enlaces duplicados. |
| `AspNetUsers` | ASP.NET Core Identity | sin cambios de esquema | Un comprador es un `ApplicationUser` sin `DeveloperCompanyId` (igual que hoy), con el rol `Buyer` en vez de `DeveloperAdmin`. |
| `AspNetRoles` | ASP.NET Core Identity | columnas estándar | Se añade el rol `Buyer`, creado de forma perezosa igual que `DeveloperAdmin` en Epic 1. |

`Status` de `BuyerInvitation` se persiste como cadena (`HasConversion<string>()`), mismo criterio que `CustomizationScope`/`HousingUnitStatus`. El `Token` se genera con suficiente entropía (256 bits, codificado en Base64Url) para ser inadivinable; no se reutiliza como clave primaria.

## Impacto en Arquitectura

### Kiwbi.Domain

Nuevo módulo `Onboarding`:

- `BuyerInvitation`, `BuyerInvitationStatus`, `HousingUnitBuyer`.
- `IBuyerInvitationRepository : IRepository<BuyerInvitation>` con `GetByHousingUnitIdAsync`, `GetByTokenAsync`, `ExistsPendingByHousingUnitIdAndEmailAsync`, `ExistsByHousingUnitIdAsync` (para bloquear el borrado de Epic 2 si hay invitaciones).
- `IHousingUnitBuyerRepository : IRepository<HousingUnitBuyer>` con `GetByHousingUnitIdAsync`, `GetByBuyerUserIdAsync` (reutilizable por Epic 5 para "Mis Viviendas"), `ExistsByHousingUnitIdAndBuyerUserIdAsync`, `ExistsByHousingUnitIdAsync` (para bloquear el borrado de Epic 2 si hay compradores vinculados).

El proyecto sigue sin referenciar EF Core ni Identity.

### Kiwbi.Application

Nuevo módulo `Onboarding`, casos de uso organizados por intención:

- **Invitación (promotora, autenticada):** `InviteBuyerToHousingUnit` (crea la invitación y envía el email), `ResendBuyerInvitation`, `CancelBuyerInvitation`, `GetBuyerInvitationsForHousingUnit` (listado, incluye compradores ya vinculados vía `HousingUnitBuyer` para la misma vista).
- **Aceptación (comprador, anónimo):** `GetBuyerInvitationByToken` (resuelve la invitación por token para pintar la pantalla de aceptación, incluye si el correo ya tiene cuenta), `AcceptBuyerInvitation` (valida token, crea o reutiliza la cuenta Identity, crea el `HousingUnitBuyer`, marca la invitación `Accepted` e inicia sesión, todo dentro de una transacción).

Nuevos puertos:

- `IEmailSender` (`Kiwbi.Application.Common` o `Onboarding`): `SendBuyerInvitationEmailAsync(email, token, expiresAtUtc, ct)`. Application no construye URLs ni conoce rutas de Web; delega en la implementación de Infrastructure.
- `IBuyerAccountProvisioningService` (paralelo a `IAccountProvisioningService`, pero para compradores en vez de administradoras): `ExistsByEmailAsync`, `FindUserIdByEmailAsync`, `CreateBuyerAccountAsync(email, password)` (crea `ApplicationUser` sin `DeveloperCompanyId`, asigna el rol `Buyer`), `GetEmailByUserIdAsync` (para mostrar el correo de los compradores ya vinculados en el listado de la promotora).

Flujo de `AcceptBuyerInvitationUseCase` (Command: `Token`, `Password`):

1. Cargar `BuyerInvitation` por token; `Result.Failure` si no existe, no está `Pending` o ha expirado.
2. Resolver si ya existe una cuenta Identity para `invitation.Email` (`IBuyerAccountProvisioningService.ExistsByEmailAsync`).
   - Si no existe: crear la cuenta con la contraseña recibida (`CreateBuyerAccountAsync`), asignando el rol `Buyer`.
   - Si ya existe: no se crea cuenta nueva; la contraseña recibida se valida a través de `IAuthenticationService.SignInAsync` (reutiliza el inicio de sesión existente, sin duplicar lógica de validación de contraseña).
3. Si no existe ya un `HousingUnitBuyer` para `(HousingUnitId, BuyerUserId)`, crearlo.
4. Marcar la invitación `Accepted`.
5. Confirmar todo con `IUnitOfWork.ExecuteInTransactionAsync`, igual patrón que `RegisterDeveloperUseCase` (Epic 1).
6. Si el paso 2 creó la cuenta (contraseña aún no verificada por `SignInAsync`), iniciar sesión al final llamando también a `IAuthenticationService.SignInAsync` con la contraseña recién establecida.

Todos los casos de uso devuelven `Result`/`Result<T>`, reciben `CancellationToken` y se registran desde `AddApplicationServices`. Los casos de uso de invitación (promotora) resuelven el tenant desde `ICurrentUser` vía `HousingUnit -> HousingPromotion -> DeveloperCompanyId`; los de aceptación (comprador) son anónimos por diseño y no consultan `ICurrentUser`.

Cambios sobre casos de uso existentes de Epic 2:

- `DeleteHousingUnitUseCase` amplía su comprobación de dependientes para bloquear el borrado si existen `BuyerInvitation` o `HousingUnitBuyer` asociados a la vivienda (`IBuyerInvitationRepository.ExistsByHousingUnitIdAsync` / `IHousingUnitBuyerRepository.ExistsByHousingUnitIdAsync`), mismo criterio incremental usado en Epic 3 sobre `DeleteHousingTypologyUseCase`/`DeleteHousingUnitUseCase`.

### Kiwbi.Infrastructure

- Configuraciones EF Core para `BuyerInvitation` (tabla `buyer_invitations`) y `HousingUnitBuyer` (tabla `home_buyer_assignments`) en `Persistence/Configurations`.
- Nuevos `DbSet<BuyerInvitation>` y `DbSet<HousingUnitBuyer>` en `KiwbiDbContext`.
- `BuyerInvitationRepository`, `HousingUnitBuyerRepository`.
- `IdentityBuyerAccountProvisioningService : IBuyerAccountProvisioningService`, reutilizando `UserManager<ApplicationUser>`/`RoleManager<IdentityRole>` (mismo patrón que `IdentityAccountProvisioningService`), creando el rol `Buyer` de forma perezosa.
- `ApplicationRoles.Buyer` añadido junto a `ApplicationRoles.DeveloperAdmin`.
- `LoggingBuyerInvitationEmailSender : IEmailSender`: adaptador de desarrollo que compone la URL del Magic Link a partir de un `AppBaseUrl` configurado (`appsettings.json`) más la ruta fija `/Onboarding/Accept?token=...`, y la registra vía `ILogger` en vez de enviarla por SMTP.
- Migración de EF Core que añade `buyer_invitations` y `home_buyer_assignments`.
- Registro de los nuevos repositorios/servicios en `AddInfrastructureServices`.
- Nueva clave `AppBaseUrl` en `appsettings.json`/`appsettings.Development.json`.

### Kiwbi.Web

- Extensión de `HousingUnitsController` (Epic 2), anidado bajo una vivienda (`[Authorize(Roles = "DeveloperAdmin")]`): acción `Invitations` (listado de invitaciones + compradores ya vinculados a la vivienda), `InviteBuyer` (GET/POST, formulario de correo), `ResendInvitation`/`CancelInvitation` (POST simples que redirigen a `Invitations`).
- Nuevo `OnboardingController` (`[AllowAnonymous]`): `Accept(string token)` GET (resuelve la invitación, muestra email/vivienda y un formulario de contraseña; si el token es inválido/caducado/cancelado/ya aceptado, muestra una vista de error específica) y `Accept(string token, string password)` POST (llama a `AcceptBuyerInvitationUseCase`; si tiene éxito, el usuario queda autenticado con cookie y se redirige a una vista de confirmación simple propia de este Epic, ya que el dashboard del comprador es Epic 5).
- ViewModels de presentación (`InviteBuyerViewModel`, `AcceptInvitationViewModel`) separados de los DTOs de Application. Los controllers no acceden a `KiwbiDbContext` ni a los repositorios directamente.
- Vistas Razor en español: formulario de invitación, listado de invitaciones/compradores de una vivienda, pantalla de aceptación del Magic Link y pantalla de confirmación final.

## Plan de Acción (Step-by-Step)

### Feature 4.1 - Generación de token y envío de email

- [ ] Crear `BuyerInvitation`, `BuyerInvitationStatus` en `Kiwbi.Domain.Onboarding`, con generación interna de token, `ExpiresAtUtc` a 7 días, y los métodos `Create`/`Resend`/`Cancel`/`MarkAccepted`/`IsExpired`.
- [ ] Crear `IBuyerInvitationRepository`.
- [ ] Crear el puerto `IEmailSender` en Application.
- [ ] Implementar `InviteBuyerToHousingUnit`, `ResendBuyerInvitation`, `CancelBuyerInvitation`, `GetBuyerInvitationsForHousingUnit`, resolviendo el tenant desde `ICurrentUser` vía la `HousingUnit`/`HousingPromotion` padre y bloqueando duplicados pendientes por `(HousingUnitId, Email)`.
- [ ] Ampliar `DeleteHousingUnitUseCase` para bloquear el borrado si existen `BuyerInvitation` asociadas.
- [ ] Crear la configuración EF Core, `DbSet<BuyerInvitation>` y `BuyerInvitationRepository`.
- [ ] Implementar `ApplicationRoles.Buyer` y `LoggingBuyerInvitationEmailSender` (adaptador de desarrollo), leyendo `AppBaseUrl` de configuración.
- [ ] Generar y aplicar la migración que incorpora `buyer_invitations`.
- [ ] Extender `HousingUnitsController` con `Invitations`, `InviteBuyer`, `ResendInvitation`, `CancelInvitation` y sus vistas.

### Feature 4.2 - Aceptación del comprador y vinculación

- [ ] Crear `HousingUnitBuyer` en `Kiwbi.Domain.Onboarding`.
- [ ] Crear `IHousingUnitBuyerRepository`.
- [ ] Crear el puerto `IBuyerAccountProvisioningService` en Application.
- [ ] Implementar `GetBuyerInvitationByToken` y `AcceptBuyerInvitation` (flujo transaccional de creación/reutilización de cuenta + enlace + marcado de invitación + inicio de sesión).
- [ ] Ampliar `DeleteHousingUnitUseCase` para bloquear también el borrado si existen `HousingUnitBuyer` asociados.
- [ ] Crear la configuración EF Core, `DbSet<HousingUnitBuyer>` y `HousingUnitBuyerRepository`.
- [ ] Implementar `IdentityBuyerAccountProvisioningService`.
- [ ] Generar y aplicar la migración que incorpora `home_buyer_assignments`.
- [ ] Crear `OnboardingController` (`AllowAnonymous`) con `Accept` GET/POST y las vistas de aceptación, error y confirmación.
- [ ] Ampliar el listado de `Invitations` de `HousingUnitsController` para mostrar también los `HousingUnitBuyer` ya confirmados (correo resuelto vía `GetEmailByUserIdAsync`).

### Cierre del Epic

- [ ] Ejecutar los tests unitarios y una compilación completa de la solución.
- [ ] Validar las migraciones contra PostgreSQL de desarrollo.
- [ ] Revisar que los controllers solo dependan de contratos de Application y no accedan a `KiwbiDbContext`.
- [ ] Verificar manualmente el flujo completo: invitar, ver el log del Magic Link, aceptar (cuenta nueva), aceptar una segunda invitación con el mismo correo (cuenta existente), reenviar, cancelar, e intentar aceptar un token caducado/cancelado/ya aceptado.
- [ ] Actualizar este documento con los checks completados y cualquier decisión técnica aprobada durante la implementación.

## Consideraciones de Testing y Notas de la IA

### Testing

- Priorizar tests de Domain para las invariantes de `BuyerInvitation` (formato de email, transiciones de estado válidas/ inválidas en `Resend`/`Cancel`/`MarkAccepted`, `IsExpired`) y de `HousingUnitBuyer` (campos obligatorios).
- Priorizar tests de Application para: aislamiento de tenant en las invitaciones (no se puede invitar/reenviar/cancelar sobre una vivienda de otra promotora), bloqueo de duplicados (`(HousingUnitId, Email)` pendiente ya existente, `(HousingUnitId, BuyerUserId)` ya enlazado), el flujo completo de `AcceptBuyerInvitation` para cuenta nueva y para cuenta ya existente (incluyendo contraseña incorrecta), rechazo de tokens inexistentes/caducados/cancelados/ya aceptados, y bloqueo de borrado de `HousingUnit` con invitaciones o compradores dependientes.
- Usar sustitutos (NSubstitute) para `IBuyerInvitationRepository`, `IHousingUnitBuyerRepository`, `IHousingUnitRepository`, `IEmailSender`, `IBuyerAccountProvisioningService`, `IAuthenticationService`, `ICurrentUser` e `IUnitOfWork`.
- No usar el proveedor In-Memory de EF Core; mantener el criterio de Epics anteriores de no sustituir PostgreSQL con In-Memory.

### Notas de la IA

- Código y nombres técnicos en inglés; vistas y mensajes en español, igual que en Epics anteriores.
- El tenant (`DeveloperCompanyId`) se deriva siempre del principal autenticado vía `ICurrentUser` en el lado de la invitación (promotora); el lado de aceptación (comprador) es intencionadamente anónimo y solo confía en la posesión del token recibido por correo, nunca en un identificador de vivienda recibido del navegador.
- El token de invitación es de un solo flujo de uso (`Pending -> Accepted` o `Pending -> Cancelled`, sin vuelta atrás); un reenvío no crea una fila nueva, regenera la existente para no dejar tokens antiguos válidos en paralelo.
- La distinción entre `BuyerInvitation` (intención/pendiente) y `HousingUnitBuyer` (vínculo confirmado) es intencional, igual criterio de separación que Epic 3 entre catálogo de configuración y datos transaccionales: permite conservar el histórico de invitaciones (incluidas las canceladas o caducadas) sin mezclarlo con la relación de negocio real comprador-vivienda que consumirá Epic 5.
- El adaptador de email de desarrollo (`LoggingBuyerInvitationEmailSender`) es una implementación explícitamente temporal: queda documentado que sustituirla por un proveedor SMTP real no requiere cambios en Domain ni Application, solo un nuevo adaptador de Infrastructure y su registro en `AddInfrastructureServices`.
- Antes de programar este Epic, el usuario debe aprobar este documento.
