# Epic 03: Customization Engine (El Motor)

## Estado

- Estado: Implementado y validado (build + tests + smoke tests manuales de las 3 Features).
- Depende de: Epic 1 (Foundation & Promotora Tenant) y Epic 2 (Real Estate Core), ya implementados. Reutiliza `HousingPromotion`, `HousingTypology`, `HousingUnit`, `ICurrentUser`, `IUnitOfWork` y el patrón `Result`.

## Objetivos

Permitir que una promotora autenticada defina, dentro de una promoción, el catálogo de personalización que después usará el comprador (Epic 5): los Gremios (con su fecha límite de selección), las Personalizaciones agrupadas por Gremio y asignadas a Promoción/Tipología/Vivienda, y las Opciones de cada Personalización con su sobrecoste y su opción por defecto.

El Epic cubre las siguientes Features del roadmap:

- **Feature 3.1 - CRUD de Gremios:** Alta, edición, consulta, listado y baja de Gremios de una promoción, cada uno con su `SelectionCutOffDateUtc`.
- **Feature 3.2 - CRUD de Personalizaciones y asociación condicional:** Alta, edición, consulta y baja de Personalizaciones dentro de un Gremio, con gestión de a qué se aplican (toda la promoción, una o varias tipologías, o una o varias viviendas concretas).
- **Feature 3.3 - CRUD de Opciones por Personalización:** Alta, edición y baja de Opciones dentro de una Personalización, con su sobrecoste y selector de opción por defecto.

Quedan explícitamente fuera de alcance: invitaciones Magic Link, vinculación de compradores, la entidad transaccional `HomeCustomizationChoice` (elección del comprador) y sus estados (`Pending/Selected/Confirmed/Paid`), el bloqueo de selección al pasar la fecha límite (aplica a la UI del comprador, Epic 5) y las exportaciones (Epic 6). Este Epic solo construye el catálogo de configuración que la promotora define; no hay todavía ningún comprador seleccionando nada.

## Decisiones confirmadas por el usuario

1. **Nomenclatura en inglés:** confirmado `TradeCategory` (Gremio), `Customization` (Personalización) y `CustomizationOption` (Opción), consistente con los ejemplos ya citados en `04-ai-coding-guidelines.md`.
2. **Alcance del Gremio:** confirmado que `TradeCategory` pertenece 1:1 a una `HousingPromotion` (sin catálogo reutilizable a nivel de Promotora). Dos promociones distintas con un Gremio de igual nombre ("Fontanería") son dos registros independientes.
3. **Modelo de asignación de la Personalización (Feature 3.2):** confirmado el modelo flexible: una `Customization` puede asignarse a toda la promoción, o a **varias** Tipologías concretas, o a **varias** Viviendas concretas (no limitado a una sola entidad por nivel). Se modela con una colección de asignaciones (`CustomizationAssignment`) dentro del propio agregado `Customization`.
4. **Límite del agregado de Opciones:** confirmado que `CustomizationOption` es una entidad hija del agregado `Customization` (no una raíz independiente con repositorio propio). La invariante "exactamente una Opción con `IsDefault = true`" se protege en Domain, dentro de los métodos de `Customization`, y no depende de que cada caso de uso recuerde desmarcar manualmente las demás opciones.
5. **Sobrecoste:** confirmado que el campo económico de `CustomizationOption` es un sobrecoste (`SurchargeAmount`), no un precio absoluto, y que **no se permiten valores negativos** (no hay opciones "descuento" en este MVP). Varias opciones de una misma Personalización pueden tener `SurchargeAmount = 0` simultáneamente (no solo la opción por defecto).
6. **Fecha límite del Gremio:** confirmado que `SelectionCutOffDateUtc` es una fecha **y hora** exactas elegidas por la promotora (no solo un día), consistente con la regla `DateTime.UtcNow > Gremio.FechaLimiteSeleccion` de `03-domain-model.md`.

## Análisis Técnico

