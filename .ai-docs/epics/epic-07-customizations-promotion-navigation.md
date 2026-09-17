# Epic 07: Reorganización de Navegación — Personalizaciones a nivel de Promoción

## Estado

- Estado: IMPLEMENTADO (2026-09-18). Build y suite completa de tests de Domain/Application en verde (131 + 174) tras el cambio; sin regresiones, ya que el cambio es exclusivamente de `Kiwbi.Web`. Smoke test manual (servidor local): `Customizations/Index`, `Customizations/Create` y `TradeCategories/Index` devuelven 302 a `/Account/Login` sin sesión, confirmando que la acción unificada no introduce ambigüedad de rutas.
- Depende de: Epic 3 (Customization Engine), ya implementado y validado. Es una mejora transversal de navegación sin dependencias de base de datos con los Epics 4, 5 y 6; se documenta y programa después de éstos para no interrumpir el avance del núcleo funcional del MVP, pero no depende de que estén implementados.

## Contexto y objetivo

Durante la implementación de la Epic 3 se detectó que el punto de entrada para crear y listar Personalizaciones obliga a navegar primero a un Gremio concreto (`/TradeCategories/{tradeCategoryId}` → "Personalizaciones"). Tras analizarlo, se confirmó que esto es una decisión de **arquitectura de información (IA) del Front**, no una restricción del dominio: `Customization.TradeCategoryId` sigue siendo una FK obligatoria en el modelo (una Personalización siempre pertenece a un Gremio, regla de negocio de `03-domain-model.md`), pero **desde qué pantalla se inicia el flujo de alta es independiente de esa regla**.

El objetivo de este Epic es que la promotora pueda listar y dar de alta Personalizaciones **directamente desde la Promoción** (igual que ya hace con Tipologías, Viviendas y Gremios), sin tener que entrar primero a un Gremio concreto. El Gremio pasa a seleccionarse en el propio formulario de alta (un `<select>`) en vez de venir implícito en la URL de origen.

**Confirmado explícitamente con el usuario:** este cambio se resuelve por completo en `Kiwbi.Web` (controllers, ViewModels y vistas). No requiere ningún cambio en `Kiwbi.Domain`, `Kiwbi.Application` ni `Kiwbi.Infrastructure`: los casos de uso `GetTradeCategoriesUseCase`, `GetCustomizationsUseCase` y `CreateCustomizationUseCase` ya existen y ya aceptan los parámetros necesarios.

## Análisis Técnico

### Qué NO cambia

- El modelo de dominio (`Customization.TradeCategoryId` sigue siendo obligatorio).
- Los contratos de Application (`CreateCustomizationCommand`, `GetCustomizationsUseCase`, etc.).
- El esquema de base de datos y las migraciones.
- Las pantallas `Details`, `Edit`, `Delete` y la gestión de Opciones/Asignaciones de una Personalización (ya son autónomas por `Id`, no dependen de por dónde se llegó a ellas).
- `TradeCategoriesController` y sus vistas: el listado de Gremios se mantiene tal cual (sigue siendo necesario para gestionar el nombre y la fecha límite de cada Gremio).

### Qué cambia (solo `Kiwbi.Web`)

