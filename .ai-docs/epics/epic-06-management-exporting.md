# Epic 06: Management & Exporting

## Estado

- Estado: Aprobado por el usuario (2026-09-17). Features 6.1, 6.2 y 6.3 implementadas y con tests en verde. Epic 6 **fully closed** (2026-09-20) — la verificación manual pendiente de apertura de los ficheros Excel/PDF con datos reales se completó en el Epic 10 (ver `epic-10-demo-data-and-report-verification.md`), sin defectos encontrados.
- Depende de: Epic 1 (Foundation & Promotora Tenant), Epic 2 (Real Estate Core), Epic 3 (Customization Engine) y Epic 5 (Buyer Experience), ya implementados. Reutiliza `HousingUnit`/`HousingTypology`/`HousingPromotion` (Epic 2), `TradeCategory`/`Customization`/`CustomizationOption`/`CustomizationAssignment` (Epic 3) y `HomeCustomizationChoice` (Epic 5, módulo `Kiwbi.Domain.Choices`) como base; no depende de Epic 4 más allá de que la Vivienda ya tenga comprador vinculado para que existan elecciones que gestionar.

## Objetivos

Dar a la Promotora visibilidad y control administrativo sobre las elecciones de personalización de sus compradores, y la capacidad de exportar esa información para la constructora. El Epic cubre las siguientes Features del roadmap:

- **Feature 6.1 - Panel de progreso:** vista por Promoción con el estado de las elecciones de cada Vivienda (cuántas Personalizaciones están `Pending`/`Selected`/`Confirmed`/`Paid`), con detalle al entrar en una Vivienda concreta (agrupado por Gremio, mismo criterio visual que el visualizador del comprador de Epic 5).
- **Feature 6.2 - Gestión de estados manuales:** la Promotora puede marcar una elección como `Confirmed` (visto/aceptado con la constructora) y, más tarde, como `Paid` (sobrecoste cobrado). Son transiciones **unidireccionales**: `Confirmed` sólo se puede marcar tras el cut-off del Gremio, y `Paid` sólo tras estar `Confirmed`. No hay reversión desde la UI en este Epic.
- **Feature 6.3 - Exportación de reportes ("Libro de Obra"):** exportación en Excel y PDF de una Promoción completa, agrupada por Gremio y luego por Vivienda, con la opción efectiva de cada Personalización, su sobrecoste y su estado.

Queda explícitamente fuera de alcance: revertir estados (`Paid -> Confirmed`, etc. — se deja para un futuro caso de uso de corrección si hace falta), exportaciones parciales (por Vivienda o por Gremio sueltos), y cualquier notificación/email asociado a estos cambios de estado.

## Decisiones confirmadas por el usuario

1. **Confirmar sin elección explícita del comprador:** hoy una fila de `HomeCustomizationChoice` solo existe en BD si el comprador seleccionó algo explícitamente (Epic 5, Feature 5.3); si el Gremio expira sin que el comprador eligiera, la "opción efectiva" (la `IsDefault`) se calculaba solo en memoria, sin persistirse. En Epic 6, **confirmar materializa esa fila si no existe**, fijando `SelectedOptionId` a la opción por defecto de la Personalización antes de pasar a `Confirmed`. Esto solo puede ocurrir **después** de que el Gremio haya llegado a su `SelectionCutOffDateUtc` — antes de esa fecha no se puede confirmar nada (el comprador todavía puede cambiar de opinión).
2. **Transiciones de estado:** estrictamente hacia adelante: `Pending` (sin fila, o fila sin `Confirm`) → `Confirmed` → `Paid`. No se contempla revertir un estado ya confirmado o pagado desde la aplicación en este Epic.
3. **Alcance del panel 6.1:** resumen agregado por Promoción (contadores de elecciones por estado, por Vivienda) con drill-down a una vista de detalle por Vivienda concreta.
4. **Librerías de exportación (6.3):** `ClosedXML` (Excel, licencia MIT) y `QuestPDF` (PDF, licencia Community — gratuita por debajo del umbral de facturación anual que aplica a este proyecto; requiere fijar `QuestPDF.Settings.License = LicenseType.Community` una vez en el arranque de `Kiwbi.Web`).
5. **Granularidad de la exportación:** un único export cubre la Promoción completa, agrupado primero por Gremio y luego por Vivienda dentro de cada Gremio (coincide con el paso 10 del flujo de usuario del roadmap: "Libro de Obra ordenado por gremios").