### Modelo de dominio

Se crea el módulo `Kiwbi.Domain.Customizations` con dos raíces de agregado y dos entidades hijas, todas ancladas al tenant de forma indirecta (a través de `HousingPromotion`, igual que `HousingTypology`/`HousingUnit` en Epic 2):

| Elemento | Tipo | Responsabilidad | Propiedades iniciales |
| --- | --- | --- | --- |
| `TradeCategory` | Entidad / raíz | Gremio de una promoción, define la fecha límite de selección de sus Personalizaciones. | `Id`, `HousingPromotionId`, `Name`, `SelectionCutOffDateUtc`, `CreatedAtUtc`, `UpdatedAtUtc` |
| `Customization` | Entidad / raíz de agregado | Personalización dentro de un Gremio; agrega sus Opciones y sus Asignaciones. | `Id`, `TradeCategoryId`, `Name`, `Options` (colección), `Assignments` (colección), `CreatedAtUtc`, `UpdatedAtUtc` |
| `CustomizationOption` | Entidad hija (sin repo propio) | Opción concreta de una Personalización. | `Id`, `Name`, `SurchargeAmount`, `IsDefault` |
| `CustomizationAssignment` | Entidad hija (sin repo propio) | A qué se aplica la Personalización: toda la promoción, una tipología o una vivienda. | `Id`, `Scope` (enum), `HousingTypologyId` (opcional), `HousingUnitId` (opcional) |
| `CustomizationScope` | Enum | Nivel de aplicación de una asignación. | `WholePromotion`, `Typology`, `Unit` |

Comportamiento explícito, replicando el estilo de `HousingPromotion`/`HousingUnit`:

- `TradeCategory.Create(housingPromotionId, name, selectionCutOffDateUtc)`, `Rename(name)`, `Reschedule(selectionCutOffDateUtc)`, `IsExpired(DateTime utcNow)`.
- `Customization.Create(tradeCategoryId, name, defaultOptionName, defaultOptionSurchargeAmount)`: crea la Personalización **junto con su primera Opción**, que nace `IsDefault = true`, para que el agregado nunca exista sin al menos una opción por defecto.
- `Customization.Rename(name)`.
- `Customization.AddOption(name, surchargeAmount)`: añade una opción adicional con `IsDefault = false`.
- `Customization.UpdateOption(optionId, name, surchargeAmount)`.
- `Customization.SetDefaultOption(optionId)`: desmarca la opción por defecto actual y marca la indicada, en una sola operación de dominio.
- `Customization.RemoveOption(optionId)`: bloquea el borrado si es la única opción restante o si es la opción por defecto (hay que fijar antes otra opción como `IsDefault` con `SetDefaultOption`).
- `Customization.AssignToWholePromotion()`, `Customization.AssignToTypology(housingTypologyId)`, `Customization.AssignToUnit(housingUnitId)`, `Customization.RemoveAssignment(assignmentId)`.

Invariantes de dominio (lanzan `DomainException`, convertidas a `Result`/`Result<T>` en Application):

- `TradeCategory`: `Name` obligatorio y no vacío; `SelectionCutOffDateUtc` obligatoria (sin restricción de "debe ser futura", para permitir correcciones tras la creación).
- `Customization`: `Name` obligatorio y no vacío; siempre tiene al menos una `CustomizationOption`; exactamente una `CustomizationOption` con `IsDefault = true` en todo momento.
- `CustomizationOption`: `Name` obligatorio; `SurchargeAmount >= 0`; no se permite un nombre de opción duplicado dentro de la misma Personalización.
- `CustomizationAssignment`: no se permite mezclar `WholePromotion` con asignaciones de `Typology`/`Unit` en la misma Personalización (son mutuamente excluyentes); no se permite duplicar la asignación a la misma Tipología o Vivienda; toda `Customization` debe tener al menos una asignación en todo momento (se exige una asignación inicial al crearla, igual que la opción por defecto).

