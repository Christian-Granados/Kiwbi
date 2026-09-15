# Epic 07: Reorganización de Navegación — Personalizaciones a nivel de Promoción

## Estado

- Estado: Diseño en revisión — pendiente de aprobación expresa del usuario antes de implementar código (regla de `04-ai-coding-guidelines.md`, sección 5).
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

1. **Listado agregado por Promoción:** nueva acción `Index(Guid promotionId)` en `CustomizationsController` (o una ruta adicional sobre la existente) que:
   - Llama a `GetTradeCategoriesUseCase.ExecuteAsync(promotionId)` para obtener los Gremios de la promoción.
   - Llama a `GetCustomizationsUseCase.ExecuteAsync(tradeCategoryId)` por cada Gremio devuelto y agrega los resultados en el Controller.
   - **Decisión aceptada:** esto son N+1 llamadas a un caso de uso ya existente (no una única consulta optimizada). Es un trade-off consciente: el volumen esperado (unos pocos Gremios por promoción, panel de administración de bajo tráfico) lo hace perfectamente asumible para el MVP, evitando tocar Application/Infrastructure. Si en el futuro esta pantalla se vuelve un cuello de botella real, la mejora natural sería añadir `ICustomizationRepository.GetByHousingPromotionIdAsync` (fuera de alcance de este Epic).
2. **Alta desde la Promoción:** `Create(Guid promotionId)` sustituye a `Create(Guid tradeCategoryId)` como entrada principal; el formulario incluye un `<select>` de Gremio (poblado con `GetTradeCategoriesUseCase(promotionId)`), igual patrón que el `<select multiple>` de Tipologías/Viviendas ya usado en la Feature 3.2. `CreateCustomizationCommand` no cambia: el Controller simplemente obtiene el `TradeCategoryId` del `<select>` en vez de la URL.
3. **ViewModel del listado:** `CustomizationListItemViewModel` se amplía con `TradeCategoryName` (o se crea un ViewModel específico para esta vista agregada) para mostrar a qué Gremio pertenece cada fila, ya que ahora conviven Personalizaciones de varios Gremios en una misma tabla.
4. **Navegación:** `HousingPromotionsController` → vista `Details` añade un enlace directo "Personalizaciones" (mismo patrón que "Tipologías"/"Viviendas"/"Gremios").
5. **Redirects tras Create/Edit/Delete:** deben apuntar de vuelta al listado a nivel de Promoción. Si se decide mantener también el acceso por Gremio (ver punto siguiente), hay que fijar un criterio único de "vuelta atrás" (recomendado: siempre a nivel de Promoción, para no duplicar lógica de retorno).
6. **¿Se mantiene el acceso por Gremio?** Se mantiene el enlace "Personalizaciones" en `TradeCategories/Index` como acceso alternativo/filtrado (útil cuando la promotora ya está gestionando un Gremio concreto y quiere ver solo sus Personalizaciones), pero deja de ser el único punto de entrada.

## Plan de Acción (Step-by-Step)

- [ ] Añadir `Index(Guid promotionId)` a `CustomizationsController`, agregando resultados de `GetCustomizationsUseCase` por cada Gremio de la promoción (vía `GetTradeCategoriesUseCase`).
- [ ] Ampliar `CustomizationListItemViewModel` (o crear uno específico) con el nombre del Gremio.
- [ ] Adaptar la vista `Customizations/Index.cshtml` para incluir la columna "Gremio" cuando se accede a nivel de Promoción.
- [ ] Adaptar `Create(Guid promotionId)` GET/POST: `<select>` de Gremio poblado desde `GetTradeCategoriesUseCase`, manteniendo `CreateCustomizationCommand` sin cambios.
- [ ] Añadir el enlace "Personalizaciones" en `HousingPromotions/Details.cshtml`.
- [ ] Fijar y documentar el criterio de redirect tras Create/Edit/Delete (recomendado: siempre al listado de Promoción).
- [ ] Confirmar que el acceso por Gremio (`TradeCategories/Index` → "Personalizaciones") sigue funcionando como filtro alternativo.
- [ ] Validar manualmente el flujo completo (alta, listado agregado, edición, borrado) y el redirect 302 a login sin sesión.

## Consideraciones de Testing y Notas de la IA

- No se esperan nuevos tests de Domain ni de Application (no hay cambios en esas capas). Si se detecta alguna regresión en los use cases existentes durante la validación manual, tratarla como bug y no como parte de este Epic.
- El proyecto no tiene actualmente tests de integración MVC (ver Notas de Epic 1: no se sustituye PostgreSQL por In-Memory); la validación de este Epic es manual (smoke tests), igual que en Epics anteriores.
- Antes de programar este Epic, el usuario debe aprobar este documento.
