# Epic 10: Datos de Demo y Verificación de Exportación de Reportes (Cierre de la Feature 6.3)

## Estado

- Estado: **Feature 10.1 (seeder de datos de demo) IMPLEMENTADA y verificada (2026-09-20). Features 10.2/10.3/10.4 pendientes.**
- Feature 10.1 resultado: `DemoDataSeeder` (`src/Kiwbi.Web/DemoSeeding/`), invocado vía `dotnet run --project src/Kiwbi.Web -- --seed-demo`, siembra el tenant "Kiwbi Demo" con las 3 Promociones descritas más abajo, reutilizando los métodos de fábrica/transición de Domain tal cual (sin cambios en Domain/Application). Idempotencia verificada ejecutando el seeder dos veces seguidas contra el Postgres local y comprobando por SQL que no quedan duplicados (exactamente 1 `DeveloperCompany` "Kiwbi Demo", 3 Promociones, conteos de Viviendas/Gremios/Personalizaciones/usuarios exactamente los esperados). La limpieza idempotente marca todo el grafo de entidades para borrado y lo confirma en una única `SaveChangesAsync` (EF Core ordena las sentencias DELETE automáticamente por dependencia de FK), y borra los `ApplicationUser` (comprador/promotora) vía `UserManager` en una fase posterior separada, una vez ya no quedan filas que los referencien (`home_buyer_assignments`/`developer_company_id` tienen FK `Restrict`). Un caso (invitación caducada) se "retrasa" en el tiempo mediante reflexión sobre los setters privados de `BuyerInvitation` (Domain no expone una fábrica para esto y no se quería añadir superficie solo para el seeder) — documentado con un comentario en el código. Credenciales y guion de demo en `demo-data-guide.md`.
- Depende de: Epic 1 (Foundation & Promotora Tenant), Epic 2 (Real Estate Core), Epic 3 (Customization Engine), Epic 4 (Onboarding B2B2C), Epic 5 (Buyer Experience) y Epic 6 (Management & Exporting — en particular la Feature 6.3, cuyo cierre motiva este Epic). No introduce reglas de negocio de Domain nuevas; es exclusivamente datos de demo + cobertura de tests + verificación manual sobre funcionalidad ya construida.
- Alcance ampliado respecto a la redacción original del roadmap (`05-development-roadmap.md`): el Epic 10 nació como una tarea de QA manual pura ("sin desarrollo de código previsto"). El usuario ha ampliado el alcance a tres bloques: (1) un seeder de datos de demo reutilizable y con volumen/coherencia pensados para una demo real del producto (no solo datos mínimos de prueba), documentado en un fichero aparte con credenciales; (2) cobertura de tests automáticos reales sobre la generación de Excel/PDF (hoy inexistente — ver "Contexto"); (3) la verificación manual original, que pasa a ser el último paso, no el único.

## Contexto

La Feature 6.3 (Epic 6) implementó y testeó unitariamente `ExportHousingPromotionReportUseCase`, pero **mockeando siempre** `IHousingPromotionReportGenerator` — la generación real de bytes con ClosedXML/QuestPDF nunca se ha ejecutado ni verificado, ni por un test automático ni manualmente contra datos reales. Además, hoy el repositorio no tiene ningún mecanismo de seeding de base de datos (confirmado: no existe `Seed`/`DbInitializer`/`HasData` en ningún sitio), por lo que no hay forma rápida de poblar una base de datos con datos representativos ni para verificar manualmente el export ni para una demo del producto.

## Objetivos

- **Feature 10.1:** construir un seeder de datos de demo, ejecutable por línea de comandos e idempotente, que genere una Promotora con **3 Promociones en distintos estados de avance** (no una única promoción grande), cubriendo los cuatro estados de `HomeCustomizationChoiceStatus`, las tres situaciones de Gremio (vencido y totalmente resuelto, vencido con pendientes, y aún abierto) y varios estados de `BuyerInvitation` (incluida una invitación caducada). Junto al seeder, un documento aparte (`demo-data-guide.md`) explica cada caso pensado para la demo y recoge las credenciales de la Promotora y de los Compradores relevantes.
- **Feature 10.2:** cobertura de tests automáticos + verificación manual de la exportación a Excel.
- **Feature 10.3:** cobertura de tests automáticos + verificación manual de la exportación a PDF.
- **Feature 10.4:** corrección de cualquier defecto detectado en 10.2/10.3, hasta confirmar que ambos ficheros son correctos con datos reales, y cierre del checklist pendiente de Epic 6.