La pertenencia de un `TradeCategory` a una promoción del tenant autenticado, y la de las Tipologías/Viviendas referenciadas por `CustomizationAssignment` a la misma promoción que el `TradeCategory` de la `Customization`, se valida en los casos de uso de Application (consultando los repositorios existentes de Epic 2), no como invariante del propio agregado — igual criterio que en Epic 2 para las relaciones entre agregados distintos.

### Relaciones y esquema relacional

| Tabla | Procedencia | Columnas relevantes | Relaciones |
| --- | --- | --- | --- |
| `trade_categories` | Dominio / EF Core | `id`, `housing_promotion_id`, `name`, `selection_cut_off_date_utc`, `created_at_utc`, `updated_at_utc` | FK obligatoria a `housing_promotions.id` (borrado restringido). Índice único `(housing_promotion_id, name)`. |
| `customizations` | Dominio / EF Core | `id`, `trade_category_id`, `name`, `created_at_utc`, `updated_at_utc` | FK obligatoria a `trade_categories.id` (borrado restringido). Índice único `(trade_category_id, name)`. |
| `customization_options` | Dominio / EF Core (tabla propiedad de `Customization`, sin `DbSet` propio) | `id`, `customization_id`, `name`, `surcharge_amount`, `is_default` | FK obligatoria a `customizations.id` (borrado en cascada, solo alcanzable a través del agregado). Índice único `(customization_id, name)`. |
| `customization_assignments` | Dominio / EF Core (tabla propiedad de `Customization`, sin `DbSet` propio) | `id`, `customization_id`, `scope`, `housing_typology_id` (nullable), `housing_unit_id` (nullable) | FK obligatoria a `customizations.id` (borrado en cascada); FK opcional a `housing_typologies.id` y a `housing_units.id` (borrado restringido). Índices únicos parciales: `(customization_id, housing_typology_id)` donde no es nulo, y `(customization_id, housing_unit_id)` donde no es nulo. |

`SurchargeAmount` se persiste como `decimal(10,2)`. `Scope` se persiste como cadena (`HasConversion<string>()`), igual criterio que `HousingUnitStatus` en Epic 2. `customization_options` y `customization_assignments` se mapean como colecciones "owned" de EF Core (`OwnsMany`) del agregado `Customization`: no tienen `DbSet` propio ni son consultables de forma independiente, reforzando a nivel de persistencia que solo se accede a ellas cargando la `Customization` completa.

## Impacto en Arquitectura

### Kiwbi.Domain

Nuevo módulo `Customizations`:

- `TradeCategory`, `Customization`, `CustomizationOption`, `CustomizationAssignment`, `CustomizationScope`.
- `ITradeCategoryRepository : IRepository<TradeCategory>` con `GetByHousingPromotionIdAsync` y `ExistsByHousingPromotionIdAsync`.
- `ICustomizationRepository : IRepository<Customization>` con `GetByTradeCategoryIdAsync` (carga el agregado completo, incluyendo `Options` y `Assignments`), `ExistsByTradeCategoryIdAsync`, `ExistsByHousingTypologyIdAsync` y `ExistsByHousingUnitIdAsync` (para bloquear el borrado de Tipologías/Viviendas de Epic 2 referenciadas por alguna asignación).

El proyecto sigue sin referenciar EF Core ni infraestructura.

### Kiwbi.Application

Nuevo módulo `Customizations`, casos de uso organizados por intención:

- **Gremios:** `CreateTradeCategory`, `UpdateTradeCategory` (nombre y fecha límite), `GetTradeCategory`, `GetTradeCategories` (listado por promoción), `DeleteTradeCategory` (bloquea si tiene Personalizaciones).
- **Personalizaciones:** `CreateCustomization` (incluye la opción por defecto inicial y la asignación inicial), `RenameCustomization`, `GetCustomization` (detalle con Opciones y Asignaciones), `GetCustomizations` (listado por Gremio), `DeleteCustomization`.
- **Opciones (dentro del agregado Customization):** `AddCustomizationOption`, `UpdateCustomizationOption`, `SetDefaultCustomizationOption`, `RemoveCustomizationOption`.
- **Asignaciones (dentro del agregado Customization):** `AssignCustomizationToTypology`, `AssignCustomizationToUnit`, `RemoveCustomizationAssignment` (la asignación a "toda la promoción" se fija solo en `CreateCustomization`, dado que es mutuamente excluyente con el resto).

