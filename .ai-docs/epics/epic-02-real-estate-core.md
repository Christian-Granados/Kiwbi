# Epic 02: Real Estate Core (Catálogo)

## Estado

- Estado: Aprobado para implementación por el usuario el 2026-09-14.
- Depende de: Epic 1 (Foundation & Promotora Tenant), ya implementado. Reutiliza `DeveloperCompany`, `ICurrentUser`, `IUnitOfWork` y el patrón `Result`.

## Objetivos

Permitir que una promotora autenticada dé de alta y mantenga el catálogo inmobiliario de su tenant: las promociones (edificios/conjuntos), sus tipologías opcionales y las viviendas concretas que las componen, junto con una vista resumen que sirva de panel de control de cada promoción.

El Epic cubre las siguientes Features del roadmap:

- **Feature 2.1 - CRUD de Promociones:** Alta, edición, consulta, listado y baja de promociones del tenant, incluyendo la carga de una imagen/plano general.
- **Feature 2.2 - CRUD de Tipologías y Viviendas:** Alta, edición, consulta y baja de tipologías (agrupación opcional) y de viviendas concretas dentro de una promoción, incluyendo la carga del plano específico de cada vivienda.
- **Feature 2.3 - Vista resumen de la promoción:** Panel de detalle de una promoción con el listado de sus viviendas, su tipología, superficie y estado comercial inicial.

Quedan explícitamente fuera de alcance: gremios, personalizaciones, opciones, invitaciones Magic Link, vinculación de compradores, elecciones y exportaciones. Se desarrollarán en los Epics 3 a 6. Tampoco se implementa en este Epic ningún estado o campo relativo a la elección de personalización (`Pending/Selected/Confirmed/Paid`), que pertenece al modelo de `HomeCustomizationChoice` de Epics posteriores.

## Decisiones confirmadas por el usuario

1. **Nomenclatura en inglés:** confirmado `HousingPromotion` (Promoción), `HousingTypology` (Tipología) y `HousingUnit` (Vivienda).
2. **Estado comercial de la vivienda:** confirmado `HousingUnitStatus` (`Available`, `Reserved`, `Sold` -> Disponible/Reservada/Vendida), administrado manualmente por la promotora, independiente del futuro vínculo con el comprador (Epic 4) y de la elección de personalización (Epic 3/6). Todas las viviendas nacen en `Available`.
3. **Carga de imágenes/planos:** confirmado almacenamiento local en disco (`wwwroot/uploads/...`) para este Epic, encapsulado tras el puerto `IFileStorageService` en `Kiwbi.Application`, de modo que una futura migración a almacenamiento cloud (S3/Azure Blob) solo requiera una nueva implementación en `Kiwbi.Infrastructure`, sin tocar Application ni Web. No se trabaja el proveedor cloud en este Epic.
4. **Límites de agregado:** confirmado `HousingPromotion`, `HousingTypology` y `HousingUnit` como tres entidades/raíces independientes, cada una con su propio repositorio, relacionadas por Id. Las invariantes cruzadas (una vivienda o tipología debe pertenecer a una promoción del tenant actual) se validan en los casos de uso de Application, no dentro del agregado.
5. **Borrado:** confirmado bloquear el borrado de una `HousingPromotion` con tipologías o viviendas, y el de una `HousingTypology` con viviendas asignadas, devolviendo `Result.Failure` en vez de borrado en cascada o lógico.

## Análisis Técnico

### Modelo de dominio

Se crea el módulo `Kiwbi.Domain.RealEstate` con tres entidades y un enum, todas ancladas al tenant a través de `DeveloperCompanyId` (directo o indirecto):