## Decisiones confirmadas por el usuario

1. **Mecanismo de seeding:** un flag de línea de comandos sobre `Kiwbi.Web` (p. ej. `dotnet run --project src/Kiwbi.Web -- --seed-demo`), NO un proyecto de consola aparte ni un script SQL directo — necesario para poder crear cuentas de Identity con contraseña real reutilizando `IAccountProvisioningService`/`IBuyerAccountProvisioningService` ya existentes.
2. **Idempotencia:** el seeder debe poder re-ejecutarse antes de la demo sin dejar duplicados — localiza el tenant demo por un email/nombre fijo conocido, borra todo su grafo de datos si ya existe, y lo vuelve a crear desde cero.
3. **Escala y composición:** 3 Promociones bajo el mismo tenant demo, cada una en una etapa distinta del ciclo de vida (ver "Diseño de datos de demo"), en vez de una única promoción grande y muy variada. La variedad de estados se concentra en las dos primeras; la tercera se deja deliberadamente mínima ("en definición").
4. **Documento de demo:** fichero nuevo `.ai-docs/demo-data-guide.md`, separado de este documento — contiene las credenciales y el guion de la demo, no el análisis técnico.
5. **Alcance del "end-to-end" de exportación:** un test a nivel de Controller (invocando la action directamente, sin servidor HTTP real ni `WebApplicationFactory`) en un nuevo proyecto `tests/Kiwbi.Web.Tests` — el primer proyecto de test de la capa Web. El verdadero "pulsar el botón y que se descargue el fichero" se sigue verificando manualmente en navegador (Features 10.2/10.3, paso de cierre).
6. **Profundidad de los tests sobre el PDF real:** sin nueva dependencia de parseo de PDF — solo se comprueba que los bytes generados no estén vacíos y que empiecen por la cabecera mágica `%PDF`. El generador de Excel sí recibe aserciones reales de celda/fila (ClosedXML puede leer su propio formato sin dependencias nuevas).
7. **Áreas adicionales de test:** generador Excel real, generador PDF real (smoke test), y casos límite adicionales del cálculo de "opción efectiva"/agrupación del reporte que hoy no están cubiertos.

## Diseño de datos de demo (Feature 10.1)

Un único tenant demo (`DeveloperCompany` + un login `DeveloperAdmin`) con **3 `HousingPromotion`**:

1. **"Residencial Vistalar" — ACABADA / entregada.** 3 Tipologías, 8 Viviendas (todas `Sold`). 2 Gremios, ambos vencidos (`SelectionCutOffDateUtc` en el pasado). Personalizaciones con ámbitos mixtos (Promoción/Tipología/Vivienda), 2-3 por Gremio. Solo 2 Viviendas tienen cuenta de Comprador real y elecciones completas (para no disparar el número de credenciales a documentar, pero cubriendo igualmente los cuatro estados):
   - Comprador 1: todas sus elecciones en `Paid` — caso "todo cerrado".
   - Comprador 2: la mayoría `Paid` pero una elección deliberadamente en `Pending` sobre un Gremio ya vencido — permite pulsar "Confirmar" en vivo durante la demo.
   - Las 6 Viviendas restantes: `Sold` pero sin cuenta de Comprador vinculada todavía (situación realista: vendida, pendiente de invitar/onboardear al comprador).
2. **"Residencial Puerta Azul" — CASI TERMINADA.** 2 Tipologías, 6 Viviendas (4 `Sold`, 2 `Reserved`). 3 Gremios: 2 vencidos, 1 todavía abierto (cutoff futuro). Personalizaciones con ámbitos mixtos.
   - Comprador 3 (una Vivienda `Sold`): Gremios vencidos con elecciones `Selected`/`Confirmed`, Gremio abierto con `Selected` — caso "en curso, casi todo decidido".
   - Comprador 4 (otra Vivienda `Sold`): un Gremio vencido todavía `Pending` (pendiente de confirmar) y el Gremio abierto también `Pending` (el comprador aún no ha decidido) — el caso más variado.
   - Una Vivienda `Reserved`: `BuyerInvitation` enviada hace tiempo, nunca aceptada, ya **Caducada**.
   - Otra Vivienda `Reserved`: `BuyerInvitation` enviada recientemente, todavía **Pendiente** (vigente, sin aceptar) — muestra el onboarding en marcha.
   - Resto de Viviendas: sin invitación ni comprador.