Todos los casos de uso:

- Resuelven `DeveloperCompanyId` desde `ICurrentUser` y devuelven `Result.Failure` si el Gremio/Personalización solicitados, o la Tipología/Vivienda de destino de una asignación, no pertenecen a una promoción del tenant autenticado.
- Retornan `Result` o `Result<T>`, reciben `CancellationToken` y se registran desde `AddApplicationServices`.
- Reutilizan `IUnitOfWork` para las operaciones de escritura.

Cambios sobre casos de uso existentes de Epic 2 (necesarios para no dejar huérfanos ni romper invariantes cruzadas):

- `DeleteHousingPromotionUseCase` amplía su comprobación de dependientes para bloquear también el borrado si la promoción tiene `TradeCategory` asociados.
- `DeleteHousingTypologyUseCase` y `DeleteHousingUnitUseCase` amplían su comprobación de dependientes para bloquear el borrado si la Tipología/Vivienda está referenciada por alguna `CustomizationAssignment` (vía `ICustomizationRepository.ExistsByHousingTypologyIdAsync` / `ExistsByHousingUnitIdAsync`).

### Kiwbi.Infrastructure

- Configuraciones EF Core para `TradeCategory` y `Customization` en `Persistence/Configurations`, con `OwnsMany` para `CustomizationOption` y `CustomizationAssignment` (incluyendo los índices únicos y únicos parciales descritos arriba).
- Nuevos `DbSet<TradeCategory>` y `DbSet<Customization>` en `KiwbiDbContext` (sin `DbSet` para las entidades hijas).
- `TradeCategoryRepository`, `CustomizationRepository` (este último siempre incluye `Options` y `Assignments` al cargar).
- Migración de EF Core que añade `trade_categories`, `customizations`, `customization_options` y `customization_assignments`.
- Registro de los nuevos repositorios en `AddInfrastructureServices`.

### Kiwbi.Web

- `TradeCategoriesController` (`[Authorize(Roles = "DeveloperAdmin")]`), anidado bajo una promoción (`/HousingPromotions/{promotionId}/TradeCategories/...`): `Index`, `Create`/`Edit` (nombre + fecha límite), `Delete`.
- `CustomizationsController`, anidado bajo un Gremio (`/TradeCategories/{tradeCategoryId}/Customizations/...`): `Index`, `Create` (nombre + primera opción + asignación inicial), `Details`/`Edit` (gestión de Opciones y Asignaciones sobre el mismo agregado), `Delete`.
- Dentro de `Details`/`Edit` de una Personalización: acciones para `AddOption`, `EditOption`, `SetDefaultOption`, `RemoveOption`, `AddTypologyAssignment`, `AddUnitAssignment`, `RemoveAssignment`, todas recargando la vista del agregado tras cada cambio.
- ViewModels de presentación separados de los DTOs de Application. Los controllers no acceden a `KiwbiDbContext` ni a los repositorios directamente.
- Vistas Razor en español: listados de Gremios, formulario de Gremio con selector de fecha y hora, y la vista de detalle de Personalización con tabla de Opciones (con badge de "Por defecto" y botón para fijarla) y selector de asignación (toda la promoción / tipologías / viviendas).

## Plan de Acción (Step-by-Step)

### Feature 3.1 - CRUD de Gremios