| Elemento | Tipo | Responsabilidad | Propiedades iniciales |
| --- | --- | --- | --- |
| `HousingPromotion` | Entidad / raíz | Representa una promoción (edificio o conjunto residencial) de una promotora. | `Id`, `DeveloperCompanyId`, `Name`, `City`, `Address`, `MasterPlanImagePath` (opcional), `CreatedAtUtc`, `UpdatedAtUtc` |
| `HousingTypology` | Entidad / raíz | Agrupación lógica opcional de viviendas dentro de una promoción (ej. "Ático Tipo A"). | `Id`, `HousingPromotionId`, `Name`, `CreatedAtUtc`, `UpdatedAtUtc` |
| `HousingUnit` | Entidad / raíz | Vivienda concreta dentro de una promoción, opcionalmente asociada a una tipología. | `Id`, `HousingPromotionId`, `HousingTypologyId` (opcional), `Floor`, `Door`, `BuiltAreaSqm`, `UsableAreaSqm` (opcional), `FloorPlanImagePath` (opcional), `Status`, `CreatedAtUtc`, `UpdatedAtUtc` |
| `HousingUnitStatus` | Enum | Estado comercial administrado manualmente por la promotora. | `Available`, `Reserved`, `Sold` |

Comportamiento explícito (sin mutaciones arbitrarias), replicando el estilo de `DeveloperCompany`:

- `HousingPromotion.Create(developerCompanyId, name, city, address)`, `UpdateDetails(name, city, address)`, `UpdateMasterPlanImage(path)`.
- `HousingTypology.Create(housingPromotionId, name)`, `Rename(name)`.
- `HousingUnit.Create(housingPromotionId, floor, door, builtAreaSqm, usableAreaSqm, housingTypologyId)`, `UpdateDetails(floor, door, builtAreaSqm, usableAreaSqm)`, `AssignTypology(housingTypologyId)`, `UpdateFloorPlanImage(path)`, `ChangeStatus(status)`.

Invariantes de dominio (lanzan `DomainException`, convertidas a `Result`/`Result<T>` en Application):

- `HousingPromotion`: `Name`, `City` y `Address` obligatorios y no vacíos.
- `HousingTypology`: `Name` obligatorio y no vacío.
- `HousingUnit`: `Floor` y `Door` obligatorios; `BuiltAreaSqm` mayor que cero; `UsableAreaSqm`, si se informa, no puede superar `BuiltAreaSqm`.

La pertenencia de una `HousingTypology` o una `HousingUnit` a una promoción del tenant autenticado, y la de una `HousingUnit` a una tipología de la misma promoción, se valida en los casos de uso de Application (consultando los repositorios), no como invariante del propio agregado.

### Relaciones y esquema relacional

| Tabla | Procedencia | Columnas relevantes | Relaciones |
| --- | --- | --- | --- |
| `housing_promotions` | Dominio / EF Core | `id`, `developer_company_id`, `name`, `city`, `address`, `master_plan_image_path`, `created_at_utc`, `updated_at_utc` | FK obligatoria a `developer_companies.id` (borrado restringido). |
| `housing_typologies` | Dominio / EF Core | `id`, `housing_promotion_id`, `name`, `created_at_utc`, `updated_at_utc` | FK obligatoria a `housing_promotions.id` (borrado restringido). Índice único `(housing_promotion_id, name)`. |
| `housing_units` | Dominio / EF Core | `id`, `housing_promotion_id`, `housing_typology_id` (nullable), `floor`, `door`, `built_area_sqm`, `usable_area_sqm` (nullable), `floor_plan_image_path`, `status`, `created_at_utc`, `updated_at_utc` | FK obligatoria a `housing_promotions.id` (borrado restringido); FK opcional a `housing_typologies.id` (borrado restringido). Índice único `(housing_promotion_id, floor, door)`. |

`MasterPlanImagePath` y `FloorPlanImagePath` almacenan una ruta relativa devuelta por `IFileStorageService`, nunca el binario, replicando el criterio ya usado para `LogoPath` en Epic 1. `Status` se persiste como cadena (enum convertido con `HasConversion<string>()`) para legibilidad en base de datos.