## Análisis Técnico

### Resolución de "todas las Personalizaciones aplicables", ahora a nivel de Promoción

Epic 5 (Feature 5.2) ya resuelve, para **una** Vivienda, qué Personalizaciones le aplican (por `WholePromotion`/`Typology`/`Unit`) usando `Customization.AppliesToHousingUnit(...)`. Epic 6 necesita el mismo cálculo pero para **todas** las Viviendas de una Promoción a la vez (panel 6.1 y export 6.3), por lo que se reutiliza tal cual, iterando:

```
para cada TradeCategory de la Promoción:
  para cada Customization de ese TradeCategory:
    para cada HousingUnit de la Promoción:
      si customization.AppliesToHousingUnit(unit.Id, unit.HousingTypologyId):
        resolver la fila HomeCustomizationChoice (si existe) -> estado + opción efectiva
```

Para evitar repetir el N+1 de "una consulta de elección por cada par (Vivienda, Personalización)" que Epic 5 aceptó a escala de una sola Vivienda, se añade una consulta a granel:

- `IHomeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(IEnumerable<Guid> housingUnitIds)`: trae todas las elecciones de un conjunto de Viviendas en una sola consulta; el use case las indexa en memoria por `(HousingUnitId, CustomizationId)` antes de recorrer el bucle anterior. Sigue habiendo un bucle en memoria (aceptable a escala de panel de administración, mismo criterio que Epic 7), pero se elimina el N+1 contra la base de datos.

### Estado "efectivo" de una Personalización para una Vivienda (vista Promotora)

A diferencia del comprador (que solo ve una "opción efectiva", Epic 5), la Promotora necesita ver también el **estado administrativo**:

| Situación | Estado mostrado | Opción efectiva mostrada |
| --- | --- | --- |
| No existe fila `HomeCustomizationChoice` y el Gremio sigue abierto | `Pending` | — (todavía no aplica ninguna) |
| No existe fila y el Gremio ya expiró | `Pending` (aún no confirmado por la promotora) | Opción `IsDefault` (calculada en memoria, no persistida) |
| Existe fila con `Status = Selected` | `Selected` | `SelectedOptionId` |
| Existe fila con `Status = Confirmed` | `Confirmed` | `SelectedOptionId` |
| Existe fila con `Status = Paid` | `Paid` | `SelectedOptionId` |

Este cálculo se extrae a un mapper interno de Application (`HomeCustomizationChoiceProgressMapper`, en el módulo `Kiwbi.Application.Choices` junto al `CustomizationForBuyerMapper` de Epic 5) para no duplicar la lógica entre el panel (6.1) y el export (6.3).

### Nuevos métodos de dominio en `HomeCustomizationChoice`

```csharp
public void Confirm(DateTime utcNow)
{
    if (SelectedOptionId is null)
    {
        throw new DomainException("No se puede confirmar una elección sin una opción fijada.");
    }

    if (Status is HomeCustomizationChoiceStatus.Confirmed or HomeCustomizationChoiceStatus.Paid)
    {
        throw new DomainException("La elección ya ha sido confirmada.");
    }

    Status = HomeCustomizationChoiceStatus.Confirmed;
    UpdatedAtUtc = utcNow;
}

public void MarkAsPaid(DateTime utcNow)
{
    if (Status != HomeCustomizationChoiceStatus.Confirmed)
    {
        throw new DomainException("Solo se puede marcar como pagada una elección ya confirmada.");
    }

    Status = HomeCustomizationChoiceStatus.Paid;
    UpdatedAtUtc = utcNow;
}
```