- [x] Crear `TradeCategory` en `Kiwbi.Domain.Customizations` con sus invariantes y métodos (`Create`, `Rename`, `Reschedule`, `IsExpired`).
- [x] Crear `ITradeCategoryRepository`.
- [x] Implementar `CreateTradeCategory`, `UpdateTradeCategory`, `GetTradeCategory`, `GetTradeCategories`, `DeleteTradeCategory`, resolviendo el tenant desde `ICurrentUser` vía la `HousingPromotion` padre.
- [x] Ampliar `DeleteHousingPromotionUseCase` para bloquear el borrado si existen `TradeCategory`.
- [x] Crear la configuración EF Core, `DbSet<TradeCategory>` y `TradeCategoryRepository`.
- [x] Generar y aplicar la migración que incorpora `trade_categories`.
- [x] Crear `TradeCategoriesController` y las vistas de listado, alta, edición y baja.

### Feature 3.2 - CRUD de Personalizaciones y asociación condicional

- [x] Crear `Customization`, `CustomizationOption`, `CustomizationAssignment` y `CustomizationScope` en `Kiwbi.Domain.Customizations`, con las invariantes descritas (opción por defecto única, asignación inicial obligatoria, exclusión mutua `WholePromotion` vs. `Typology`/`Unit`).
- [x] Crear `ICustomizationRepository`, incluyendo `GetByTradeCategoryIdAsync`, `ExistsByTradeCategoryIdAsync`, `ExistsByHousingTypologyIdAsync` y `ExistsByHousingUnitIdAsync`.
- [x] Implementar `CreateCustomization` (con opción y asignación inicial), `RenameCustomization`, `GetCustomization`, `GetCustomizations`, `DeleteCustomization`, `AssignCustomizationToTypology`, `AssignCustomizationToUnit`, `RemoveCustomizationAssignment`, validando que el Gremio y la Tipología/Vivienda de destino pertenezcan a la misma promoción del tenant actual.
- [x] Ampliar `DeleteHousingTypologyUseCase` y `DeleteHousingUnitUseCase` (Epic 2) para bloquear el borrado si existe alguna `CustomizationAssignment` que las referencie.
- [x] Crear la configuración EF Core (`OwnsMany` para `Options` y `Assignments`, con sus índices únicos) y `DbSet<Customization>`.
- [x] Implementar `CustomizationRepository`.
- [x] Generar y aplicar la migración que incorpora `customizations` y `customization_assignments`.
- [x] Crear `CustomizationsController` y sus vistas (listado por Gremio, alta con asignación inicial, detalle con gestión de asignaciones, baja).

### Feature 3.3 - CRUD de Opciones por Personalización

- [x] Implementar `AddCustomizationOption`, `UpdateCustomizationOption`, `SetDefaultCustomizationOption`, `RemoveCustomizationOption` sobre el agregado `Customization` ya cargado.
- [x] Incorporar `customization_options` a la configuración EF Core (`OwnsMany`) creada en la Feature 3.2, con su índice único de nombre.
- [x] Extender la vista de detalle de `CustomizationsController` con la tabla de Opciones (nombre, sobrecoste, badge "Por defecto") y las acciones para añadir, editar, fijar por defecto y eliminar opciones.
- [x] Confirmar que no se puede eliminar la única opción restante ni la opción por defecto sin fijar antes otra.

### Cierre del Epic

- [x] Ejecutar los tests unitarios y una compilación completa de la solución (195 tests: 92 Domain + 103 Application, 0 fallos).
- [x] Validar las migraciones contra PostgreSQL de desarrollo.
- [x] Revisar que los controllers solo dependan de contratos de Application y no accedan a `KiwbiDbContext`.
- [x] Actualizar este documento con los checks completados y cualquier decisión técnica aprobada durante la implementación.

## Consideraciones de Testing y Notas de la IA

### Testing