## Impacto en Arquitectura

### Kiwbi.Domain

Se crearán en un nuevo módulo `RealEstate`:

- `HousingPromotion`, `HousingTypology`, `HousingUnit` y el enum `HousingUnitStatus`.
- `IHousingPromotionRepository : IRepository<HousingPromotion>` con `GetByDeveloperCompanyIdAsync`.
- `IHousingTypologyRepository : IRepository<HousingTypology>` con `GetByHousingPromotionIdAsync`.
- `IHousingUnitRepository : IRepository<HousingUnit>` con `GetByHousingPromotionIdAsync` y una consulta de existencia para validar duplicados de `Floor`/`Door` dentro de una promoción.

El proyecto sigue sin referenciar EF Core, Identity ni infraestructura de ficheros.

### Kiwbi.Application

Nuevo módulo `RealEstate`, con casos de uso organizados por intención (un command/result por caso, igual que en `Developers`):

- **Promociones:** `CreateHousingPromotion`, `UpdateHousingPromotion`, `UpdateHousingPromotionMasterPlan`, `GetHousingPromotion` (detalle), `GetHousingPromotions` (listado del tenant actual), `DeleteHousingPromotion`.
- **Tipologías:** `CreateHousingTypology`, `UpdateHousingTypology`, `DeleteHousingTypology`.
- **Viviendas:** `CreateHousingUnit`, `UpdateHousingUnit`, `UpdateHousingUnitFloorPlan`, `ChangeHousingUnitStatus`, `DeleteHousingUnit`.
- **Resumen (Feature 2.3):** `GetHousingPromotionSummary`, que devuelve los datos de la promoción junto con el listado de sus viviendas (tipología, planta, puerta, superficie, estado).

Todos los casos de uso:

- Resuelven `DeveloperCompanyId` desde `ICurrentUser` y devuelven `Result.Failure` (no una excepción) si la promoción/tipología/vivienda solicitada no pertenece al tenant autenticado.
- Retornan `Result` o `Result<T>`, reciben `CancellationToken` y se registran desde `AddApplicationServices`.
- Reutilizan `IUnitOfWork` para las operaciones de escritura.

Nuevo puerto en `Kiwbi.Application.Common`:

- `IFileStorageService`: `Task<Result<string>> SaveAsync(Stream content, string fileName, string subFolder, CancellationToken ct)` y `void Delete(string relativePath)`, para desacoplar Application de cualquier detalle de almacenamiento físico.

### Kiwbi.Infrastructure

- Configuraciones EF Core para `HousingPromotion`, `HousingTypology` y `HousingUnit` en `Persistence/Configurations`, con los índices únicos descritos arriba.
- Nuevos `DbSet` en `KiwbiDbContext`.
- `HousingPromotionRepository`, `HousingTypologyRepository`, `HousingUnitRepository`.
- `LocalFileStorageService : IFileStorageService`, basado en `IWebHostEnvironment.WebRootPath`, que escribe bajo `wwwroot/uploads/promotions` y `wwwroot/uploads/units` con nombres de fichero únicos (prefijo GUID) para evitar colisiones y path traversal.
- Migración de EF Core que añade `housing_promotions`, `housing_typologies` y `housing_units`.
- Registro de los nuevos repositorios y del servicio de almacenamiento en `AddInfrastructureServices`.

### Kiwbi.Web

