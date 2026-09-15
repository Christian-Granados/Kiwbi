# Epic 05: Buyer Experience (Frontend HTMX)

## Estado

- Estado: Propuesto, pendiente de aprobación del usuario. No implementado.
- Depende de: Epic 1 (Foundation & Promotora Tenant), Epic 2 (Real Estate Core), Epic 3 (Customization Engine) y Epic 4 (Onboarding B2B2C), ya implementados. Reutiliza `HousingUnit`/`HousingTypology`/`HousingPromotion` (Epic 2), `TradeCategory`/`Customization`/`CustomizationOption`/`CustomizationAssignment` (Epic 3) y `HousingUnitBuyer` (Epic 4) como datos de solo lectura; no modifica ninguna de esas entidades.

## Objetivos

Dar al Comprador, ya vinculado a una o varias Viviendas (Epic 4), una experiencia propia para ver sus Viviendas, visualizar las Personalizaciones que le aplican agrupadas por Gremio, y seleccionar sus opciones de forma interactiva (HTMX, sin recargar página), respetando la fecha límite de selección de cada Gremio.

El Epic cubre las siguientes Features del roadmap:

- **Feature 5.1 - Dashboard del Comprador:** listado de "Mis Viviendas" (las Viviendas vinculadas al comprador autenticado vía `HousingUnitBuyer`).
- **Feature 5.2 - Visualizador de Personalizaciones:** para una Vivienda concreta, resolver y mostrar qué Personalizaciones le aplican (por Promoción completa, por su Tipología o por la Vivienda en sí), agrupadas por Gremio, con sus Opciones, sobrecostes y estado de selección actual.
- **Feature 5.3 - Selección interactiva (HTMX):** el comprador elige una Opción por Personalización; el guardado se hace vía HTMX sin recargar la página completa, actualizando solo la tarjeta de esa Personalización.
- **Feature 5.4 - Bloqueo por fecha límite:** si la fecha límite (`SelectionCutOffDateUtc`) del Gremio ya pasó, la Personalización queda en solo lectura mostrando la opción por defecto como opción efectiva, y el servidor rechaza cualquier intento de selección aunque se manipule la petición HTMX desde el cliente.

Queda explícitamente fuera de alcance: el panel de gestión de la promotora sobre el progreso de las elecciones (Epic 6, Feature 6.1), los estados manuales `Confirmed`/`Paid` (Epic 6, Feature 6.2 — el enum los define desde ya, pero sin casos de uso hasta entonces) y la exportación de reportes (Epic 6, Feature 6.3).

## Decisiones confirmadas por el usuario

1. **Modelo de la Elección:** una fila por cada par `(HousingUnitId, CustomizationId)`, con `SelectedOptionId` (nullable hasta que el comprador elige por primera vez) y `Status`. El comprador puede cambiar de opción libremente mientras el Gremio no haya llegado a su fecha límite; cada cambio actualiza `SelectedOptionId` y pasa `Status` a `Selected`.
2. **Bloqueo por fecha límite:** no se escribe nada en base de datos automáticamente al pasar el cut-off. La "opción efectiva" (`SelectedOptionId` si existe, si no la opción `IsDefault` de la Personalización) se calcula en el momento de la lectura, tanto en la vista del comprador como en los futuros reportes de Epic 6. Esto evita escrituras fantasma disparadas por un simple `GET` y mantiene el histórico real de lo que el comprador eligió (o no eligió) explícitamente.
3. **Alcance de `Confirmed`/`Paid`:** el enum `HomeCustomizationChoiceStatus` incluye ya los cuatro valores del modelo de dominio (`Pending`, `Selected`, `Confirmed`, `Paid`), pero Epic 5 solo implementa las transiciones `Pending -> Selected` (lado comprador). `Confirmed`/`Paid` quedan sin casos de uso hasta Epic 6, mismo patrón incremental que `HousingUnitStatus` en Epic 2.
4. **Alcance del Dashboard (Feature 5.1):** listado mínimo de "Mis Viviendas" con datos básicos (promoción, ciudad, planta, puerta, plano si existe) y enlace al visualizador de cada vivienda; sin totales de sobrecoste todavía.

## Análisis Técnico

### Resolución de Personalizaciones aplicables a una Vivienda

Dada una `HousingUnit` (con su `HousingPromotionId` y `HousingTypologyId` opcional), una Personalización le aplica si tiene una `CustomizationAssignment` que cumpla:

- `Scope == WholePromotion`, o
- `Scope == Typology` y `HousingTypologyId == unit.HousingTypologyId` (si la vivienda no tiene tipología, nunca aplica), o
- `Scope == Unit` y `HousingUnitId == unit.Id`.

Esta resolución se hace en Application (no en Domain, coherente con el criterio ya usado en Epics 2/3 de que las comprobaciones que cruzan agregados viven en casos de uso): se cargan las `TradeCategory` de la promoción de la vivienda, y para cada una sus `Customization` (ya con `Options`/`Assignments` incluidos por ser agregado con `OwnsMany`), filtrando por la regla anterior.

### Nueva entidad transaccional: Elección (HomeCustomizationChoice)

Se crea el módulo `Kiwbi.Domain.Choices`, con una raíz de agregado independiente (mismo criterio que `HousingUnitBuyer` en Epic 4: ancla por id a entidades de otros módulos, sin navegación de colección):

| Elemento | Tipo | Responsabilidad | Propiedades iniciales |
| --- | --- | --- | --- |
| `HomeCustomizationChoice` | Entidad / raíz | Relaciona una Vivienda, una Personalización y (opcionalmente) la Opción elegida por el comprador. | `Id`, `HousingUnitId`, `CustomizationId`, `SelectedOptionId` (opcional), `Status` (enum), `SelectedAtUtc` (opcional), `CreatedAtUtc`, `UpdatedAtUtc` |
| `HomeCustomizationChoiceStatus` | Enum | Ciclo de vida de la elección. | `Pending`, `Selected`, `Confirmed`, `Paid` |

Comportamiento:

- `HomeCustomizationChoice.Create(housingUnitId, customizationId)`: nace en `Pending`, sin `SelectedOptionId`. Solo se invoca la primera vez que el comprador selecciona algo para ese par (no al visualizar); ver "Notas de la IA" sobre por qué no se materializa antes.
- `HomeCustomizationChoice.SelectOption(customizationOptionId, utcNow)`: fija `SelectedOptionId`, pasa `Status` a `Selected` y `SelectedAtUtc = utcNow`. No valida aquí que la opción pertenezca a la Personalización ni que el Gremio siga abierto (son invariantes cruzadas de agregado, verificadas en Application antes de llamar a este método, mismo criterio que otras invariantes cruzadas de Epics anteriores).

Invariantes de Domain (lanzan `DomainException`): `HousingUnitId`/`CustomizationId` obligatorios en `Create`; `customizationOptionId` no puede ser `Guid.Empty` en `SelectOption`.

### Relaciones y esquema relacional

| Tabla | Procedencia | Columnas relevantes | Relaciones |
| --- | --- | --- | --- |
| `home_customization_choices` | Dominio / EF Core | `id`, `housing_unit_id`, `customization_id`, `selected_option_id` (nullable), `status`, `selected_at_utc` (nullable), `created_at_utc`, `updated_at_utc` | FK obligatoria a `housing_units.id` (borrado restringido); FK obligatoria a `customizations.id` (borrado restringido). Índice único en `(housing_unit_id, customization_id)`. `selected_option_id` se guarda como columna simple **sin FK de base de datos** hacia `customization_options`, porque esa tabla es un `OwnsMany` gestionado íntegramente por el agregado `Customization` (las opciones pueden añadirse/eliminarse por sus propios métodos de dominio); la validez de `selected_option_id` se comprueba en Application al seleccionar, no con una restricción de integridad referencial. |

`Status` se persiste como cadena (`HasConversion<string>()`), mismo criterio que `BuyerInvitationStatus`/`HousingUnitStatus`/`CustomizationScope`.

## Impacto en Arquitectura

### Kiwbi.Domain

Nuevo módulo `Choices`:

- `HomeCustomizationChoice`, `HomeCustomizationChoiceStatus`.
- `IHomeCustomizationChoiceRepository : IRepository<HomeCustomizationChoice>` con `GetByHousingUnitIdAsync` (todas las elecciones de una vivienda, para el visualizador), `GetByHousingUnitIdAndCustomizationIdAsync` (para leer/crear una elección concreta), `ExistsByCustomizationIdAsync` (para bloquear el borrado de una `Customization` con elecciones ya registradas, mismo patrón incremental de Epic 3/4).

Cambio sobre un caso de uso existente de Epic 3:

- `DeleteCustomizationUseCase` (si existe una elección registrada, aunque sea de una Personalización que ya no aplica, no debería poder eliminarse silenciosamente) amplía su comprobación de dependientes con `IHomeCustomizationChoiceRepository.ExistsByCustomizationIdAsync`.

El proyecto sigue sin referenciar EF Core ni Identity.

### Kiwbi.Application

Nuevo módulo `Choices`, con casos de uso organizados por intención (todos los de este módulo son de comprador, anclados en `ICurrentUser.UserId`, y validan primero que la Vivienda esté vinculada al comprador actual vía `IHousingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync`):

- **`GetHousingUnitsForCurrentBuyerUseCase`** (Feature 5.1): usa `IHousingUnitBuyerRepository.GetByBuyerUserIdAsync` para obtener las viviendas del comprador, carga cada `HousingUnit` y su `HousingPromotion` padre, y devuelve `BuyerHousingUnitDto` (promoción, ciudad, planta, puerta, ruta del plano).
- **`GetHousingUnitCustomizationsForBuyerUseCase`** (Feature 5.2): valida la vinculación comprador-vivienda, resuelve las Personalizaciones aplicables (ver más arriba) agrupadas por Gremio, y para cada una añade la elección existente (si la hay) y la opción efectiva calculada (`SelectedOptionId` si existe, si no `IsDefault` cuando el Gremio ya expiró, si no `null`). Devuelve una lista de `TradeCategoryCustomizationsDto` (cada uno con su lista de `CustomizationForBuyerDto`, que a su vez lista `CustomizationOptionForBuyerDto`).
- **`SelectCustomizationOptionUseCase`** (Feature 5.3/5.4, Command: `HousingUnitId`, `CustomizationId`, `CustomizationOptionId`): valida vinculación comprador-vivienda; carga la `Customization`, comprueba que la opción pertenece a ella y que alguna de sus asignaciones aplica a la vivienda (reutilizando la misma regla de resolución); carga la `TradeCategory` y comprueba `!IsExpired(utcNow)` (si ha expirado, `Result.Failure`, verificación de servidor independiente de lo que muestre la UI); obtiene o crea (`GetByHousingUnitIdAndCustomizationIdAsync` -> si no existe, `HomeCustomizationChoice.Create(...)`) la elección y llama a `SelectOption`; persiste con `IUnitOfWork`.

Nuevos DTOs en `Kiwbi.Application.Choices`: `BuyerHousingUnitDto`, `TradeCategoryCustomizationsDto`, `CustomizationForBuyerDto`, `CustomizationOptionForBuyerDto`.

Todos los casos de uso devuelven `Result`/`Result<T>`, reciben `CancellationToken` y se registran desde `AddApplicationServices`.

### Kiwbi.Infrastructure

- Configuración EF Core para `HomeCustomizationChoice` (tabla `home_customization_choices`) en `Persistence/Configurations`, incluyendo el índice único `(housing_unit_id, customization_id)`.
- Nuevo `DbSet<HomeCustomizationChoice>` en `KiwbiDbContext`.
- `HomeCustomizationChoiceRepository`.
- Migración de EF Core que añade `home_customization_choices`.
- Registro del nuevo repositorio en `AddInfrastructureServices`.
- Se añade `htmx.org` y `Alpine.js` como recursos estáticos servidos localmente (mismo criterio que `bootstrap`/`jquery`, gestionados vía LibMan en `wwwroot/lib`, no CDN, para no depender de disponibilidad externa ni introducir SRI/CSP adicionales) — se referencian solo desde el layout/vistas del Comprador, no desde el panel de la promotora.

### Kiwbi.Web

- Nuevo `BuyerController` (`[Authorize(Roles = "Buyer")]`, primer controller de esta zona; distinto de los controllers existentes que son todos de promotora):
  - `Index` (Feature 5.1): dashboard "Mis Viviendas".
  - `HousingUnit(Guid id)` (Feature 5.2): visualizador de una vivienda concreta, agrupado por Gremio, con las tarjetas de Personalización (usa `GetHousingUnitCustomizationsForBuyerUseCase`).
  - `SelectOption(Guid housingUnitId, Guid customizationId, Guid customizationOptionId)` (Feature 5.3/5.4, `[HttpPost]`): invoca `SelectCustomizationOptionUseCase` y devuelve una `PartialView` con la tarjeta de esa Personalización ya actualizada (patrón HTMX: `hx-post` en cada opción, `hx-target`/`hx-swap` apuntando al contenedor de la tarjeta), o la misma tarjeta con un mensaje de error si el gremio ya expiró o la opción no es válida.