- Priorizar tests de Domain para las invariantes de `TradeCategory` (nombre y fecha límite obligatorios, `IsExpired`) y de `Customization` (siempre al menos una opción, exactamente un `IsDefault`, no se puede eliminar la única opción ni la opción por defecto sin reemplazo, `SurchargeAmount >= 0`, exclusión mutua de asignaciones, no duplicar asignación a la misma Tipología/Vivienda).
- Priorizar tests de Application para: aislamiento de tenant (no se puede editar/consultar un Gremio o Personalización de otra promotora, ni asignar a una Tipología/Vivienda de otra promoción), bloqueo de borrado con dependientes (Promoción con Gremios, Gremio con Personalizaciones, Tipología/Vivienda con asignaciones), y el flujo completo de `SetDefaultCustomizationOption` (la opción anterior deja de ser `IsDefault`).
- Usar sustitutos (NSubstitute) para `ITradeCategoryRepository`, `ICustomizationRepository`, `IHousingPromotionRepository`, `IHousingTypologyRepository`, `IHousingUnitRepository`, `ICurrentUser` e `IUnitOfWork`.
- No usar el proveedor In-Memory de EF Core; mantener el criterio de Epics anteriores de no sustituir PostgreSQL con In-Memory.

### Notas de la IA

- Código y nombres técnicos en inglés; vistas y mensajes en español, igual que en Epics anteriores.
- El tenant (`DeveloperCompanyId`) se deriva siempre del principal autenticado vía `ICurrentUser`, nunca de un valor recibido del cliente, resolviéndolo a través de la cadena `Customization -> TradeCategory -> HousingPromotion`.
- A diferencia de `HousingTypology`/`HousingUnit` (raíces independientes en Epic 2), `CustomizationOption` y `CustomizationAssignment` son entidades hijas del agregado `Customization` sin repositorio propio: toda mutación pasa por métodos de `Customization` y se persiste cargando/guardando el agregado completo (`OwnsMany` en EF Core). Esto es intencional: su invariante ("una única opción por defecto", "sin asignaciones duplicadas o contradictorias") es interna al agregado, a diferencia de las invariantes de Epic 2 que cruzan agregados distintos (Vivienda-Promoción) y por eso se resuelven en Application.
- `CustomizationAssignment` referencia Tipologías/Viviendas por Id de forma dinámica: si se asigna a una Tipología, cualquier Vivienda que se dé de alta después en esa Tipología queda automáticamente cubierta por la Personalización (relevante para Epic 5, no se testea aquí).
- El bloqueo por fecha límite (`SelectionCutOffDateUtc`) y el volcado automático a la opción por defecto al expirar son reglas de Epic 5/6 sobre `HomeCustomizationChoice`; este Epic solo expone `TradeCategory.IsExpired(utcNow)` como bloque de dominio reutilizable, sin consumirlo todavía.
- Antes de programar este Epic, el usuario debe aprobar este documento.

### Decisiones técnicas durante la ejecución (Feature 3.1)

- `DeleteTradeCategoryUseCase` se implementó **sin** dependencia de `ICustomizationRepository` en esta Feature (esa interfaz aún no existe): de momento borra el Gremio sin comprobar Personalizaciones dependientes. Se ampliará en la Feature 3.2, replicando el mismo criterio incremental usado en Epic 2 (`DeleteHousingPromotionUseCase` se amplió en la Feature 2.2 cuando `IHousingTypologyRepository`/`IHousingUnitRepository` empezaron a existir).
- `DeleteHousingPromotionUseCase` sí se amplió ya en esta Feature con `ITradeCategoryRepository.ExistsByHousingPromotionIdAsync`, bloqueando el borrado de una promoción con Gremios asociados.
- El selector de fecha y hora usa `<input type="datetime-local">`: el Input Tag Helper de ASP.NET Core detecta este `type` y formatea automáticamente el valor de un `DateTime` al formato `yyyy-MM-ddTHH:mm:ss.fff` esperado por el control HTML5, sin necesitar formateo manual en la vista. La cultura invariante ya forzada en `Program.cs` (Epic 2) evita conflictos de separador decimal/fecha con el resto de formularios.
- Verificado manualmente: `/TradeCategories?promotionId=...` sin sesión redirige 302 a `/Account/Login`. 135 tests pasando (65 Domain + 70 Application) tras esta Feature.