- `HousingPromotionsController` (`[Authorize(Roles = "DeveloperAdmin")]`): `Index` (listado del tenant), `Create`/`Edit` (con subida de plano general), `Details` (resumen de Feature 2.3), `Delete`.
- `HousingTypologiesController`, con rutas anidadas bajo una promoción (`/HousingPromotions/{promotionId}/Typologies/...`): `Create`, `Edit`, `Delete`.
- `HousingUnitsController`, con rutas anidadas bajo una promoción (`/HousingPromotions/{promotionId}/Units/...`): `Create`/`Edit` (con subida de plano específico y selección opcional de tipología), `Delete`, acción específica `ChangeStatus`.
- ViewModels de presentación con `IFormFile` para los campos de imagen; los controllers convierten el archivo a `Stream` antes de invocar el caso de uso correspondiente y no acceden a `KiwbiDbContext` ni a rutas de disco directamente.
- Vistas Razor en español: listados, formularios de alta/edición y el resumen de promoción con tabla de viviendas y badges de estado.

## Plan de Acción (Step-by-Step)

### Feature 2.1 - CRUD de Promociones

- [x] Crear `HousingPromotion` en `Kiwbi.Domain.RealEstate` con sus invariantes y métodos (`Create`, `UpdateDetails`, `UpdateMasterPlanImage`).
- [x] Crear `IHousingPromotionRepository`.
- [x] Crear el puerto `IFileStorageService` en `Kiwbi.Application.Common`.
- [x] Implementar `CreateHousingPromotion`, `UpdateHousingPromotion`, `UpdateHousingPromotionMasterPlan`, `GetHousingPromotion`, `GetHousingPromotions` y `DeleteHousingPromotion`, resolviendo siempre el tenant desde `ICurrentUser`.
- [x] Crear la configuración EF Core, `DbSet<HousingPromotion>` y `HousingPromotionRepository`.
- [x] Implementar `LocalFileStorageService` y registrarlo en `AddInfrastructureServices`.
- [x] Generar y aplicar la migración que incorpora `housing_promotions`.
- [x] Crear `HousingPromotionsController` y las vistas de listado, alta, edición y baja, con subida del plano general.

### Feature 2.2 - CRUD de Tipologías y Viviendas

- [x] Crear `HousingTypology` y `HousingUnit` (+ `HousingUnitStatus`) en `Kiwbi.Domain.RealEstate` con sus invariantes.
- [x] Crear `IHousingTypologyRepository` e `IHousingUnitRepository`, incluyendo la consulta de duplicados `Floor`/`Door` por promoción.
- [x] Implementar `CreateHousingTypology`, `UpdateHousingTypology`, `DeleteHousingTypology` (bloqueando el borrado si hay viviendas asociadas), validando que la tipología pertenezca a una promoción del tenant actual.
- [x] Implementar `CreateHousingUnit`, `UpdateHousingUnit`, `UpdateHousingUnitFloorPlan`, `ChangeHousingUnitStatus`, `DeleteHousingUnit`, validando promoción/tipología del tenant actual y unicidad de `Floor`/`Door`.
- [x] Crear las configuraciones EF Core, `DbSet`s e índices únicos para `HousingTypology` y `HousingUnit`.
- [x] Implementar `HousingTypologyRepository` y `HousingUnitRepository`.
- [x] Generar y aplicar la migración que incorpora `housing_typologies` y `housing_units`.
- [x] Crear `HousingTypologiesController` y `HousingUnitsController` con sus vistas (alta, edición, baja, cambio de estado y subida del plano específico).

### Feature 2.3 - Vista resumen de la promoción

- [ ] Implementar `GetHousingPromotionSummary`, devolviendo los datos de la promoción y el listado de sus viviendas con tipología, planta, puerta, superficie y estado.
- [ ] Extender la vista `Details` de `HousingPromotionsController` para mostrar el resumen con una tabla de viviendas y badges de estado (`Available`/`Reserved`/`Sold`).
- [ ] Confirmar que el resumen solo es accesible para el tenant propietario de la promoción.

### Cierre del Epic

- [ ] Ejecutar los tests unitarios y una compilación completa de la solución.
- [ ] Validar la migración contra PostgreSQL de desarrollo.
- [ ] Revisar que los controllers solo dependan de contratos de Application y no accedan a `KiwbiDbContext` ni a rutas de disco.
- [ ] Actualizar este documento con los checks completados y cualquier decisión técnica aprobada durante la implementación.