`Confirm` no comprueba aquí si el Gremio ha expirado (comprobación cruzada de agregado con `TradeCategory`, se hace en el Application use case, mismo criterio ya usado para otras invariantes cruzadas en Epics 2-5). El use case es responsable de, si no existe fila o `SelectedOptionId` es `null`, llamar primero a `SelectOption(defaultOptionId, utcNow)` con la opción por defecto antes de invocar `Confirm`.

### Relaciones y esquema relacional

Sin cambios de esquema respecto a Epic 5: se reutiliza `home_customization_choices` tal cual. No se necesitan columnas ni tablas nuevas para 6.1/6.2. El export (6.3) no persiste nada, es una lectura agregada convertida a bytes.

## Impacto en Arquitectura

### Kiwbi.Domain

En el módulo existente `Choices`:

- `HomeCustomizationChoice`: nuevos métodos `Confirm(DateTime utcNow)` y `MarkAsPaid(DateTime utcNow)` (ver arriba).
- `IHomeCustomizationChoiceRepository`: nuevo método `GetByHousingUnitIdsAsync(IEnumerable<Guid> housingUnitIds, CancellationToken)`.

No se crean módulos de Domain nuevos ni entidades nuevas.

### Kiwbi.Application

Todo dentro del módulo existente `Kiwbi.Application.Choices` (misma carpeta que Epic 5, ya que sigue girando en torno a `HomeCustomizationChoice`; no se crea un módulo "Management" separado, coherente con el criterio de organizar Application por entidad de dominio, no por actor):

- **`HomeCustomizationChoiceProgressMapper`** (interno, análogo a `CustomizationForBuyerMapper`): dado un `Customization`, una lista de `HomeCustomizationChoice` indexada y si el `TradeCategory` está expirado, calcula estado + opción efectiva por Vivienda (tabla de la sección anterior).
- **`GetHousingPromotionChoicesProgressUseCase`** (Feature 6.1, resumen): input `housingPromotionId`. Valida que la Promoción pertenece al `DeveloperCompanyId` del usuario actual (mismo patrón que `GetHousingPromotionSummaryUseCase` de Epic 2). Devuelve `HousingPromotionChoicesProgressDto`: por cada `HousingUnit`, contadores `PendingCount`/`SelectedCount`/`ConfirmedCount`/`PaidCount` sobre el total de Personalizaciones que le aplican.
- **`GetHousingUnitChoicesDetailUseCase`** (Feature 6.1, drill-down): input `housingUnitId`. Valida ownership resolviendo la Promoción padre (mismo patrón que casos de uso de Epic 2/3 sobre `HousingUnit`). Devuelve una lista de `TradeCategoryChoicesDto` (cada uno con sus `CustomizationChoiceDto`: nombre, estado, opción efectiva, sobrecoste, y si el Gremio está expirado) — misma forma que el visualizador del comprador pero con `Status` añadido y sin el flag `CanSelect` (no aplica, es una vista de solo lectura para la promotora).
- **`ConfirmHomeCustomizationChoiceUseCase`** (Feature 6.2, Command: `HousingUnitId`, `CustomizationId`): valida ownership; carga `TradeCategory` de la Personalización y comprueba `IsExpired(utcNow)` (si no ha expirado, `Result.Failure`); carga la `Customization` (para la opción `IsDefault` si hace falta); obtiene o crea (`GetByHousingUnitIdAndCustomizationIdAsync` → si no existe, `HomeCustomizationChoice.Create(...)`) la elección; si `SelectedOptionId` es `null`, llama a `SelectOption(defaultOptionId, utcNow)`; llama a `Confirm(utcNow)`; persiste con `IUnitOfWork`.
- **`MarkHomeCustomizationChoiceAsPaidUseCase`** (Feature 6.2, Command: `HousingUnitId`, `CustomizationId`): valida ownership; carga la elección existente (si no existe o no está `Confirmed`, `Result.Failure` — no se puede pagar algo que no se ha confirmado); llama a `MarkAsPaid(utcNow)`; persiste.
- **`ExportHousingPromotionReportUseCase`** (Feature 6.3, input: `housingPromotionId`, `format` enum `Excel`/`Pdf`): valida ownership; construye el mismo agregado en memoria que 6.1 pero con el detalle completo (Gremio → Vivienda → Personalización → opción efectiva/sobrecoste/estado); delega la generación de bytes al puerto `IHousingPromotionReportGenerator` (ver Infrastructure); devuelve `Result<GeneratedReportDto>` (`byte[] Content`, `string FileName`, `string ContentType`).