3. **"Residencial Nuevos Pinos" — EN DEFINICIÓN.** 2 Tipologías, 5 Viviendas, todas `Available`. 1 Gremio con cutoff muy lejano, 1 Personalización (`WholePromotion`) recién creada con su opción por defecto + 1 opción adicional. Cero invitaciones, cero Compradores, cero elecciones — fase puramente de catálogo/configuración, deliberadamente mínima.

Total de credenciales a documentar en `demo-data-guide.md`: 1 Promotora + 4 Compradores (Comprador 1-4). Emails/contraseñas fijas y fácilmente reconocibles (p. ej. `demo@kiwbi.test` / `buyer1@kiwbi.test`…`buyer4@kiwbi.test`), con las cadenas exactas decididas durante la implementación.

## Análisis Técnico

### Seeder de datos de demo

El seeder (`DemoDataSeeder`, nuevo, en `Kiwbi.Web`) orquesta la creación usando:

- Los métodos factoría de Domain (`Customization.CreateForWholePromotion/CreateForTypologies/CreateForUnits`, `TradeCategory.Create`, `HousingUnit.Create`, etc.) a través de los repositorios ya existentes + `IUnitOfWork`, exactamente igual que un Application use case.
- `IAccountProvisioningService`/`IBuyerAccountProvisioningService` (puertos de Application, implementados en Infrastructure/Identity) para crear el login de la Promotora y los 4 Compradores — estos puertos no dependen de `ICurrentUser`/contexto HTTP, así que son seguros de invocar desde un proceso de línea de comandos.
- Los métodos reales `HomeCustomizationChoice.Create`/`SelectOption`/`Confirm`/`MarkAsPaid` para materializar las elecciones en cada estado, con las fechas de corte de los Gremios sembradas en el pasado/futuro según convenga — así los datos de demo respetan exactamente las mismas invariantes que produciría un uso real de la aplicación (p. ej. no se puede `Confirm` antes del cutoff), en vez de insertar filas "a mano" saltándose las reglas de Domain.

**Idempotencia:** antes de crear nada, el seeder busca el `DeveloperCompany` por el email fijo de la Promotora demo. Si existe, borra todo el subgrafo en orden seguro para las FKs: `HomeCustomizationChoice` → `Customization` (que arrastra en cascada `CustomizationOption`/`CustomizationAssignment`, al ser `OwnsMany`) → `TradeCategory` → `HousingUnitBuyer` → `BuyerInvitation` → `HousingUnit` → `HousingTypology` → `HousingPromotion` → los `ApplicationUser` de Comprador vinculados exclusivamente a este tenant → el `ApplicationUser` de la Promotora → `DeveloperCompany`. Se verificará durante la implementación si los repositorios existentes ya exponen los métodos de borrado necesarios para cada paso; si falta alguno se añade en Infrastructure siguiendo el patrón ya usado por los casos de uso `Delete*`.

**Punto de entrada:** en `Program.cs` de `Kiwbi.Web`, tras construir la `WebApplication` (contenedor de DI ya disponible) pero antes de `app.Run()`, se comprueba si `args` contiene `--seed-demo`; si es así, se crea un scope, se resuelve/ejecuta `DemoDataSeeder`, se registra un resumen por log, y el proceso termina sin arrancar Kestrel.

### Cobertura de tests automáticos (Features 10.2/10.3)

Hoy `ExportHousingPromotionReportUseCaseTests` (4 tests) mockea por completo `IHousingPromotionReportGenerator` — ningún test ejercita ClosedXML/QuestPDF reales, y no existe ningún proyecto de test para `Kiwbi.Web`. Se añade:

1. **`HousingPromotionReportGeneratorTests`** (nuevo, en `tests/Kiwbi.Application.Tests/Choices/ExportHousingPromotionReport/`, que ya referencia `Kiwbi.Infrastructure` desde el Epic 8b): instancia el `HousingPromotionReportGenerator` real con un `HousingPromotionReportDto` construido a mano (varios Gremios/Viviendas/Personalizaciones, incluyendo una con opción efectiva nula y otra con sobrecoste). Para Excel, abre los bytes devueltos con `ClosedXML.Excel.XLWorkbook` y comprueba cabeceras de columna, número de filas y valores de celda concretos (incluido el "-" cuando no hay opción efectiva). Para PDF, solo comprueba que los bytes no estén vacíos y empiecen por la cabecera `%PDF` (decisión confirmada, sin librería nueva de parseo).
2. **Casos límite adicionales** en `ExportHousingPromotionReportUseCaseTests.cs` (o un fichero hermano): una Vivienda sin ninguna Personalización aplicable en ningún Gremio (debe quedar excluida del todo), un Gremio sin ninguna Personalización aplicable a ninguna Vivienda de la Promoción (debe quedar excluido entero), propagación correcta de nombre/sobrecoste cuando la opción efectiva NO es la opción por defecto, y una Promoción sin Viviendas/Gremios (reporte vacío, sin excepción).
3. **Nuevo proyecto `tests/Kiwbi.Web.Tests`** (primer proyecto de test de la capa Web; mismo set de paquetes que `Kiwbi.Application.Tests` — xunit/NSubstitute/FluentAssertions — con referencia de proyecto a `Kiwbi.Web`, añadido a `Kiwbi.slnx`). `HousingPromotionChoicesExportTests` construye la instancia real de `ExportHousingPromotionReportUseCase` con los repositorios/`ICurrentUser` mockeados vía NSubstitute + un `IHousingPromotionReportGenerator` sustituido (misma técnica que el test de Application ya existente), construye el `HousingPromotionChoicesController` real (los otros 4 casos de uso que exige su constructor se instancian de forma trivial, con repos mockeados que nunca se llegan a invocar en estos tests) e invoca `ExportExcel`/`ExportPdf` directamente, comprobando: `NotFound()` cuando falla la comprobación de propiedad (`DeveloperCompanyId` no coincide), y un `FileContentResult` con el `ContentType`/`FileDownloadName`/bytes esperados en el caso de éxito, para ambos formatos. No se extrae ninguna interfaz nueva para los casos de uso (son clases concretas hoy) — se reutiliza el mismo patrón de mocking a nivel de repositorio que ya usan los tests de Application, evitando tocar el contrato del Controller solo para hacerlo testeable.

## Impacto en Arquitectura

### Kiwbi.Web

- Nueva carpeta `DemoSeeding/` con `DemoDataSeeder` (+ una clase de constantes con los emails/nombre fijos del tenant demo, usados tanto para sembrar como para detectar/limpiar ejecuciones previas).
- `Program.cs`: nueva rama para el flag `--seed-demo` (ver Análisis Técnico).
- Nuevo proyecto de test `tests/Kiwbi.Web.Tests` (añadido a `Kiwbi.slnx`).

### Kiwbi.Application / Kiwbi.Application.Tests

- Sin cambios de producción (no se toca ningún use case ni DTO existente).
- Tests nuevos: `HousingPromotionReportGeneratorTests` + casos límite añadidos a `ExportHousingPromotionReportUseCaseTests`.

### Kiwbi.Infrastructure

- Sin cambios previstos salvo que, durante la implementación del paso de limpieza idempotente del seeder, se detecte que algún repositorio no expone todavía un método de borrado necesario — en ese caso se añade siguiendo el patrón ya existente (mismo criterio que las extensiones incrementales de repos en Epics anteriores).

### Kiwbi.Domain

- Sin cambios. El seeder reutiliza los métodos de fábrica/transición ya existentes tal cual.

## Plan de Acción (Step-by-Step)

### Feature 10.1 — Datos de demo (✅ COMPLETA, 2026-09-20)

- [x] Crear `DemoDataSeeder` (+ constantes de emails/nombre del tenant demo) en `Kiwbi.Web/DemoSeeding/`.
- [x] Implementar la limpieza idempotente (borrado del tenant demo previo, en orden seguro para FKs) antes de recrear los datos.
- [x] Sembrar las 3 Promociones con su composición completa (Tipologías, Viviendas, Gremios, Personalizaciones/Opciones/Asignaciones, invitaciones, cuentas de Comprador, elecciones) tal como se detalla en "Diseño de datos de demo".
- [x] Añadir la rama `--seed-demo` en `Program.cs`.
- [x] Ejecutar `dotnet run --project src/Kiwbi.Web -- --seed-demo` contra el Postgres local (con las migraciones ya aplicadas) y repetir una segunda vez para confirmar la idempotencia (verificado por SQL: exactamente 1 `DeveloperCompany` "Kiwbi Demo", 3 Promociones, conteos de Viviendas/Gremios/Personalizaciones/usuarios correctos tras la 2ª ejecución).
- [x] Verificar manualmente en la app en ejecución (login como la Promotora demo) que las 3 Promociones y sus datos son coherentes antes de redactar la guía (verificado en navegador: listado de 3 promociones, detalle de Residencial Vistalar con las 8 viviendas/tipologías correctas, y el caso "Confirmar en vivo" de 1ºB/Armario empotrado renderiza el botón "Confirmar" tal como estaba previsto).
- [x] Redactar `.ai-docs/demo-data-guide.md` (credenciales + narrativa de cada Promoción + guion de la demo).
- [x] Añadir una referencia cruzada a `demo-data-guide.md` desde este documento y desde la entrada del Epic 10 en `05-development-roadmap.md`.