### Decisiones técnicas durante la ejecución (Feature 3.2)

- `CustomizationOption` y `CustomizationAssignment` se mapearon con `OwnsMany` de EF Core: son colecciones "owned" sin `DbSet` propio, siempre cargadas junto con su `Customization` propietaria (no hace falta `.Include()` explícito). Se configuró `SetPropertyAccessMode(PropertyAccessMode.Field)` en ambas navegaciones para que EF Core lea/escriba directamente los campos privados `_options`/`_assignments` sin necesitar un setter público en las colecciones.
- `Customization.CreateForWholePromotion/CreateForTypologies/CreateForUnits` son tres fábricas estáticas (en vez de un único `Create` con un enum + lista opcional) para que la invariante "al menos una asignación inicial" sea imposible de omitir por construcción: cada fábrica exige sus propios argumentos obligatorios. `CreateForTypologies`/`CreateForUnits` deduplican automáticamente los ids repetidos.
- `ICustomizationRepository.ExistsByHousingTypologyIdAsync`/`ExistsByHousingUnitIdAsync` consultan con `_dbContext.Customizations.SelectMany(c => c.Assignments).AnyAsync(...)`: EF Core traduce la navegación a la colección "owned" `customization_assignments` a un JOIN SQL normal, sin necesitar un `DbSet` propio para esa tabla.
- Se ampliaron `DeleteHousingTypologyUseCase` y `DeleteHousingUnitUseCase` (Epic 2) con `ICustomizationRepository` para bloquear el borrado de una Tipología/Vivienda referenciada por alguna `CustomizationAssignment`, cerrando el hueco de integridad señalado en el análisis inicial del Epic.
- La gestión de Opciones (añadir, editar, fijar por defecto, eliminar) se deja fuera de las vistas de esta Feature a propósito: el dominio ya expone `AddOption`/`UpdateOption`/`SetDefaultOption`/`RemoveOption` (necesarios para que `Create` tenga una opción por defecto), pero los casos de uso y la UI dedicados se implementan en la Feature 3.3, tal como estaba planeado.
- El formulario de alta de Personalización muestra siempre los `<select multiple>` de tipologías y viviendas (sin JavaScript de mostrar/ocultar según el `Scope` elegido), priorizando la simplicidad sobre la interactividad; Alpine.js/HTMX quedan reservados para el Epic 5 según `02-architecture-and-stack.md`.
- Verificado manualmente: `/Customizations?tradeCategoryId=...` sin sesión redirige 302 a `/Account/Login`. 185 tests pasando (92 Domain + 93 Application) tras esta Feature.

### Decisiones técnicas durante la ejecución (Feature 3.3)

- La edición de una Opción se implementó como una página GET/POST dedicada (`EditOption.cshtml`), igual patrón que `TradeCategories/Edit` o `HousingTypologies/Edit`, en vez de una fila editable inline dentro de la tabla: un `<form>` no puede envolver válidamente varias celdas `<td>` de una misma fila sin romper el HTML de la tabla, y esta alternativa mantiene la consistencia con el resto de formularios CRUD del proyecto.
- `AddOption`/`SetDefaultOption`/`RemoveOption` sí se mantienen como acciones POST simples que redirigen de vuelta a `Details`, ya que no necesitan mostrar un formulario propio (solo un input inline para añadir, o botones de acción directa).
- Las invariantes de dominio ya cubrían "no eliminar la única opción" y "no eliminar la opción por defecto" desde la Feature 3.2 (necesarias para que `Customization.Create...` garantizase una opción por defecto inicial); esta Feature solo expuso los casos de uso y la UI que las ejercitan.
- Verificado manualmente: `/Customizations/EditOption?...` sin sesión redirige 302 a `/Account/Login`. 195 tests pasando (92 Domain + 103 Application) tras esta Feature; Epic 3 completo.