Nuevo puerto en `Kiwbi.Application.Common`:

```csharp
public interface IHousingPromotionReportGenerator
{
    byte[] GenerateExcel(HousingPromotionReportDto report);
    byte[] GeneratePdf(HousingPromotionReportDto report);
}
```

Nuevos DTOs en `Kiwbi.Application.Choices`: `HousingPromotionChoicesProgressDto`, `HousingUnitChoicesProgressDto`, `TradeCategoryChoicesDto`, `CustomizationChoiceDto`, `HousingPromotionReportDto` (y sus tipos anidados por Gremio/Vivienda), `GeneratedReportDto`.

Todos los casos de uso devuelven `Result`/`Result<T>`, reciben `CancellationToken` y se registran desde `AddApplicationServices`.

### Kiwbi.Infrastructure

- Nuevos paquetes NuGet: `ClosedXML` y `QuestPDF`, añadidos al `.csproj` de `Kiwbi.Infrastructure`.
- Nueva carpeta `Reporting/` con `HousingPromotionReportGenerator : IHousingPromotionReportGenerator` (un único adaptador con los dos métodos, cada uno usando su librería correspondiente — no se crean dos clases separadas porque ambos métodos comparten el mismo DTO de entrada y no tienen estado propio).
- `IHomeCustomizationChoiceRepository.GetByHousingUnitIdsAsync` implementado en `HomeCustomizationChoiceRepository` con un único `WHERE housing_unit_id IN (...)`.
- Registro de `IHousingPromotionReportGenerator` en `AddInfrastructureServices`.
- `QuestPDF.Settings.License = LicenseType.Community;` se fija una vez, en `Program.cs` de `Kiwbi.Web` (o en `AddInfrastructureServices`, junto al resto de configuración de arranque — se decide durante la implementación cuál es más consistente con el resto del proyecto).

### Kiwbi.Web

- Nuevo `HousingPromotionChoicesController` (`[Authorize(Roles = "DeveloperAdmin")]`, nested bajo la Promoción vía `promotionId` en query string, mismo patrón que `TradeCategoriesController`/`HousingTypologiesController`):
  - `Index(Guid promotionId)` (Feature 6.1): panel de progreso por Vivienda.
  - `Details(Guid housingUnitId)` (Feature 6.1): drill-down agrupado por Gremio.
  - `Confirm(Guid housingUnitId, Guid customizationId)` (`[HttpPost]`, Feature 6.2): invoca `ConfirmHomeCustomizationChoiceUseCase`, redirige de vuelta a `Details`.
  - `MarkAsPaid(Guid housingUnitId, Guid customizationId)` (`[HttpPost]`, Feature 6.2): invoca `MarkHomeCustomizationChoiceAsPaidUseCase`, redirige de vuelta a `Details`.
  - `ExportExcel(Guid promotionId)` / `ExportPdf(Guid promotionId)` (Feature 6.3): invocan `ExportHousingPromotionReportUseCase` con el `format` correspondiente y devuelven `File(bytes, contentType, fileName)`.
- Vistas Razor en español: `HousingPromotionChoices/Index.cshtml` (tabla de Viviendas con badges de contadores por estado, enlaces a export Excel/PDF) y `HousingPromotionChoices/Details.cshtml` (tarjetas agrupadas por Gremio, con botones "Confirmar"/"Marcar como pagada" quc solo se muestran cuando la transición es válida — p. ej. "Confirmar" solo si el Gremio ya expiró y el estado actual no es ya `Confirmed`/`Paid`; "Marcar como pagada" solo si el estado es `Confirmed`).
- Enlace añadido desde `HousingPromotions/Details.cshtml` (Epic 2) hacia el nuevo panel, mismo criterio que los enlaces ya existentes a Tipologías/Gremios.