- ViewModels de presentación separados de los DTOs de Application (`BuyerHousingUnitViewModel`, etc., si aportan valor sobre el DTO; si son un mapeo 1:1 sin lógica de presentación adicional, se reutiliza el DTO directamente en la vista, mismo criterio pragmático que controllers anteriores).
- Vistas Razor en español: `Buyer/Index.cshtml` (listado de viviendas), `Buyer/HousingUnit.cshtml` (visualizador con las Personalizaciones agrupadas por Gremio) y una `PartialView` `_CustomizationCard.cshtml` reutilizada tanto en la carga inicial como en la respuesta HTMX de `SelectOption`.
- `_Layout.cshtml` incorpora los scripts de `htmx.org`/`Alpine.js` (primera vez que se usan en el proyecto); las tarjetas de Personalización en solo lectura (Gremio expirado) no incluyen los atributos `hx-*` de selección, solo muestran la opción efectiva con una nota indicando que el plazo ha finalizado.

## Plan de Acción (Step-by-Step)

### Feature 5.1 - Dashboard del Comprador (Mis Viviendas)

- [ ] Crear `GetHousingUnitsForCurrentBuyerUseCase` y `BuyerHousingUnitDto` en `Kiwbi.Application.Choices` (o en `Kiwbi.Application.Onboarding`, reutilizando `IHousingUnitBuyerRepository` ya existente de Epic 4; decidir ubicación al implementar según si depende de algo del nuevo módulo `Choices`).
- [ ] Crear `BuyerController` (`[Authorize(Roles = "Buyer")]`) con la acción `Index` y su vista.
- [ ] Tests de Application: comprador sin viviendas ve lista vacía; comprador con varias viviendas (de distintas promociones) las ve todas; una vivienda de otro comprador nunca aparece.

### Feature 5.2 - Visualizador de Personalizaciones

- [ ] Crear `Kiwbi.Domain.Choices`: `HomeCustomizationChoice`, `HomeCustomizationChoiceStatus`, `IHomeCustomizationChoiceRepository`.
- [ ] Crear la configuración EF Core, `DbSet<HomeCustomizationChoice>` y `HomeCustomizationChoiceRepository`.
- [ ] Generar y aplicar la migración que incorpora `home_customization_choices`.
- [ ] Implementar `GetHousingUnitCustomizationsForBuyerUseCase` (resolución de Personalizaciones aplicables agrupadas por Gremio + cálculo de opción efectiva) y sus DTOs.
- [ ] Añadir `IHomeCustomizationChoiceRepository.ExistsByCustomizationIdAsync` a la comprobación de dependientes de `DeleteCustomizationUseCase` (Epic 3).
- [ ] Añadir la acción `HousingUnit` a `BuyerController` y su vista, agrupando por Gremio.
- [ ] Tests de Application: resolución correcta de Personalizaciones por `WholePromotion`/`Typology`/`Unit`; vivienda sin tipología ignora asignaciones de tipología; comprador no vinculado a la vivienda no puede consultarla; cálculo de opción efectiva (elección existente vs. opción por defecto tras cut-off vs. sin elección con Gremio todavía abierto).

### Feature 5.3 - Selección interactiva con HTMX

- [ ] Añadir `htmx.org`/`Alpine.js` vía LibMan a `wwwroot/lib` y referenciarlos en `_Layout.cshtml`.
- [ ] Implementar `SelectCustomizationOptionUseCase` (validación de vinculación, pertenencia de la opción, aplicabilidad de la personalización a la vivienda, creación/actualización de `HomeCustomizationChoice`).
- [ ] Añadir la acción `SelectOption` (`[HttpPost]`) a `BuyerController` devolviendo la `PartialView` `_CustomizationCard.cshtml` actualizada.
- [ ] Maquetar `_CustomizationCard.cshtml` con los atributos `hx-post`/`hx-target`/`hx-swap` sobre cada opción seleccionable.
- [ ] Tests de Application: selección válida crea la elección la primera vez y la actualiza en selecciones posteriores; opción de otra Personalización rechazada; Personalización no aplicable a la vivienda rechazada.

### Feature 5.4 - Bloqueo por fecha límite