### Features 10.2/10.3 — Cobertura de tests + verificación manual de exportación

- [ ] Crear `HousingPromotionReportGeneratorTests` (Excel con aserciones reales de celda vía ClosedXML; PDF con verificación de bytes/cabecera `%PDF`).
- [ ] Añadir los casos límite adicionales a los tests de `ExportHousingPromotionReportUseCase`.
- [ ] Crear el proyecto `tests/Kiwbi.Web.Tests` (añadido a `Kiwbi.slnx`) con `HousingPromotionChoicesExportTests` (Controller-level, sin servidor HTTP real).
- [ ] Ejecutar la batería completa de tests y confirmar que todo está en verde.
- [ ] Verificación manual: usando la Promoción con más datos (o las que corresponda), exportar Excel y PDF desde `HousingPromotionChoices/Index`, abrir ambos ficheros y comprobar agrupación por Gremio/Vivienda, opciones efectivas/sobrecostes/estados correctos, exclusión de Personalizaciones no aplicables, y que la maquetación del PDF (tablas, paginación) es legible y no corta contenido.

### Feature 10.4 — Cierre

- [ ] Corregir cualquier defecto detectado en el paso de verificación manual anterior (en `HousingPromotionReportGenerator`, `ExportHousingPromotionReportUseCase`, o donde corresponda) y repetir la verificación hasta confirmar que ambos ficheros son correctos.
- [ ] Actualizar el checklist de cierre de Epic 6 (`epic-06-management-exporting.md`) marcando como resuelto el punto pendiente de verificación manual.
- [ ] Actualizar la entrada del Epic 10 en `05-development-roadmap.md` con el resultado.
- [ ] Actualizar la memoria de repositorio (`kiwbi-structure.md`) con un resumen del Epic cerrado, mismo criterio que Epics anteriores.

## Consideraciones de Testing y Notas de la IA

### Testing

- El seeder de demo NO sustituye a los tests automáticos — sirve exclusivamente para poblar datos representativos para la verificación manual y para la propia demo del producto; no se testea unitariamente a sí mismo (es una herramienta de desarrollo/demo, no lógica de negocio de dominio).
- Se sigue el mismo criterio de Epics anteriores de NO usar el proveedor In-Memory de EF Core; los tests que necesiten un mapeo EF Core real (incluidos los nuevos, si acaso lo necesitan) usan SQLite en memoria como `CustomizationOptionEfCoreMappingTests` (Epic 8b).
- `HousingPromotionReportGeneratorTests` instancia el generador real (sin mocks) — es la única forma de detectar problemas reales de ClosedXML/QuestPDF (nombres de hoja, formato de columnas, cabecera PDF válida) que los tests mockeados de Epic 6 no pueden detectar por construcción.
- El nuevo proyecto `tests/Kiwbi.Web.Tests` se limita, en este Epic, a las acciones `ExportExcel`/`ExportPdf`; no se amplía a otras acciones del Controller (`Confirm`/`MarkAsPaid`/`Index`/`Details`) ni a otros Controllers — eso queda fuera de alcance de este Epic, pendiente de un futuro esfuerzo de testing de la capa Web si se decide abordarlo.

### Notas de la IA

- El Epic 10 no modifica ninguna regla de negocio existente; toda la superficie de cambio de producción se limita a `Kiwbi.Web` (seeder + rama de arranque) y a proyectos de test nuevos/existentes. Si la limpieza idempotente del seeder revela que falta algún método de borrado en un repositorio de Infrastructure, se añade siguiendo el patrón ya establecido en Epics anteriores (extensión incremental, no un rediseño).
- Los emails del tenant demo deben ser claramente distintos de cualquier cuenta de prueba ya creada manualmente en la base de datos de desarrollo (p. ej. `fase0demo@kiwbi.test`, usada durante el Epic 8) para que el paso de limpieza idempotente del seeder no toque datos ajenos por error.
- Antes de programar este Epic, el usuario debe aprobar este documento.