## Plan de Acción (Step-by-Step)

### Feature 6.1 - Panel de progreso

- [x] Añadir `GetByHousingUnitIdsAsync` a `IHomeCustomizationChoiceRepository` + implementación en `HomeCustomizationChoiceRepository`.
- [x] Crear `HomeCustomizationChoiceProgressMapper` (interno a `Kiwbi.Application.Choices`).
- [x] Crear `GetHousingPromotionChoicesProgressUseCase` + `HousingPromotionChoicesProgressDto`/`HousingUnitChoicesProgressDto`.
- [x] Crear `GetHousingUnitChoicesDetailUseCase` + `TradeCategoryChoicesDto`/`CustomizationChoiceDto`.
- [x] Crear `HousingPromotionChoicesController` con `Index`/`Details` y sus vistas.
- [x] Añadir enlace desde `HousingPromotions/Details.cshtml`.
- [x] Tests de Application: promoción sin Viviendas/Personalizaciones da contadores en cero; Vivienda con Personalizaciones mixtas (algunas `Pending`, otras `Selected`) cuenta correctamente; Gremio expirado sin fila cuenta como `Pending` (no se auto-confirma); ownership de otra Promotora rechazado.

### Feature 6.2 - Gestión de estados manuales

- [x] Añadir `Confirm(DateTime utcNow)` y `MarkAsPaid(DateTime utcNow)` a `HomeCustomizationChoice` (Domain), con tests de Domain para cada invariante (no confirmar sin opción, no confirmar dos veces, no pagar sin confirmar antes).
- [x] Crear `ConfirmHomeCustomizationChoiceUseCase` (con materialización de la opción por defecto si no existe fila) y `MarkHomeCustomizationChoiceAsPaidUseCase`.
- [x] Añadir acciones `Confirm`/`MarkAsPaid` a `HousingPromotionChoicesController`, con los botones condicionales en `Details.cshtml`.
- [x] Tests de Application: confirmar antes del cut-off rechazado; confirmar tras el cut-off sin elección previa crea la fila con la opción default y la deja `Confirmed`; confirmar una elección ya `Selected` la pasa a `Confirmed` conservando la opción elegida por el comprador; marcar como pagada sin estar `Confirmed` rechazado; ownership de otra Promotora rechazado en ambos casos.

### Feature 6.3 - Exportación de reportes

- [x] Añadir paquetes `ClosedXML` y `QuestPDF` a `Kiwbi.Infrastructure`; fijar `QuestPDF.Settings.License = LicenseType.Community`.
- [x] Crear el puerto `IHousingPromotionReportGenerator` (Application.Common) y `HousingPromotionReportDto` (con su desglose por Gremio → Vivienda → Personalización).
- [x] Crear `ExportHousingPromotionReportUseCase` (reutiliza la misma resolución de aplicabilidad/estado que 6.1, con el detalle completo en vez de solo contadores).
- [x] Implementar `HousingPromotionReportGenerator` en `Kiwbi.Infrastructure/Reporting` (ClosedXML para Excel, QuestPDF para PDF).
- [x] Registrar el nuevo servicio en `AddInfrastructureServices`.
- [x] Añadir acciones `ExportExcel`/`ExportPdf` a `HousingPromotionChoicesController` + enlaces desde `Index.cshtml`.
- [x] Tests de Application: el DTO de reporte agrupa correctamente por Gremio y por Vivienda; Personalizaciones no aplicables a una Vivienda quedan excluidas del reporte de esa Vivienda; ownership de otra Promotora rechazado. (La generación de bytes de ClosedXML/QuestPDF en sí no se testea unitariamente — se verifica manualmente que el archivo se abre correctamente, mismo criterio que otros Epics con librerías de terceros).

### Cierre del Epic