- [ ] Verificar en `GetHousingUnitCustomizationsForBuyerUseCase` que las Personalizaciones de un Gremio expirado se marcan como no seleccionables (`CanSelect = false`) y muestran la opción efectiva calculada.
- [ ] Verificar en `SelectCustomizationOptionUseCase` el rechazo server-side de selecciones sobre Gremios expirados, independientemente de lo que el cliente envíe.
- [ ] Ajustar `_CustomizationCard.cshtml` para no renderizar los atributos `hx-*` de selección cuando `CanSelect == false`, mostrando en su lugar la opción efectiva y un aviso de plazo finalizado.
- [ ] Tests de Application: Gremio expirado sin elección previa expone la opción por defecto como efectiva y de solo lectura; Gremio expirado con elección previa expone esa elección (no la cambia a la opción por defecto); intento de `SelectCustomizationOptionUseCase` sobre Gremio expirado devuelve `Result.Failure` y no modifica ninguna fila.

### Cierre del Epic

- [ ] Ejecutar los tests unitarios y una compilación completa de la solución.
- [ ] Validar la migración contra PostgreSQL de desarrollo.
- [ ] Revisar que `BuyerController` solo dependa de contratos de Application y no acceda a `KiwbiDbContext`.
- [ ] Verificar manualmente el flujo completo: login como comprador (aceptando una invitación de Epic 4), ver el dashboard, entrar al visualizador de una vivienda, seleccionar/cambiar opciones vía HTMX sin recarga, y comprobar el bloqueo de un Gremio con `SelectionCutOffDateUtc` en el pasado.
- [ ] Actualizar este documento con los checks completados y cualquier decisión técnica aprobada durante la implementación.

## Consideraciones de Testing y Notas de la IA

### Testing

- Priorizar tests de Domain para `HomeCustomizationChoice` (campos obligatorios en `Create`, `SelectOption` actualiza `SelectedOptionId`/`Status`/`SelectedAtUtc`).
- Priorizar tests de Application para: aislamiento de comprador (una vivienda de otro comprador nunca es visible ni seleccionable), la regla de resolución de Personalizaciones aplicables (`WholePromotion`/`Typology`/`Unit`, incluyendo el caso de vivienda sin tipología), el cálculo de opción efectiva antes/después del cut-off, y el rechazo server-side de selecciones sobre Personalizaciones no aplicables/opciones ajenas/Gremios expirados.
- Usar sustitutos (NSubstitute) para `IHomeCustomizationChoiceRepository`, `IHousingUnitBuyerRepository`, `IHousingUnitRepository`, `IHousingPromotionRepository`, `ITradeCategoryRepository`, `ICustomizationRepository`, `ICurrentUser` e `IUnitOfWork`.
- No usar el proveedor In-Memory de EF Core; mantener el criterio de Epics anteriores de no sustituir PostgreSQL con In-Memory.

### Notas de la IA

- Código y nombres técnicos en inglés; vistas y mensajes en español, igual que en Epics anteriores.
- `HomeCustomizationChoice` no se crea al visualizar (Feature 5.2), solo al seleccionar por primera vez (Feature 5.3): un `GET` nunca debe tener efectos secundarios de escritura; la "opción efectiva" para una Personalización sin elección registrada se calcula igualmente en memoria a partir de `IsDefault`/cut-off, sin necesidad de una fila en base de datos. Epic 6 (progreso/reportes) deberá reutilizar esta misma lógica de resolución de opción efectiva en lugar de asumir que existe una fila por cada par vivienda-Personalización.
- La comprobación de que una Vivienda pertenece al comprador autenticado se hace siempre contra `HousingUnitBuyer` (Epic 4), nunca confiando en un `housingUnitId` recibido del cliente sin verificar la vinculación primero — igual criterio de "nunca confiar en identificadores recibidos del navegador sin comprobar propiedad/tenant" ya aplicado en Epics 2-4 para la promotora.
- El bloqueo por fecha límite se verifica siempre en el servidor dentro de `SelectCustomizationOptionUseCase`, no solo ocultando los controles `hx-*` en la vista: un comprador podría en teoría disparar la petición HTMX manualmente después de que el plazo expire mientras tenía la página abierta.
- `htmx.org`/`Alpine.js` se instalan vía LibMan igual que `bootstrap`/`jquery` (no CDN), para mantener consistencia con la gestión de dependencias de front-end ya usada en el proyecto.
- Antes de programar este Epic, el usuario debe aprobar este documento.