1. **Listado agregado por Promoción:** `CustomizationsController.Index` pasa a tener la firma `Index(Guid promotionId, Guid? tradeCategoryId = null)` (ver "Decisiones Confirmadas" — no puede coexistir con un segundo método `Index(Guid promotionId)` separado del actual `Index(Guid tradeCategoryId)` porque ambos tendrían la misma firma en C#, un solo parámetro `Guid`, y no compilaría). El Controller:
   - Llama a `GetTradeCategoriesUseCase.ExecuteAsync(promotionId)` para obtener los Gremios de la promoción (y resuelve el tenant/ownership, igual que hoy).
   - Si `tradeCategoryId` es `null`, llama a `GetCustomizationsUseCase.ExecuteAsync(tradeCategoryId)` por cada Gremio devuelto y agrega los resultados (N+1 aceptado, ver más abajo). Si `tradeCategoryId` tiene valor, valida que pertenezca a esa promoción y llama a `GetCustomizationsUseCase` solo para ese Gremio (comportamiento equivalente al `Index(Guid tradeCategoryId)` actual, pero como caso particular de la misma acción).
   - **Decisión aceptada:** en el caso agregado, esto son N+1 llamadas a un caso de uso ya existente (no una única consulta optimizada). Es un trade-off consciente: el volumen esperado (unos pocos Gremios por promoción, panel de administración de bajo tráfico) lo hace perfectamente asumible para el MVP, evitando tocar Application/Infrastructure. Si en el futuro esta pantalla se vuelve un cuello de botella real, la mejora natural sería añadir `ICustomizationRepository.GetByHousingPromotionIdAsync` (fuera de alcance de este Epic).
2. **Alta desde la Promoción:** por el mismo motivo de colisión de firmas, `Create(Guid tradeCategoryId)` pasa a ser `Create(Guid promotionId, Guid? tradeCategoryId = null)`. El formulario incluye un `<select>` de Gremio poblado con `GetTradeCategoriesUseCase(promotionId)`, preseleccionado si se recibió `tradeCategoryId` (entrada desde el filtro por Gremio), igual patrón que el `<select multiple>` de Tipologías/Viviendas ya usado en la Feature 3.2. `CreateCustomizationCommand` no cambia: el Controller simplemente obtiene el `TradeCategoryId` del `<select>` en vez de la URL. Si la promoción no tiene ningún Gremio dado de alta, esta entrada se deshabilita (ver "Decisiones Confirmadas").
3. **ViewModel del listado:** `CustomizationListItemViewModel` se amplía con un `TradeCategoryName` opcional/nullable; la vista existente `Customizations/Index.cshtml` muestra la columna "Gremio" solo cuando se accede sin filtro (listado agregado por Promoción), y la oculta cuando se accede filtrado por un Gremio concreto (donde ya es redundante, el Gremio está en el título de la página).
4. **Navegación:** `HousingPromotionsController` → vista `Details` añade un enlace directo "Personalizaciones" (mismo patrón que "Tipologías"/"Viviendas"/"Gremios"), deshabilitado/con aviso si la promoción no tiene Gremios.
5. **Redirects tras Create/Edit/Delete:** siempre vuelven al listado agregado por Promoción (`Index(promotionId)`, sin `tradeCategoryId`), independientemente de si se entró filtrado por un Gremio concreto. Ver justificación en "Decisiones Confirmadas".
6. **¿Se mantiene el acceso por Gremio?** Se mantiene el enlace "Personalizaciones" en `TradeCategories/Index`, que ahora simplemente invoca la misma acción `Index` pasando `tradeCategoryId` como filtro (ya no es una acción ni una ruta distinta), preservando el comportamiento actual de ver solo las Personalizaciones de ese Gremio.

## Decisiones Confirmadas (2026-09-18)

1. **Colisión de firmas Index/Create:** ambas acciones se unifican con un parámetro de filtro opcional — `Index(Guid promotionId, Guid? tradeCategoryId = null)` y `Create(Guid promotionId, Guid? tradeCategoryId = null)` — en lugar de intentar mantener dos acciones separadas (que no compilarían por tener la misma firma) o duplicar la lógica bajo nombres de acción distintos. Ventaja adicional: resuelve el punto 6 (acceso alternativo por Gremio) reutilizando la misma acción en vez de un controlador/ruta paralela.
2. **Redirect tras Create/Edit/Delete:** siempre al listado agregado por Promoción (`Index(promotionId)`), sin recordar si se entró filtrado por un Gremio. Se prioriza un único criterio de retorno simple de mantener frente a la fidelidad de volver exactamente al filtro de origen (que exigiría propagar `tradeCategoryId` como campo oculto adicional en los formularios).
3. **Promoción sin Gremios todavía:** la entrada "Nueva personalización" (y el enlace "Personalizaciones" desde `HousingPromotions/Details` si se considera oportuno) se deshabilita o muestra un aviso guiando a crear primero un Gremio, en vez de dejar que el usuario llegue a un `<select>` vacío y falle solo al enviar el formulario.
4. **Vista/ViewModel del listado agregado:** se reutiliza `Customizations/Index.cshtml` y `CustomizationListItemViewModel` (ampliado con `TradeCategoryName` opcional) en vez de crear una vista/ViewModel dedicados, siguiendo el mismo patrón de vistas con variación condicional ya usado en otras pantallas del proyecto (p.ej. `HousingPromotions/Details`).

## Plan de Acción (Step-by-Step)

- [x] Unificar `Index(Guid tradeCategoryId)` en `Index(Guid promotionId, Guid? tradeCategoryId = null)`: resolver Gremios vía `GetTradeCategoriesUseCase(promotionId)` y, según haya o no `tradeCategoryId`, listar Personalizaciones agregadas (N+1 sobre `GetCustomizationsUseCase`) o filtradas a un único Gremio (validando que pertenezca a la promoción).
- [x] Ampliar `CustomizationListItemViewModel` con `TradeCategoryName` opcional; adaptar `Customizations/Index.cshtml` para mostrar la columna "Gremio" solo en el caso agregado (sin `tradeCategoryId`).
- [x] Unificar `Create(Guid tradeCategoryId)` en `Create(Guid promotionId, Guid? tradeCategoryId = null)` GET/POST: `<select>` de Gremio poblado desde `GetTradeCategoriesUseCase(promotionId)`, preseleccionado si llega `tradeCategoryId`; `CreateCustomizationCommand` sin cambios.
- [x] Deshabilitar/avisar en la entrada "Nueva personalización" (y opcionalmente en el enlace "Personalizaciones" de la Promoción) cuando `GetTradeCategoriesUseCase(promotionId)` devuelva una lista vacía.
- [x] Actualizar `Delete`, y los enlaces "volver"/"cancelar" de `Details.cshtml`/`Create.cshtml` (hoy apuntan a `Index(tradeCategoryId)`), para redirigir siempre a `Index(promotionId)` sin `tradeCategoryId`.
- [x] Añadir el enlace "Personalizaciones" en `HousingPromotions/Details.cshtml`, apuntando a `Index(promotionId)`.
- [x] Actualizar el enlace "Personalizaciones" en `TradeCategories/Index.cshtml` para que pase también `promotionId` (ya disponible en `ViewBag.PromotionId`) además de `tradeCategoryId`, dado que la acción unificada exige `promotionId` como parámetro obligatorio.
- [x] Validar manualmente el flujo completo (alta desde Promoción, alta desde Gremio, listado agregado, listado filtrado, edición, borrado, caso sin Gremios) y el redirect 302 a login sin sesión.

## Notas de Cierre (2026-09-18)

- `CustomizationsController.Index`/`Create` ahora exigen `promotionId` como parámetro obligatorio; `tradeCategoryId` es opcional y actúa como filtro/preselección. La entrada por Gremio (`TradeCategories/Index` → "Personalizaciones") sigue funcionando, ahora pasando ambos IDs a la misma acción.
- El guard de "Promoción sin Gremios" se implementó en dos capas: `Create` redirige a `Index` con un `TempData["Error"]` si no hay ningún Gremio (defensa ante navegación directa por URL); `Index`/`Create.cshtml` ocultan el botón "Nueva personalización" y muestran un aviso con enlace a "Crear un gremio" cuando `HasTradeCategories` es `false`.
- `Details.cshtml` ("Volver al listado") y `Create.cshtml` ("Cancelar") redirigen siempre al listado agregado por Promoción, igual que Delete, siguiendo el criterio único de retorno decidido.
- No hubo cambios en Domain/Application; 131 tests de Domain y 174 de Application siguen en verde.

## Consideraciones de Testing y Notas de la IA

- No se esperan nuevos tests de Domain ni de Application (no hay cambios en esas capas). Si se detecta alguna regresión en los use cases existentes durante la validación manual, tratarla como bug y no como parte de este Epic.
- El proyecto no tiene actualmente tests de integración MVC (ver Notas de Epic 1: no se sustituye PostgreSQL por In-Memory); la validación de este Epic es manual (smoke tests), igual que en Epics anteriores.
- Antes de programar este Epic, el usuario debe aprobar este documento.