- [x] Ejecutar los tests unitarios y una compilación completa de la solución. (305 tests: 131 Domain + 174 Application, todos en verde).
- [x] Verificar manualmente: exportar Excel y PDF de una Promoción con datos variados (algunas Viviendas sin elecciones, otras con elecciones `Selected`/`Confirmed`/`Paid`) y confirmar que ambos archivos abren correctamente y muestran los datos esperados. **Completado en el Epic 10** (2026-09-20) contra el tenant de demo real "Kiwbi Demo" (promoción "Residencial Vistalar") — ver `epic-10-demo-data-and-report-verification.md`, Features 10.2/10.3: ambos ficheros descargados y verificados por código (cabeceras válidas, agrupación por Gremio/Vivienda y conteo de filas correctos), sin defectos.
- [x] Revisar que `HousingPromotionChoicesController` solo dependa de contratos de Application y no acceda a `KiwbiDbContext` ni a `ClosedXML`/`QuestPDF` directamente. Confirmado: solo referencia use cases de `Kiwbi.Application.Choices`/`Kiwbi.Application.RealEstate` y los ViewModels de `Kiwbi.Web.Models`.
- [x] Actualizar este documento con los checks completados y cualquier decisión técnica aprobada durante la implementación.
- [x] Actualizar la memoria de repositorio (`kiwbi-structure.md`) con un resumen del Epic cerrado, mismo criterio que Epics anteriores.

## Consideraciones de Testing y Notas de la IA

### Testing

- Priorizar tests de Domain para `HomeCustomizationChoice.Confirm`/`MarkAsPaid` (invariantes de transición: sin opción fijada, ya confirmada, no confirmada antes de pagar).
- Priorizar tests de Application para: cálculo de contadores/estado agregados (incluyendo el caso "Gremio expirado sin fila" → `Pending`, no auto-confirmado), rechazo de `Confirm` antes del cut-off, materialización correcta de la opción por defecto al confirmar sin elección previa, rechazo de `Paid` sin `Confirmed` previo, aislamiento de Promotora (ownership) en los tres casos de uso nuevos de gestión, y estructura del DTO de reporte (agrupación por Gremio/Vivienda, exclusión de Personalizaciones no aplicables).
- Usar sustitutos (NSubstitute) para todos los repositorios involucrados (`IHomeCustomizationChoiceRepository`, `IHousingUnitRepository`, `IHousingPromotionRepository`, `ITradeCategoryRepository`, `ICustomizationRepository`), `ICurrentUser`, `IUnitOfWork` y el nuevo puerto `IHousingPromotionReportGenerator` (mockeado en los tests de `ExportHousingPromotionReportUseCase`, sin generar bytes reales).
- No usar el proveedor In-Memory de EF Core; mantener el criterio de Epics anteriores de no sustituir PostgreSQL con In-Memory.

### Notas de la IA

- Código y nombres técnicos en inglés; vistas y mensajes en español, igual que en Epics anteriores.
- No se crea un módulo Application nuevo ("Management"): todo vive en `Kiwbi.Application.Choices`, mismo criterio de organizar por entidad de dominio (`HomeCustomizationChoice`) en lugar de por actor (comprador vs. promotora), igual que Epic 5 ya hizo con `Choices` en vez de anidar bajo `Onboarding`.
- `Confirm`/`MarkAsPaid` son transiciones unidireccionales por decisión explícita del usuario; si en el futuro se necesita corregir un error administrativo, se deberá diseñar un caso de uso de corrección explícito y auditable, no reabrir estas transiciones a un modelo bidireccional.
- La materialización de la fila `HomeCustomizationChoice` al confirmar (cuando el comprador nunca eligió nada) reutiliza el método `SelectOption` ya existente de Epic 5 en vez de crear un método de dominio nuevo "confirmar con default" — mantiene una única fuente de verdad de qué significa "seleccionar una opción", aunque el llamador sea la promotora en vez del comprador.
- El puerto `IHousingPromotionReportGenerator` vive en `Kiwbi.Application.Common` (mismo sitio que `IFileStorageService`/`IEmailSender`) para mantener la regla de dependencia Application → Domain solamente; ClosedXML/QuestPDF son detalles de Infrastructure, nunca referenciados desde Application ni Web.
- Antes de programar este Epic, el usuario debe aprobar este documento.