## Consideraciones de Testing y Notas de la IA

### Testing

- Priorizar tests de Domain para las invariantes de `HousingPromotion`, `HousingTypology` y `HousingUnit` (nombre/ciudad/dirección obligatorios, planta/puerta obligatorios, superficie útil no mayor que la construida, transiciones de `HousingUnitStatus`).
- Priorizar tests de Application para: aislamiento de tenant (no se puede editar/consultar una promoción, tipología o vivienda de otra promotora), validación de duplicados `Floor`/`Door`, bloqueo de borrado con dependientes y el contenido del resumen de Feature 2.3.
- Usar sustitutos (NSubstitute) para `IHousingPromotionRepository`, `IHousingTypologyRepository`, `IHousingUnitRepository`, `IFileStorageService`, `ICurrentUser` e `IUnitOfWork`.
- No usar el proveedor In-Memory de EF Core; mantener el criterio de Epic 1 de no sustituir PostgreSQL con In-Memory.

### Notas de la IA

- Código y nombres técnicos en inglés; vistas y mensajes en español, igual que en Epic 1.
- El tenant (`DeveloperCompanyId`) se deriva siempre del principal autenticado vía `ICurrentUser`, nunca de un valor recibido del cliente.
- `HousingTypology` y `HousingUnit` no dependen de `HousingPromotion` como agregado contenedor: cada una es una raíz independiente con su propio repositorio; las invariantes cruzadas se validan en Application.
- Los controllers no deben manipular rutas de disco directamente: toda subida de fichero pasa por `IFileStorageService`.
- Antes de programar este Epic, el usuario debe resolver las preguntas abiertas de la sección anterior y aprobar este documento.

### Decisiones técnicas durante la ejecución (Feature 2.1)

- `app.MapStaticAssets()` (introducido en .NET 9/10 para assets con manifiesto de compilación) no sirve ficheros creados en tiempo de ejecución. Se añadió `app.UseStaticFiles()` en `Program.cs` para que los planos subidos a `wwwroot/uploads` sean servibles.
- `wwwroot/uploads/` se añadió a `.gitignore`: los ficheros subidos son datos de entorno, no código versionable.
- `LocalFileStorageService` valida extensión (whitelist `.jpg/.jpeg/.png/.webp/.pdf`) y tamaño máximo (10 MB), y genera siempre un nombre de fichero aleatorio en servidor; el nombre original del cliente nunca se usa para construir la ruta en disco (mitiga path traversal y ejecución de archivos no deseados).
- `UpdateHousingPromotionMasterPlanUseCase` borra el fichero anterior tras confirmar el guardado del nuevo, evitando huérfanos en disco al reemplazar el plano.
- Verificado manualmente: `/HousingPromotions` sin sesión redirige 302 a `/Account/Login`; `/` responde 200.

### Decisiones técnicas durante la ejecución (Feature 2.2)

- `DeleteHousingPromotionUseCase` se amplió (respecto al diseño inicial) para depender también de `IHousingTypologyRepository` e `IHousingUnitRepository` y bloquear el borrado si la promoción tiene tipologías o viviendas, tal como estaba previsto para cuando estos repositorios existieran.
- `HousingTypology` y `HousingUnit` no almacenan `DeveloperCompanyId` directamente; la pertenencia al tenant se resuelve en cada caso de uso cargando su `HousingPromotion` y comparando `DeveloperCompanyId`, consistente con la decisión de agregados independientes.
- Se añadieron casos de uso de lectura (`GetHousingTypology(s)`, `GetHousingUnit(s)`) no listados explícitamente en el análisis inicial, necesarios para las pantallas de listado y edición del CRUD.
- Verificado manualmente: `/HousingTypologies` y `/HousingUnits` sin sesión redirigen 302 a `/Account/Login`.
