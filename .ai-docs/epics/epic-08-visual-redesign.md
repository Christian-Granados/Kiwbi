# Epic 08: Rediseño Visual Global (Design System)

## Estado

- Estado: Diseño en revisión — pendiente de aprobación expresa del usuario antes de implementar código (regla de `04-ai-coding-guidelines.md`, sección 5).
- Depende de: ninguno de los Epics funcionales (1-6) a nivel de datos. Es una mejora transversal que afecta a todas las vistas ya construidas; se documenta y programa después del núcleo del MVP para no interrumpir su avance, pero puede abordarse en paralelo o en cualquier momento posterior.

## Contexto y objetivo

Las vistas actuales de Kiwbi (Epics 1-3) usan Bootstrap 5 "de fábrica" (el tema por defecto del scaffolding de ASP.NET Core MVC), sin una identidad visual propia. Además, `DeveloperCompany.Branding` (`PrimaryColor`, `SecondaryColor`, `LogoPath` — Epic 1) se captura y se muestra como datos en `DeveloperProfile`, pero **no se aplica todavía a ningún elemento visual real de la aplicación** (no hay CSS dinámico ni tema por tenant).

El objetivo de este Epic es definir y aplicar un sistema de diseño (Design System) propio de Kiwbi que:
1. Dé una identidad visual coherente y profesional a toda la aplicación.
2. Contemple desde el principio cómo se integra el *branding* por tenant ya existente (cada promotora podría ver su propio color primario/secundario/logo reflejado en su panel).
3. Sea compatible con el stack ya decidido en `02-architecture-and-stack.md` (ASP.NET Core MVC + Razor, HTMX, Alpine.js; "NO se usan frameworks SPA pesados").

## Alcance

**Dentro de alcance:**
- Exploración visual asistida por herramientas de IA (visión global + subconjunto priorizado de pantallas) y, a partir de sus resultados, decisión del enfoque de implementación real en `Kiwbi.Web` (Feature 8.1).
- Definición de tokens de diseño (colores, tipografía, espaciados) y de cómo el branding por tenant los sobreescribe (Feature 8.2).
- Componentes base reutilizables: layout/navegación, botones, formularios, tablas, badges, cards, alerts (Feature 8.2).
- Aplicación del nuevo sistema a las vistas ya existentes de los Epics 1-6 (Feature 8.3).

**Fuera de alcance (explícitamente):**
- Cualquier cambio de Domain/Application/Infrastructure: este Epic es 100% `Kiwbi.Web` (vistas, layout, assets estáticos, y como mucho un endpoint de CSS dinámico si el enfoque elegido lo requiere para aplicar el branding por tenant).
- Rediseño de la lógica de negocio o de los flujos funcionales; solo se retocan la presentación y los estilos, no el comportamiento.
- Las vistas de Epics futuros aún no construidos (Epic 4-6 si no estuvieran implementados en el momento de ejecutar este Epic) se construirán directamente con el nuevo sistema una vez esté listo, sin necesidad de "migrarlas" después.

## Feature 8.1 - Exploración visual asistida por IA

**Alcance revisado (2026-09-18):** en vez de arrancar directamente evaluando frameworks/librerías CSS de forma abstracta, esta Feature usa herramientas de generación de diseño asistidas por IA para explorar visualmente cómo podría verse Kiwbi, **primero a nivel de visión global de la aplicación y después pantalla a pantalla sobre un subconjunto priorizado**, antes de fijar tokens/componentes definitivos en la Feature 8.2. Es un proceso deliberadamente iterativo y de varias rondas (no un único entregable): se espera repetir el ciclo "generar variaciones → revisar → afinar" tantas veces como haga falta, tanto para la visión global como para cada pantalla, y probablemente a lo largo de varias sesiones de trabajo.

**Reparto de trabajo:**
- **Usuario:** introduce los *briefs*/prompts en la(s) herramienta(s) de IA elegida(s), genera variaciones y comparte los resultados (capturas, enlaces, HTML exportado si lo hay).
- **IA (esta sesión y las siguientes):** redacta los *briefs* de cada ronda, revisa y critica las propuestas recibidas señalando qué encaja o no con Kiwbi, y documenta en este Epic la dirección visual que se vaya consolidando. Cuando la herramienta elegida genere código directamente reutilizable (p. ej. HTML/CSS en Bootstrap), lo adapta a Razor.

### Herramientas recomendadas

| Herramienta | Tipo de resultado | Cuándo usarla |
| --- | --- | --- |
| **Galileo AI / Uizard** | Mockups de pantalla completos a partir de un prompt de texto, variaciones rápidas. | Fase de visión global y primeras rondas por pantalla: explorar dirección estética sin preocuparse por el stack técnico (el resultado siempre es una imagen/mockup "a traducir" después, sea cual sea el framework real). |
| **v0.dev (Vercel)** | Código real (React + Tailwind) con vista previa interactiva. | Inspiración de composición/interacción/microinteracciones; su código no es directamente reutilizable en Razor/Bootstrap (hay que reinterpretarlo, no copiarlo/pegarlo). |
| **Prompt directo a un LLM pidiendo HTML/CSS en Bootstrap 5** | HTML/CSS ya en el framework real del proyecto. | Última ronda, una vez la dirección visual esté acordada: para obtener algo mucho más cercano a "casi reutilizable" como base de la Feature 8.2, en vez de tener que reinterpretar mockups de imagen o código React/Tailwind. |

No es necesario elegir una sola herramienta: es razonable combinar Galileo AI/Uizard para divergir rápido en la fase de visión global, y reservar el prompt directo en Bootstrap 5 para la fase de convergencia final antes de pasar a la Feature 8.2.

### Brief de visión global (ronda 1)

Texto de partida para introducir en la herramienta de IA elegida (ajustar libremente entre rondas):

```
Diseña la identidad visual de "Kiwbi", una aplicación web B2B2C para promotoras
inmobiliarias que gestionan la personalización de viviendas en construcción.

Contexto de negocio:
- Usuario principal (B2B): una promotora inmobiliaria que gestiona sus promociones,
  viviendas, gremios (electricidad, fontanería, carpintería...) y las opciones de
  personalización que ofrece a sus compradores (ej. tipo de suelo, grifería).
  Panel de administración denso en datos: listados, formularios CRUD, tablas de
  progreso/estado.
- Usuario secundario (B2C): un comprador de vivienda que recibe una invitación por
  email, entra a un portal sencillo, ve su vivienda y elige entre las opciones de
  personalización disponibles antes de una fecha límite por gremio.
- Tono deseado: profesional, confiable, moderno, sector inmobiliario/construcción.
  Ni "corporativo aburrido" ni "startup juguetona"; debe transmitir seriedad y
  control (se manejan decisiones de compra de vivienda) sin resultar frío.
- Cada promotora (tenant) tiene su propio color primario/secundario y logo, que
  deben poder aplicarse como acento sobre el sistema de diseño base (no sustituirlo
  por completo) - piensa en un layout neutro que "acepte" un color de marca externo
  en botones/acentos clave, sin comprometer la legibilidad.

Restricciones técnicas (para que las variaciones sean realistas, no obligatorias
en herramientas puramente de mockup):
- El framework CSS real de la aplicación es Bootstrap 5 (sin build de Tailwind).
- No hay SPA: cada pantalla es una página Razor renderizada en servidor, con algo
  de interactividad vía HTMX/Alpine.js en puntos concretos (no toda la UI).

Genera 3-4 variaciones de la visión global: paleta de color (con un color neutro
base + cómo se vería un acento de marca de tenant superpuesto), tipografía,
estilo de layout/navegación (barra superior vs. sidebar), y tono visual general
(cards, tablas, formularios). No hace falta que sean pantallas completas todavía,
puede ser un dashboard genérico + una pantalla de listado/tabla como referencia.
```

### Ronda 1 (visión global) — resultado (2026-09-18)

El usuario generó 4 variaciones en v0.dev (proyecto Next.js + Tailwind + shadcn descargado, revisado directamente desde su código fuente en `C:\Users\cgran\Documents\identidad-visual-kiwbi`, sin necesidad de moverlo al repo): **01 Operativo · Sidebar denso**, 02 Editorial, 03 Blueprint · Técnico, 04 Cálido.

**Decisión:** gana **Operativo · Sidebar denso**, combinado con el patrón de sidebar colapsable a *icon-rail* de **Blueprint · Técnico** (en vez del sidebar de Operativo fijo siempre expandido). Elementos consolidados:
- **Paleta base:** neutro frío tipo *Slate* (fondo `#f8fafc`, bordes `#e2e8f0`, texto `#0f172a`/`#64748b`). El acento de marca del tenant se aplica *solo* sobre: logo/mark, item de nav activo, botón primario, punto de notificación y relleno de barras de progreso — nunca sobre el fondo general ni el texto, para no comprometer la legibilidad con colores de tenant arbitrarios.
- **Tipografía:** Inter (Google Fonts), tamaños compactos (`0.875rem` base, `~0.7rem` para metadatos), `font-weight: 600` para énfasis en vez de negrita extrema.
- **Radios:** dos niveles, `0.5rem` (botones/inputs/badges) y `0.75rem` (cards/tablas).
- **Navegación: sidebar con dos estados**, expandido (~224px, logo + nombre de tenant + labels de texto + widget "Plan Promotora" anclado abajo) y colapsado a icon-rail (~64px, solo iconos con `title` como tooltip, sin el widget inferior). Con botón de contraer/expandir en la topbar y persistencia de la preferencia (pensado para `localStorage` en el PoC; en la implementación real de la Feature 8.2 podría persistir por usuario si se considera necesario).

**Pendiente de esta ronda** (no bloqueante para seguir, pero a tener en cuenta): los 4 estados usados en el mockup (Completada/En progreso/Sin iniciar/Vencida) son ilustrativos, no corresponden 1:1 a los enums reales del dominio (`HousingUnitStatus`, `HomeCustomizationChoiceStatus`) — el mapeo final se hará en la Feature 8.2/8.3.

### PoC técnico (2026-09-18)

Para validar que la dirección elegida es realmente construible con el stack real del proyecto (Bootstrap 5 + Razor, sin build de Tailwind) antes de seguir generando más mockups, se creó `wwwroot/design-preview/viviendas.html`: una página HTML estática y autocontenida (no forma parte de ninguna vista Razor ni ruta de la aplicación, no toca `_Layout.cshtml` ni ningún Controller) que reutiliza el Bootstrap 5 ya vendorizado del proyecto (`~/lib/bootstrap`) más CSS/JS propios, y reproduce fielmente en HTML/CSS real la pantalla "Viviendas" de Operativo con el sidebar híbrido de Blueprint.

**Validado manualmente en navegador (capturas + interacción):**
- El sidebar colapsa/expande correctamente (botón en la topbar), con transición de ancho.
- El interruptor de color de marca por tenant (4 swatches de muestra) recolorea en vivo el logo, el item de nav activo y las barras de progreso sin recargar la página, confirmando que el mecanismo "acento de marca sobre base neutra" es perfectamente viable con CSS custom properties (`--kiwbi-accent`/`--kiwbi-accent-2`) + Bootstrap 5, sin necesitar Tailwind ni ningún framework nuevo.
- Layout, tipografía, densidad y componentes (KPI cards, tabla con badges de estado y barras de progreso) coinciden visualmente con el mockup de Operativo.

Esto resuelve de facto la "Selección del enfoque de implementación" (más abajo) a favor de **Bootstrap 5 a medida** (variables CSS/custom properties + clases propias tipo `kiwbi-*`), sin necesidad de adoptar una plantilla open-source de terceros ni migrar a Tailwind: el resultado ya es indistinguible del mockup usando el stack actual.

### Plan de acción de la Feature 8.1

- [x] Adaptar el alcance de la Feature de "evaluación de frameworks" a "exploración visual con IA" (este documento, 2026-09-18).
- [x] Ronda 1 (visión global): usuario genera variaciones con el brief anterior (v0.dev) y comparte los resultados.
- [x] IA revisa las variaciones de la ronda 1 y consolida una dirección: Operativo · Sidebar denso + sidebar colapsable de Blueprint · Técnico.
- [x] Validar la viabilidad técnica de la dirección elegida en el stack real (`wwwroot/design-preview/viviendas.html`, Bootstrap 5 puro) antes de seguir iterando mockups.
- [x] Con la validación técnica ya hecha, decidir el enfoque de implementación real en `Kiwbi.Web`: **Bootstrap 5 a medida** (ver "Selección del enfoque de implementación" más abajo).
- [x] Decidir el mecanismo definitivo de aplicación del branding por tenant al tema para la implementación real (ver "Mecanismo de inyección del branding por tenant" más abajo).
- [x] Implementar el mecanismo de branding decidido (ver "Implementación del mecanismo de branding" más abajo) y validarlo end-to-end con datos reales.
- [x] Redactar los briefs del resto del subconjunto priorizado de pantallas (ver "Ronda 2 (pantallas priorizadas) — briefs" más abajo): Brief A (Dashboard Promotora), Brief B (CRUD de Personalizaciones), Brief C (Portal del Comprador).
- [x] Usuario genera los mockups de Brief A/B/C en v0.dev (mismo proyecto/hilo, un solo mockup coherente por pantalla) y comparte la carpeta descargada.
- [x] IA revisa Brief A/B/C, traduce las 5 pantallas a Bootstrap 5 real bajo `wwwroot/design-preview/`, y documenta la dirección final por pantalla (ver "Ronda 2 (pantallas priorizadas) — resultado" más abajo).
- [x] Revisión conjunta usuario + IA de las 5 pantallas ya traducidas: se detecta un problema de arquitectura de navegación (ver "Ronda 3" más abajo) — no se cierra la Feature 8.1 todavía, sigue habiendo pantallas pendientes de diseñar.
- [x] Decidir y documentar el modelo de navegación en dos niveles (global vs. contextual de Promoción) y eliminar "Compradores" del nav.
- [x] Brief de sincronización: usuario lo pasa a v0.dev sobre el mismo proyecto/hilo para retrofitar las 5 pantallas ya generadas al nuevo modelo de dos niveles, antes de pedir pantallas nuevas.
- [x] IA revisa el resultado de la sincronización y actualiza los 5 ficheros de `wwwroot/design-preview/` ya existentes.
- [x] Usuario genera **Brief D — Resumen/Hub de Promoción** en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief D, traduce a Bootstrap 5 real, y documenta la dirección final de esa pantalla.
- [x] Usuario genera **Brief E — Gremios (listado + alta/edición)** en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief E, traduce a Bootstrap 5 real, y documenta la dirección final de esa pantalla.
- [x] Optimizar los briefs restantes agrupando pantallas relacionadas (ver "Ronda 4 (resto del inventario) — briefs" más abajo): Brief F (Tipologías), Brief G (Vivienda + Invitaciones de comprador), Brief H (Detalle de Personalización), Brief I (Progreso), Brief J (Mi promotora + Crear/Editar Promoción) — 5 briefs en vez de 7, cada uno con al menos una pantalla relacionada, salvo Brief H que se mantiene individual por su complejidad.
- [x] Usuario genera Brief F en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief F, traduce a Bootstrap 5 real, y documenta la dirección final.
- [x] Usuario genera Brief G en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief G, traduce a Bootstrap 5 real, y documenta la dirección final.
- [x] Usuario genera Brief H en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief H, traduce a Bootstrap 5 real, y documenta la dirección final.
- [x] Usuario genera Brief I en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief I, traduce a Bootstrap 5 real, y documenta la dirección final.
- [x] Usuario genera Brief J en v0.dev y comparte la carpeta descargada.
- [x] IA revisa Brief J, traduce a Bootstrap 5 real, y documenta la dirección final.
- [x] **Inventario completo de pantallas cerrado**: las 13 pantallas del inventario de la Ronda 3 (más Personalizaciones y Viviendas, ya cubiertas antes) están diseñadas, traducidas a Bootstrap 5 y validadas en navegador.
- [ ] **Regla explícita mientras dure esta Feature: no se pasa a la Feature 8.2 hasta que todo el inventario de pantallas quede diseñado, traducido y validado** (o se decida explícitamente dar por cerrado el alcance del rediseño con lo que haya en ese momento).
- [ ] Cerrar la Feature 8.1 dejando el PoC + mockups + decisiones documentadas como entrada de la Feature 8.2 (que convertirá esto en tokens/componentes reales integrados en las vistas Razor, con datos reales en vez de contenido de muestra).

### Ronda 2 (pantallas priorizadas) — resultado (2026-09-19)

El usuario generó los tres briefs en el **mismo proyecto/hilo de v0.dev** que la ronda 1 (buena práctica ya recomendada: mantiene el mismo lenguaje visual sin tener que reexplicarlo), descargado esta vez en `C:\Users\cgran\Documents\identidad-visual-kiwbi-dashboard-comprador`. El proyecto contenía los tres componentes esperados — `dashboard-promotora.tsx` (Brief A), `personalizaciones.tsx` (Brief B, con listado + formulario ya como dos vistas conmutables) y `buyer-experience.tsx` (Brief C, con las dos pantallas del comprador) — muy fieles a lo pedido, incluyendo detalles no explícitamente solicitados pero coherentes con la dirección ya acordada:
- Sidebar con estado de colapso real (`useState`, no solo CSS) en Dashboard Promotora y Personalizaciones.
- Un nuevo item de navegación **"Ajustes"** anclado bajo el widget "Plan Promotora" en el pie del sidebar (no estaba en el PoC de Viviendas de la ronda 1 — se ha añadido también allí para que las 5 pantallas compartan exactamente el mismo sidebar).
- Avatar del usuario con iniciales + nombre corto (p. ej. "CG" / "Carmen G.") en vez de un círculo vacío. Como `Kiwbi.Web` no guarda hoy un nombre de persona por usuario (solo el nombre de la promotora vía `DeveloperCompany`), esto queda anotado como una decisión pendiente para la Feature 8.2/8.3: usar el nombre de la promotora, las iniciales del email, o añadir un campo de nombre a `ApplicationUser` si se considera necesario.
- En el Dashboard del Comprador, cada tarjeta de vivienda incluye un pequeño punto de color + nombre de la promotora al pie (no es el acento global de la página, solo una etiqueta informativa por tarjeta) — coherente con "sin acento de marca" a nivel de página, pero es una idea a valorar para la Feature 8.2/8.3 si se quiere trasladar (exigiría exponer el color de marca por Vivienda en `BuyerHousingUnitViewModel`, hoy no lo tiene).

**Las 5 pantallas ya están traducidas a Bootstrap 5 real y verificadas en navegador** (capturas + interacciones):

| Pantalla | Fichero | Verificado |
| --- | --- | --- |
| Viviendas (ronda 1, refactorizada) | `wwwroot/design-preview/viviendas.html` | Sidebar colapsa/expande, breadcrumb, avatar con iniciales |
| Dashboard Promotora (Brief A) | `wwwroot/design-preview/dashboard-promotora.html` | Tabla de Promociones con columnas reales, KPIs, estado vacío comentado en el HTML como referencia |
| Personalizaciones · listado (Brief B) | `wwwroot/design-preview/personalizaciones.html` | Columna Gremio, chips "Se aplica a", enlaza a la vista de alta |
| Personalizaciones · alta (Brief B) | `wwwroot/design-preview/personalizaciones-nueva.html` | Selector "Se aplica a" cambia de opción activa y muestra/oculta el multi-select de tipologías/viviendas; pills de selección múltiple con toggle |
| Mis viviendas — Comprador (Brief C, pantalla 1) | `wwwroot/design-preview/buyer-index.html` | Sin sidebar ni acento, tarjetas con plano/placeholder |
| Detalle de Vivienda — Comprador (Brief C, pantalla 2) | `wwwroot/design-preview/buyer-housing-unit.html` | Cabecera con degradado del acento de la promotora dueña; tarjetas de opción seleccionables (grupo abierto) vs. deshabilitadas (grupo vencido); selector de "vivienda de ejemplo" (solo de esta PoC) confirma que el acento cambia por vivienda, no por sesión de usuario |

**Decisión de arquitectura tomada al traducir Brief B a Razor real:** en el mockup de v0.dev, listado y formulario son dos vistas que se conmutan en el cliente (estado de React). Como Kiwbi es explícitamente no-SPA (páginas Razor renderizadas en servidor, `02-architecture-and-stack.md`), se tradujeron como **dos páginas reales enlazadas** (`personalizaciones.html` → `personalizaciones-nueva.html`, con un enlace `<a>`, no un `<button>` con estado JS) — más fiel a como ya funciona `CustomizationsController.Index`/`Create` (Epic 7) que replicar el patrón de un solo componente con estado interno.

**Refactor técnico realizado sobre el PoC (no visible en los mockups, decisión propia al traducir):** con 5 pantallas ya comparten el mismo shell (sidebar/topbar/KPIs/tabla/badges/formularios), se extrajo el CSS y JS que antes vivían inline en `viviendas.html` a dos ficheros compartidos — `wwwroot/design-preview/kiwbi-preview.css` y `kiwbi-preview.js` — para evitar que cada nueva pantalla duplicara y pudiera desincronizar esas reglas. `viviendas.html` se adaptó para referenciarlos (mismo resultado visual, verificado de nuevo tras el cambio). El JS compartido añade además los manejadores genéricos para el selector "Se aplica a" (`.kiwbi-scope-option` + `[data-scope-panel]`), las pills de selección múltiple (`.kiwbi-pill`) y las tarjetas de opción seleccionables (`[data-option-group]`), reutilizables en cualquier pantalla futura sin JS adicional.

### Ronda 3 (arquitectura de navegación e inventario de pantallas) — 2026-09-19

Al revisar conjuntamente las 5 pantallas de la ronda 2, el usuario detectó una inconsistencia de fondo en el nav del sidebar (heredado tal cual de los mockups de v0 desde la ronda 1): mezclaba elementos de **dos niveles distintos** bajo una única lista plana. Se verificó contra los 11 Controllers reales de `Kiwbi.Web`, confirmando la sospecha:

- **Nivel global (promotora, sin una Promoción concreta):** `HousingPromotionsController.Index` (listado de Promociones — es literalmente lo que el nav llamaba "Panel") y `DeveloperProfileController` (perfil + marca — "Mi promotora").
- **Nivel Promoción (todo Controller exige `promotionId`):** `HousingPromotionsController.Details/Edit`, `HousingTypologiesController`, `HousingUnitsController`, `TradeCategoriesController`, `CustomizationsController`, `HousingPromotionChoicesController`.

El propio mockup ya delataba la mezcla: en `dashboard-promotora.html` el nav marcaba "Panel" como activo mientras el *breadcrumb* decía "Panel / Promociones" y el contenido era el listado de Promociones — dos cosas tratadas como una.

**Además, "Compradores" no corresponde a ninguna pantalla real:** no existe ningún listado agregado de compradores (ni global ni por Promoción) en ningún Controller, ni aparece en `.ai-docs/01-business-context.md` ni en el resto de documentos de negocio. Lo único que existe es `HousingUnitsController.Invitations/InviteBuyer`, gestión de invitaciones **por Vivienda concreta**. Confirmado: fue un añadido genérico de v0, no un requisito de Kiwbi.

**Decisiones tomadas (2026-09-19):**
1. **Navegación en dos niveles.** Sidebar global (fuera del contexto de una Promoción): Panel (= listado de Promociones), Mi promotora, Ajustes. Sidebar contextual (al entrar en una Promoción concreta, desde `Details` o cualquier subpantalla): Resumen, Tipologías, Viviendas, Gremios, Personalizaciones, Progreso de personalizaciones — con un enlace de vuelta a Promociones. No es una idea nueva: es lo que ya hace hoy `HousingPromotions/Details.cshtml` con su fila de botones, llevado al sidebar.
2. **"Compradores" desaparece del nav.** Se sigue gestionando exclusivamente desde la Vivienda concreta (pantalla "Invitaciones de comprador", pendiente de diseñar, ver inventario).
3. **Regla de proceso:** no se avanza a la Feature 8.2 (aplicar el sistema a las vistas Razor reales) hasta cerrar el diseño de todo el inventario de pantallas de abajo, o decidir explícitamente recortar el alcance.

**Inventario completo de pantallas** (Global / Promoción / Comprador / Anónimo), cruzado con lo ya cubierto:

| Nivel | Pantalla | Estado |
| --- | --- | --- |
| Global | Promociones (listado) | ✅ cubierta (`dashboard-promotora.html`) |
| Global | Mi promotora (perfil + marca) | ✅ cubierta (`mi-promotora.html`) — **Brief J** |
| Promoción | **Resumen/Hub de la Promoción** (`Details`, tabla de viviendas) | ✅ cubierta (`promocion-resumen.html`) — **Brief D** |
| Promoción | Crear/Editar Promoción | ✅ cubierta (`promocion-nueva.html`) — **Brief J** |
| Promoción | Tipologías (listado + alta/edición) | ✅ cubierta (`tipologias.html`/`tipologias-nueva.html`) — **Brief F** |
| Promoción | Viviendas (listado) | ✅ cubierta (`viviendas.html`) |
| Promoción | Crear/Editar Vivienda | ✅ cubierta (`vivienda-editar.html`) — **Brief G** |
| Promoción | Invitaciones de comprador (por Vivienda) | ✅ cubierta (`vivienda-invitaciones.html`) — **Brief G** |
| Promoción | **Gremios** (listado + alta/edición) | ✅ cubierta (`gremios.html`/`gremios-nuevo.html`) — **Brief E** |
| Promoción | Personalizaciones (listado + alta) | ✅ cubiertas (`personalizaciones.html`/`-nueva.html`) |
| Promoción | Detalle de Personalización (opciones + asignaciones) | ✅ cubierta (`personalizacion-detalle.html`) — **Brief H** |
| Promoción | Progreso de personalizaciones (agregado + detalle por vivienda) | ✅ cubierta (`progreso.html`/`progreso-detalle.html`) — **Brief I** — no aparecía ni en el nav del mockup pese a estar implementada desde Epic 6 |
| Comprador | Mis viviendas / Detalle de vivienda | ✅ cubiertas (sin cambios, no llevan sidebar) |
| Anónimo | Login/Register, aceptación de invitación | Fuera de prioridad por ahora |

**Priorizado para continuar ahora mismo:** primero un brief de "sincronización" (retrofit del nav en las 5 pantallas ya generadas, para no arrastrar inconsistencias a las pantallas nuevas), después Brief D (Resumen/Hub de Promoción) y Brief E (Gremios). El resto del inventario se aborda en próximas sesiones según se priorice.

#### Sincronización — resultado (2026-09-19)

El usuario pasó el brief de sincronización en el **mismo proyecto/hilo original de v0.dev** (`C:\Users\cgran\Documents\identidad-visual-kiwbi`, el de la ronda 1 — no el de la ronda 2). El resultado fue muy fiel y, de hecho, mejor estructurado de lo pedido: en vez de duplicar el sidebar en cada componente, v0 extrajo un componente compartido `KiwbiSidebar` (`components/kiwbi/sidebar.tsx`) con una prop `mode: 'global' | 'context'`, más `GLOBAL_NAV`/`CONTEXT_NAV` (arrays de nav separados en `data.ts`, sin "Compradores" en ninguno de los dos) y un `contextName`/`onBack` para el indicador "Estás dentro de". Verificado leyendo el código fuente de `dashboard-promotora.tsx` (`mode="global"`), `personalizaciones.tsx` y `variation-operativo.tsx`/Viviendas (ambos `mode="context"` con `contextName="Residencial Miravalle"`) — los tres coinciden exactamente con lo pedido.

**Retrofit aplicado a los 4 ficheros reales ya existentes** (`dashboard-promotora.html`, `viviendas.html`, `personalizaciones.html`, `personalizaciones-nueva.html`) bajo `wwwroot/design-preview/`:
- `dashboard-promotora.html` pasa a la sidebar **global**: solo Panel (activo) / Mi promotora / Ajustes; breadcrumb simplificado a "Panel" (ya no "Panel / Promociones", redundante).
- Los otros tres pasan a la sidebar **contextual**: nuevo bloque `.kiwbi-context-indicator` ("Estás dentro de: Residencial Miravalle", con flecha y enlace de vuelta a `dashboard-promotora.html`, colapsable a solo la flecha en modo icon-rail) + nav Resumen/Tipologías/Viviendas/Gremios/Personalizaciones/Progreso (dos iconos nuevos añadidos a `kiwbi-preview.css`/inline SVG: capas para Tipologías, gráfico de barras para Progreso). "Resumen" y "Gremios" enlazan a `#` por ahora (pendientes de Brief D y Brief E); breadcrumbs actualizados a "Residencial Miravalle / Viviendas" y "Residencial Miravalle / Personalizaciones".
- Nueva regla CSS `.kiwbi-context-indicator` añadida a `kiwbi-preview.css` (compartida, ningún cambio en `kiwbi-preview.js`: el colapso de sidebar ya funcionaba igual).

**Validado en navegador** (capturas + interacción): sidebar global de `dashboard-promotora.html` muestra únicamente 3 items; sidebar contextual de `viviendas.html`/`personalizaciones.html`/`personalizaciones-nueva.html` muestra el indicador "Estás dentro de" (expandido y colapsado) y los 6 items correctos, sin "Compradores" en ningún caso. Build de `Kiwbi.Web` en verde tras el retrofit (no se tocó ningún `.cs`, solo HTML/CSS de `wwwroot/design-preview/`).

#### Brief de sincronización (retrofit del nav en las 5 pantallas existentes)

```
Dirección ya acordada (no la cambies): sistema "Operativo + sidebar híbrido" ya
generado (paleta Slate, Inter, radios 0.5rem/0.75rem, acento de marca solo en
logo/nav activo/botón primario/notificación/progreso).

CAMBIO DE ARQUITECTURA DE NAVEGACIÓN (aplícalo a los 5 componentes que ya
generamos: dashboard-promotora, personalizaciones [listado y formulario] y el
layout de viviendas): el sidebar deja de ser una única lista plana y pasa a
tener DOS NIVELES:

1) Sidebar GLOBAL (cuando NO se está dentro de ninguna Promoción concreta —
   úsalo en dashboard-promotora, que es el listado de Promociones): solo
   3 items — "Panel" (activo, es el propio listado de Promociones), "Mi
   promotora" y "Ajustes". Nada de Viviendas/Gremios/Personalización aquí:
   no tienen sentido sin haber elegido antes una Promoción.

2) Sidebar CONTEXTUAL (cuando SÍ se está dentro de una Promoción concreta —
   úsalo en Viviendas y en Personalización [listado y formulario]): además
   del logo/marca de la promotora, muestra debajo un pequeño indicador de
   "estás dentro de: Residencial Miravalle" con un enlace/flecha para volver
   al listado de Promociones, y DESPUÉS el nav con: Resumen, Tipologías,
   Viviendas, Gremios, Personalizaciones, Progreso. Quita el item "Panel" de
   este sidebar (ya no aplica, estás dentro de una Promoción).

Elimina por completo el item de nav "Compradores" de ambos sidebars: no
corresponde a ninguna pantalla real, se gestiona de otra forma (a nivel de
cada vivienda concreta, fuera de este nav).

Regenera los 5 componentes ya existentes con este nav corregido, sin cambiar
nada más de lo ya diseñado en cada uno (mismo contenido de cada pantalla).
```

#### Brief D — Resumen/Hub de Promoción (`HousingPromotions/Details`)

```
Dirección ya acordada (no la cambies): igual que en briefs anteriores, con el
sidebar CONTEXTUAL de Promoción ya corregido en el brief de sincronización
(Resumen activo en este caso, con el indicador "estás dentro de: X" arriba).

Pantalla a diseñar: el "hub" de una Promoción concreta — es la pantalla a la
que se llega al pulsar "Ver" desde el listado de Promociones, y desde la que
se accede a todo lo demás (Tipologías, Viviendas, Gremios, Personalizaciones,
Progreso), además de poder Editar o Eliminar la propia Promoción. Contenido:

- Cabecera con Nombre de la promoción, Ciudad, Dirección, y el plano general
  (imagen) si existe, o un placeholder neutro si no.
- Una fila de accesos directos a las secciones contextuales (Tipologías,
  Viviendas, Gremios, Personalizaciones, Progreso) — puede ser redundante con
  el propio sidebar contextual, está bien que lo sea, refuerza la navegación.
- Una tabla resumen de las Viviendas de esta promoción: Planta, Puerta,
  Tipología, Superficie construida, Superficie útil, Estado comercial (badge:
  Disponible/Reservada/Vendida).
- Botones de "Editar" y "Eliminar" la promoción.

Genera un único mockup coherente con la dirección ya acordada.
```

#### Brief D — resultado (2026-09-19)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/promotion-hub.tsx`. Muy fiel al brief y ya usa correctamente el `KiwbiSidebar` de la sincronización (`mode="context"`, `activeLabel="Resumen"`). Incluye, además de lo pedido: una fila de estadísticas (Viviendas totales + Disponibles/Reservadas/Vendidas con un punto de color cada una) entre la cabecera y los botones de acción, y un `checkbox` de demo "Sin plano general" para previsualizar el estado sin plano.

Traducido a `wwwroot/design-preview/promocion-resumen.html`, reutilizando el sidebar/topbar/breadcrumb ya establecidos:
- Cabecera con badge "Promoción", nombre, dirección+ciudad, fila de 4 estadísticas, botones Editar/Eliminar, y el plano (o placeholder) a la derecha.
- Rejilla de 5 tarjetas de acceso directo (Tipologías/Viviendas/Gremios/Personalizaciones/Progreso) con icono, hint y chevron; "Viviendas" y "Personalizaciones" ya enlazan a sus páginas reales, el resto a `#` hasta que existan.
- Tabla resumen de las 8 viviendas de ejemplo con badges de estado comercial: se añadió `.kiwbi-badge-amber` a `kiwbi-preview.css` para "Reservada" (ámbar), reutilizando `.kiwbi-badge-success`/`.kiwbi-badge-neutral` ya existentes para "Disponible"/"Vendida".
- El enlace "Resumen" del sidebar contextual en `viviendas.html`/`personalizaciones.html`/`personalizaciones-nueva.html` (antes apuntando a `#`) y el botón "Ver" de la tabla de Promociones en `dashboard-promotora.html` ahora apuntan a `promocion-resumen.html`, cerrando el ciclo de navegación entre las 6 pantallas ya traducidas.

Validado en navegador (captura): cabecera, estadísticas, accesos directos y tabla con badges por color coinciden con el mockup.

#### Brief E — Gremios (`TradeCategories/Index` + `TradeCategories/Create`/`Edit`)

```
Dirección ya acordada (no la cambies): igual que en briefs anteriores, sidebar
CONTEXTUAL de Promoción (Gremios activo).

Pantalla a diseñar: los "Gremios" de Kiwbi (piensa en ellos como partidas de
obra — electricidad, fontanería, carpintería... — cada uno con una fecha
límite a partir de la cual sus Personalizaciones asociadas quedan bloqueadas
para el comprador):

1) Listado: tabla con columnas Nombre y Fecha límite de selección (fecha +
   hora), y acciones Editar/Eliminar por fila. Si la fecha límite ya pasó,
   indícalo visualmente (p. ej. un badge "Vencido" junto a la fecha). Botón
   "Nuevo gremio" arriba.

2) Formulario de alta/edición: campo de texto para el Nombre, y un selector
   de fecha y hora para la Fecha límite de selección.

Genera un único mockup coherente con la dirección ya acordada para ambas
vistas (listado + formulario).
```

#### Brief E — resultado (2026-09-20)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/gremios.tsx` (`Gremios`, con listado y formulario como dos vistas conmutadas por estado de React, igual que hizo con Brief B). Muy fiel al brief: tabla con Nombre (icono+nombre), Fecha límite (fecha+hora con badge "Abierto"/"Vencido" calculado contra una fecha de referencia), columna opcional de Nº de personalizaciones, acciones Editar/Eliminar/más; formulario con Nombre + selectores nativos de fecha y hora separados.

Traducido, siguiendo la misma decisión de arquitectura ya tomada en Brief B (Kiwbi es no-SPA: dos páginas reales enlazadas, no un toggle de estado), a `wwwroot/design-preview/gremios.html` (listado) + `gremios-nuevo.html` (alta, reutilizada también como destino de "Editar" en esta PoC estática, mismo criterio que `personalizaciones-nueva.html`). Reutiliza `.kiwbi-table-card`/`.kiwbi-badge-success` ("Abierto") y `.kiwbi-badge-danger` ya existente ("Vencido", sin necesidad de una clase nueva) para el listado, y `.kiwbi-fieldset`/`.kiwbi-legend` del patrón ya usado en `personalizaciones-nueva.html` para el formulario. Datos de ejemplo (6 gremios con fechas variadas, 2 ya vencidas) tomados de `GREMIOS_ROWS` en `data.ts`.

Cerrado el bucle de navegación: el enlace "Gremios" del sidebar contextual (antes `#`) en `viviendas.html`/`personalizaciones.html`/`personalizaciones-nueva.html`/`promocion-resumen.html`, y la tarjeta de acceso directo "Gremios" de `promocion-resumen.html`, ahora apuntan a `gremios.html`. Validado en navegador (captura): listado y formulario coinciden visualmente con el mockup.

### Ronda 4 (resto del inventario) — briefs (2026-09-20)

Quedan 7 pantallas pendientes del inventario de la Ronda 3. En vez de seguir pidiendo una pantalla por ronda (como D y E), se agrupan aquí en **5 briefs** por afinidad real de dominio — mismo criterio ya usado en Brief C (dos pantallas relacionadas en un solo brief) — para reducir las idas y vueltas con v0.dev sin perder foco: cada brief cubre una única familia de pantallas coherente entre sí, salvo Brief H que se mantiene individual por ser la más compleja (gestión de opciones + reasignación).

| Brief | Pantallas que cubre | Por qué van juntas |
| --- | --- | --- |
| **F** | Tipologías (listado + alta/edición) | Es, con diferencia, la pantalla más simple del inventario (una entidad con un único campo, Nombre) — un brief propio y rápido. |
| **G** | Alta/Edición de Vivienda + Invitaciones de comprador | Ambas giran sobre una misma Vivienda concreta (datos físicos y gestión de sus compradores); tiene sentido diseñarlas como dos pestañas/pantallas del mismo contexto. |
| **H** | Detalle de Personalización (opciones + asignación) | La más rica de las pendientes (gestión de opciones + reasignación de "se aplica a"); se mantiene sola para no diluir el foco. |
| **I** | Progreso agregado + Progreso por vivienda | Ya son, en la app real, un Index/Details de un mismo `HousingPromotionChoicesController` — siempre se han diseñado/planteado como pareja. |
| **J** | Mi promotora (perfil + marca) + Crear/Editar Promoción | Ambas son formularios de metadatos + imagen (logo vs. plano general), uno a nivel global y otro a nivel de Promoción — comparten estructura y son rápidas de revisar juntas. |

#### Brief F — Tipologías (listado + alta/edición)

```
Dirección ya acordada (no la cambies): sidebar CONTEXTUAL de Promoción
(Tipologías activo), mismo sistema visual ya usado en Gremios/
Personalizaciones (paleta Slate, Inter, radios 0.5rem/0.75rem, acento de
marca solo en logo/nav activo/botón primario/notificación/progreso).

Pantalla a diseñar: las "Tipologías" de Kiwbi. Una Tipología es solo una
etiqueta que las Viviendas pueden compartir opcionalmente (p. ej. "2 dorm.
Tipo A") para poder asignarles Personalizaciones "por tipología" en vez de
una a una - no tiene más campo que el Nombre:

1) Listado: tabla con columnas Nombre y Nº de viviendas que usan esa
   tipología, acciones Editar/Eliminar por fila. Botón "Nueva tipología"
   arriba.

2) Formulario de alta/edición: un único campo de texto, el Nombre.

Genera un único mockup coherente para ambas vistas (listado + formulario) -
es deliberadamente la pantalla más simple de todo el inventario.
```

#### Brief F — resultado (2026-09-20)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/tipologias.tsx` (`Tipologias`, listado+formulario conmutados por estado, mismo patrón que Gremios/Personalizaciones). Muy fiel al brief, y deliberadamente la pantalla más simple generada hasta ahora: tabla con Nombre (icono+nombre) y Nº de viviendas, acciones Editar/Eliminar/más; formulario con un único campo de texto (Nombre). Datos de ejemplo `TIPOLOGIAS_ROWS`/`TipologiaRow` añadidos a `data.ts` (5 tipologías: "2 dorm. · Tipo A" (12), "2 dorm. · Tipo B" (8), "3 dorm. · Tipo C" (18), "4 dorm. · Ático" (6), "Bajo con jardín" (4)).

Traducido, con la misma decisión de arquitectura no-SPA ya aplicada a Brief B/E, a `wwwroot/design-preview/tipologias.html` (listado) + `tipologias-nueva.html` (alta, reutilizada también como destino de "Editar"). No hizo falta ninguna clase CSS nueva: reutiliza `.kiwbi-table-card`/`.kiwbi-row-icon` para el listado y `.kiwbi-fieldset`/`.kiwbi-legend` para el formulario, exactamente igual que Gremios.

Cerrado el bucle de navegación: el enlace "Tipologías" del sidebar contextual (antes `#`) en las 6 pantallas que ya lo tenían (`viviendas.html`, `personalizaciones.html`, `personalizaciones-nueva.html`, `gremios.html`, `gremios-nuevo.html`, `promocion-resumen.html`) y la tarjeta de acceso directo "Tipologías" de `promocion-resumen.html`, ahora apuntan a `tipologias.html`. Validado en navegador (captura): listado y formulario coinciden con el mockup.

#### Brief G — Vivienda: alta/edición + invitaciones de comprador

```
Dirección ya acordada (no la cambies): sidebar CONTEXTUAL de Promoción
(Viviendas activo en ambas pantallas - el listado ya está diseñado, esto
añade el formulario de alta/edición y la gestión de invitaciones de una
vivienda concreta).

Pantalla 1 - Alta/Edición de Vivienda: formulario con Planta, Puerta, un
<select> de Tipología (opcional, con una opción "Sin tipología asignada"),
Superficie construida en m² (obligatoria) y Superficie útil en m²
(opcional, no puede superar la construida), y un campo de subida de imagen
para el plano de la vivienda (con vista previa si ya existe, placeholder
si no). El Estado comercial (Disponible/Reservada/Vendida) NO se edita
aquí junto al resto de campos: muéstralo solo como un badge de solo
lectura en la cabecera del formulario cuando es edición (se cambia con una
acción aparte ya implementada, fuera de este brief).

Pantalla 2 - Invitaciones de comprador (de esa misma vivienda): cabecera
con la vivienda (p. ej. "Vivienda 1º A"), un formulario simple para
invitar (campo Email + botón "Enviar invitación"), y dos tablas: una de
invitaciones enviadas (Email, Estado - Pendiente/Aceptada/Cancelada -,
fecha de caducidad, acciones Reenviar/Cancelar solo disponibles si está
Pendiente) y otra de compradores ya vinculados a esta vivienda (Email,
fecha de aceptación).

Genera un mockup coherente por cada una de las dos pantallas.
```

#### Brief G — resultado (2026-09-20)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/vivienda-editor.tsx` (`ViviendaEditor`), con las dos pantallas conmutadas por un selector de pestañas de demo ("Alta/Edición" vs. "Invitaciones") sobre el mismo sidebar contextual (`activeLabel="Viviendas"`). Muy fiel al brief en ambas pantallas:
- **Alta/Edición**: fieldsets "Ubicación" (Planta, Puerta, Tipología con `<select>` poblado desde `TIPOLOGIAS_ROWS`), "Superficies" (construida obligatoria, útil opcional con validación en vivo `useState` de "no puede superar la construida" — solo posible client-side en el mockup, en Razor real será validación de servidor + `data-val` de ASP.NET), "Plano de la vivienda" (preview o placeholder + dropzone de subida + botón "Quitar plano actual"). Badge de Estado comercial (`Reservada`) explícitamente de solo lectura junto al título, con una nota aclaratoria debajo, tal como se pidió.
- **Invitaciones**: formulario de invitar por email + tabla de invitaciones (`INVITACIONES` en `data.ts`: 2 Pendientes con Reenviar/Cancelar, 1 Cancelada y 1 Aceptada sin acciones) + tabla de compradores vinculados (`COMPRADORES_VINCULADOS`, con avatar de inicial).

Traducido a `wwwroot/design-preview/vivienda-editar.html` + `vivienda-invitaciones.html`, con un nuevo bloque `.kiwbi-seg-tabs`/`.kiwbi-seg-tab` en `kiwbi-preview.css` (dos enlaces reales, no un toggle JS, según la misma regla no-SPA ya aplicada) para navegar entre ambas. Nuevas clases añadidas: `.kiwbi-dropzone` (área de subida con borde discontinuo) y `.kiwbi-plano-preview` (miniatura del plano actual o placeholder); los badges de Pendiente/Aceptada/Cancelada y Disponible/Reservada/Vendida reutilizan `.kiwbi-badge-amber`/`.kiwbi-badge-success`/`.kiwbi-badge-neutral` ya existentes, sin necesitar nada nuevo.

El botón "Nueva vivienda" de `viviendas.html` (antes un `<button>` sin acción) ahora enlaza a `vivienda-editar.html`. Nota pendiente: la tabla de `viviendas.html` (heredada de la Ronda 1, con columnas de progreso de personalización en vez de columnas CRUD reales) todavía no tiene una columna de acciones por fila para enlazar a estas dos pantallas nuevas — ya estaba documentado como una brecha de esa pantalla (estados ilustrativos, no mapeados a los enums reales); se resolverá junto con esa pantalla en la Feature 8.2/8.3, no bloquea Brief G.

Validado en navegador (capturas): formulario con validación de superficie, dropzone, badge de solo lectura, y las dos tablas de invitaciones/compradores coinciden con el mockup.

#### Brief H — Detalle de Personalización (gestión de opciones y asignación)

```
Dirección ya acordada (no la cambies): sidebar CONTEXTUAL de Promoción
(Personalizaciones activo) - es la pantalla a la que se llega al pulsar
"Detalle" desde el listado ya diseñado.

Pantalla a diseñar: el detalle de una Personalización concreta (p. ej.
"Tipo de suelo" del gremio Pavimentos), con dos bloques:

1) Opciones: tabla/lista de las opciones de esta personalización (Nombre,
   Sobrecoste en €, y un badge/marca visual sobre cuál es la "opción por
   defecto"), con acciones Editar/Eliminar por opción y un botón "Añadir
   opción" que abre un formulario simple (Nombre + Sobrecoste) en la misma
   pantalla o como modal, a tu criterio. Ten en cuenta al diseñarlo (no
   hace falta que lo bloquees visualmente, es solo contexto): una opción
   no se puede eliminar si es la única que queda, ni si es la opción por
   defecto actual sin marcar antes otra como tal.

2) Asignación ("se aplica a"): reutiliza el mismo selector de 3 opciones ya
   usado en el alta (Toda la promoción / Tipología / Vivienda) para
   mostrar y poder cambiar a qué tipologías o viviendas concretas se
   aplica esta personalización, con los mismos chips/pills de selección
   múltiple ya usados en el formulario de alta.

Genera un único mockup coherente con la dirección ya acordada.
```

#### Brief H — resultado (2026-09-20)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/personalizacion-detalle.tsx` (`PersonalizacionDetalle`), sidebar contextual con "Personalizaciones" activo. Muy fiel al brief, con los dos bloques pedidos:
- **Opciones**: lista con un marcador de estrella para "opción por defecto" (rellena y coloreada con el acento si lo es, hueca si no — clic para marcarla), badge "Por defecto" junto al nombre, sobrecoste en € (o "Incluido" si es 0), acciones Editar/Eliminar por opción con las reglas de bloqueo ya pedidas (no se puede eliminar la única opción restante, ni la opción por defecto actual sin marcar otra antes — reflejado con el botón Eliminar deshabilitado + `title` explicativo), y un formulario "Añadir opción" (Nombre + Sobrecoste).
- **Se aplica a**: reutiliza EXACTAMENTE el mismo selector de 3 opciones y las pills multi-selección ya construidos en `personalizaciones-nueva.html` (Brief B) — sin inventar nada nuevo, tal como pedía el brief.

Datos de ejemplo `PERSONALIZACION_DETAIL`/`OpcionRow` añadidos a `data.ts` ("Tipo de suelo" del gremio Pavimentos, ámbito `tipologia` con "3 dorm. · Tipo C" y "4 dorm. · Ático" ya seleccionadas, 4 opciones con Roble natural como opción por defecto).

Traducido a `wwwroot/design-preview/personalizacion-detalle.html`, con 3 nuevas clases en `kiwbi-preview.css`: `.kiwbi-option-row` (fila de opción), `.kiwbi-option-star`/`.is-default` (marcador de opción por defecto) y `.kiwbi-badge-accent` (badge "Por defecto"); el bloque "Se aplica a" reutiliza `.kiwbi-scope-option`/`.kiwbi-pill` ya existentes sin cambios. El formulario "Añadir opción" se muestra siempre visible en esta PoC estática (sin el toggle `useState` del mockup, ya que no hay JS de estado en estas páginas) — decisión de traducción menor, no afecta al contenido.

Cerrado el bucle de navegación: los 6 botones "Detalle" de `personalizaciones.html` (antes `<button>` sin acción) ahora enlazan todos a `personalizacion-detalle.html`, reutilizada como página representativa única (mismo criterio ya aplicado a `gremios-nuevo.html`/`tipologias-nueva.html` para "Editar").

Validado en navegador (capturas): lista de opciones con badge/estrella de opción por defecto y sobrecostes, y el bloque "Se aplica a" con el scope "Tipología" activo y sus dos pills preseleccionadas, coinciden con el mockup.

#### Brief I — Progreso de personalizaciones (agregado + detalle por vivienda)

```
Dirección ya acordada (no la cambies): sidebar CONTEXTUAL de Promoción
(Progreso activo en ambas pantallas).

Pantalla 1 - Progreso agregado: tabla con una fila por Vivienda de la
promoción (Planta, Puerta, Tipología) y 4 columnas de recuento -
Pendientes / Seleccionadas / Confirmadas / Pagadas - en las que se reparte
el total de Personalizaciones aplicables a esa vivienda, con una barra de
progreso visual combinando los 4 recuentos por fila, y una acción "Ver
detalle" por fila.

Pantalla 2 - Detalle por vivienda: cabecera con la vivienda, y debajo sus
Personalizaciones agrupadas por Gremio (mismo patrón visual que la pantalla
de comprador ya diseñada, pero orientado a la promotora): cada fila
muestra el nombre de la Personalización, la opción actualmente efectiva,
un badge de estado (Pendiente/Seleccionada/Confirmada/Pagada), y botones de
acción "Confirmar" (solo si el gremio ya venció y hay una opción elegida) o
"Marcar como pagada" (solo si ya está Confirmada), según corresponda fila a
fila.

Genera un mockup coherente por cada una de las dos pantallas.
```

#### Brief I — resultado (2026-09-20)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/progreso.tsx` (`Progreso`), sidebar contextual con "Progreso" activo. Muy fiel al brief, con una decisión de diseño explícita y acertada que conviene conservar: **los 4 colores de estado (Pendiente/Seleccionada/Confirmada/Pagada) son fijos y NO usan el acento del tenant** — deben leerse igual en cualquier promotora, al ser estados de negocio reales (`HomeCustomizationChoiceStatus`), no un elemento de marca.

- **Pantalla 1 (agregado)**: leyenda con los 4 estados + sus recuentos totales de la promoción y el total general, y una tabla con una fila por Vivienda (Planta·Puerta, Tipología, 4 columnas de recuento, una barra apilada "Reparto" combinando los 4 colores proporcionalmente, y "Ver detalle" por fila).
- **Pantalla 2 (detalle)**: cabecera con la vivienda + 4 chips de recuento (mismo color que la leyenda), y sus Personalizaciones agrupadas por Gremio reutilizando `.kiwbi-gremio-badge-open`/`.kiwbi-gremio-badge-expired` ya existentes (Abierto/Cerrado); cada línea muestra Nombre + opción efectiva, un badge de estado, y la acción correspondiente calculada con las reglas exactas pedidas: "Confirmar" solo si el gremio venció y hay opción elegida (`Seleccionada`), "Marcar como pagada" solo si está `Confirmada`, o "Cerrada"/"Sin acciones" en el resto de casos.

Datos de ejemplo `PROGRESO_VIVIENDAS`/`PROGRESO_ORDER`/`progresoCounts()` añadidos a `data.ts` (4 viviendas de ejemplo con distintas combinaciones de estado, incluyendo una que demuestra "Marcar como pagada" y otra con el estado `Seleccionada` ya vencido).

Traducido a `wwwroot/design-preview/progreso.html` (agregado) + `progreso-detalle.html` (detalle de la vivienda "1ª A", la que mejor combina los 4 estados y las dos acciones), con 3 grupos de clases nuevas en `kiwbi-preview.css`: `.kiwbi-status-dot`/`.kiwbi-stacked-bar` (leyenda y barra de reparto), `.kiwbi-badge-pendiente/-seleccionada/-confirmada/-pagada` (colores fijos, iguales a los del mockup) y `.kiwbi-progreso-chip` (chips de recuento de la cabecera de detalle); el bloque de Gremio reutiliza `.kiwbi-gremio-badge-open`/`.kiwbi-gremio-badge-expired` sin cambios.

Cerrado el bucle de navegación: el enlace "Progreso" del sidebar contextual (antes `#`) en las 10 pantallas que lo tenían, y la tarjeta de acceso directo "Progreso" de `promocion-resumen.html`, ahora apuntan a `progreso.html`; solo la fila "1ª · A" del agregado enlaza a `progreso-detalle.html` (única vivienda con página de detalle traducida en esta PoC), el resto de filas mantienen un botón "Ver detalle" sin acción.

Validado en navegador (capturas): leyenda + tabla con barras de reparto proporcionales, y la cabecera de detalle con chips + los 3 gremios (cerrado con "Cerrada", cerrado con "Marcar como pagada" x2, abierto con "Sin acciones") coinciden con el mockup.

**Con Brief I se completa todo el inventario de pantallas de la Ronda 3/4 salvo Brief J (Mi promotora + Crear/Editar Promoción), el último pendiente.**

#### Brief J — Mi promotora (perfil + marca) y Crear/Editar Promoción

```
Dirección ya acordada (no la cambies): Mi promotora usa el sidebar GLOBAL
("Mi promotora" activo); Crear/Editar Promoción usa el sidebar GLOBAL si es
alta (se crea desde el listado de Promociones) o el CONTEXTUAL con
"Resumen" activo si es edición (se edita desde el hub de una promoción ya
existente) - el contenido del formulario es el mismo en ambos casos, solo
cambia qué sidebar lo envuelve; no hace falta que dupliques el mockup por
eso si prefieres mostrar una sola variante de sidebar.

Pantalla 1 - Mi promotora: formulario con Nombre de la promotora, un campo
de subida de Logo (con vista previa, y un fallback tipo "iniciales sobre
color" si todavía no hay logo, igual que ya se ve en el sidebar), y dos
selectores de color (Color primario / Color secundario) con una vista
previa en vivo de cómo quedaría el acento de marca sobre un botón y un item
de navegación de ejemplo (reutiliza la idea ya validada del selector de
tenant que aparece en las pantallas ya generadas).

Pantalla 2 - Crear/Editar Promoción: formulario con Nombre, Ciudad,
Dirección, y un campo de subida de imagen para el plano general (con vista
previa si ya existe, placeholder si no).

Genera un mockup coherente por cada una de las dos pantallas.
```

#### Brief J — resultado (2026-09-20)

Generado en el mismo proyecto/hilo (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), como `components/kiwbi/promotora-settings.tsx` (`PromotoraSettings`), sidebar GLOBAL para ambas pantallas (tal y como se pidió: "Mi promotora" activo en la primera, "Panel" activo en la segunda porque la alta se dispara desde el listado). Muy fiel al brief, con dos añadidos propios que merece la pena conservar:
- **Paletas rápidas**: en el bloque de Colores de marca, un grupo de "pills" reutilizando los 4 tenants de muestra ya usados en el interruptor de marca del sidebar, para aplicar de un clic una combinación primario/secundario ya probada.
- **Vista previa en vivo**: dentro del mismo bloque, una previsualización en miniatura de un item de nav activo + botón primario/secundario + dos barras de progreso, todo coloreado con los valores actuales de primario/secundario — refuerza visualmente el alcance ya decidido del acento de marca (nav activo/botón/progreso) antes de guardar.
- **Mi promotora**: Nombre, Logo (preview con fallback de iniciales sobre color, igual que el ya usado en el sidebar, + dropzone de subida + "Quitar logo"), y los dos selectores de color descritos arriba.
- **Crear/Editar Promoción**: Nombre, Ciudad, Dirección, y un dropzone para el plano general (con preview-o-placeholder), igual que se pidió.

Traducido a `wwwroot/design-preview/mi-promotora.html` + `promocion-nueva.html`, ambas con la sidebar GLOBAL. Nuevas clases en `kiwbi-preview.css`: `.kiwbi-color-field`/`.kiwbi-color-swatch`/`.kiwbi-color-hex` (selector de color nativo + hex editable), `.kiwbi-palette-pill`/`.kiwbi-palette-swatch` (paletas rápidas) y `.kiwbi-logo-preview` (logo o iniciales); la vista previa en vivo reutiliza patrones ya existentes (`.kiwbi-progress` con `background` de la barra sobrescrito inline para primario/secundario). `promocion-nueva.html` reutiliza `.kiwbi-dropzone` ya creado en Brief G.

Cerrado el bucle de navegación: "Mi promotora" (antes `#`) en `dashboard-promotora.html` ahora enlaza a `mi-promotora.html`; el botón "Nueva promoción" de `dashboard-promotora.html` (antes sin acción) y el botón "Editar promoción" de `promocion-resumen.html` (antes sin acción) ahora enlazan ambos a `promocion-nueva.html`, reutilizada como página representativa única para alta y edición (mismo criterio ya aplicado en briefs anteriores).

Validado en navegador (capturas): formulario de Mi promotora con paletas rápidas y vista previa en vivo coloreada correctamente, y el formulario de Nueva promoción con el dropzone del plano general, coinciden con el mockup.

**Con Brief J se completa el inventario íntegro de pantallas de las Rondas 3 y 4.** Quedan fuera de este inventario, deliberadamente, las pantallas de Login/Register/aceptación de invitación (marcadas "fuera de prioridad" desde la Ronda 3) — el resto de pantallas de Kiwbi ya está diseñado, traducido a Bootstrap 5 y validado en navegador.

### Ronda 5 (auditoría de coherencia previa al cierre) — 2026-09-20

Antes de dar por cerrada la Feature 8.1, se hizo una auditoría exhaustiva cruzando cada uno de los 19 ficheros de `wwwroot/design-preview/` contra: (a) el grafo de navegación completo (todos los `href` internos), y (b) el código real de Controllers/Application/Domain que cada pantalla representa — no solo el aspecto visual, sino la lógica de negocio y los flujos tal y como están implementados hoy. Objetivo: detectar antes de la Feature 8.2 cualquier caso en el que el mockup se haya desviado de cómo funciona realmente Kiwbi.

**Grafo de navegación: cerrado correctamente.** De los ~150 enlaces internos revisados, los únicos `href="#"` restantes son los 3 del item de nav "Ajustes" (ver hallazgo 7 más abajo) — ninguna otra pantalla del inventario tiene un enlace roto o pendiente sin justificar.

**Hallazgos que requieren decisión o corrección antes/durante la Feature 8.2:**

1. **Detalle de Personalización — "Se aplica a" (Brief H) no refleja el flujo real.** El mockup reutilizó el patrón de selector de scope + pills + "Guardar asignación" en bloque del formulario de alta (Brief B). La `Details.cshtml` real (`CustomizationsController.Details`, ya implementada desde Epic 3) funciona de forma bien distinta: una tabla de asignaciones actuales con un botón "Quitar" individual por fila, más dos mini-formularios separados "Añadir tipología" / "Añadir vivienda" (cada uno con su propio `<select>` + botón, una asignación a la vez vía `AddTypologyAssignment`/`AddUnitAssignment`/`RemoveAssignment`). Además, **no existe ningún caso de uso para convertir una personalización ya creada a "Toda la promoción"** — el dominio (`EnsureCanAddSpecificAssignment`) solo impide añadir asignaciones concretas si ya es de toda la promoción, pero no hay operación inversa. Antes de aplicar el sistema de diseño a la vista real, hay que rehacer el layout de este bloque siguiendo el patrón añadir/quitar granular, no el de scope-selector.
2. **Progreso — la condición de "Confirmar" en el brief y el mockup es más restrictiva que la real.** Escribí (y v0 implementó fielmente) `canConfirm = gremio.venció && hasChoice && estado === 'Seleccionada'`. La regla real (`Details.cshtml` de `HousingPromotionChoicesController`, y el propio `ConfirmHomeCustomizationChoiceUseCase`) es `tradeCategory.IsExpired && status is Pending or Selected` — es decir, **también se puede Confirmar una personalización en estado Pendiente** si su gremio ya venció (el caso de uso asigna automáticamente la opción por defecto antes de confirmar). Los datos de ejemplo de `progreso.html`/`progreso-detalle.html` no llegan a exponer visualmente este caso (ninguna vivienda de muestra tiene un gremio vencido con línea Pendiente), así que no hay una pantalla "rota", pero la regla tal y como quedó escrita en el brief es incorrecta y debe corregirse antes de que se use como referencia en la Feature 8.2.
3. **Tipologías — la columna "Nº de viviendas" no tiene datos reales detrás.** `HousingTypologyDto`/`GetHousingTypologiesUseCase` solo exponen `Id`, `Name`, fechas — ningún recuento de viviendas asociadas. La Feature 8.2 necesitará añadir esa capacidad (nuevo cálculo en el use case o una consulta agregada), no es solo un cambio de estilos.
4. **Gremios — el formulario real usa un único campo `datetime-local`**, no Fecha y Hora por separado como en `gremios-nuevo.html`. Diferencia menor (mismo dato, distinta composición de inputs) pero hay que reconciliarla al construir la vista Razor real.
5. **Mi promotora — el upload de logo NO está implementado hoy.** `UpdateDeveloperBrandingCommand(PrimaryColor, SecondaryColor, LogoPath)` recibe `LogoPath` como `string`, sin subida de fichero real (a diferencia del patrón ya usado para plano de Vivienda/Promoción con `IFileStorageService`). El dropzone de `mi-promotora.html` anticipa una capacidad que **no existe en Application/Infrastructure todavía** y que, además, excede el alcance declarado de este Epic ("100% Kiwbi.Web"). Requiere una decisión explícita: (a) ampliar `UpdateDeveloperBrandingUseCase` para aceptar un fichero (pequeño cambio de Application/Infrastructure, análogo al ya existente para planos), o (b) recortar esta pantalla a un campo de texto/URL en la Feature 8.2 y dejar la subida real de logo para otro Epic.
6. **Invitaciones de comprador — faltan un estado visual y una columna.** La vista real (`Invitations.cshtml`) distingue **4 estados**, no 3: Aceptada / Cancelada / **Caducada** (`IsExpired`, calculado, distinto de Pendiente aunque el dominio solo persiste Pending/Accepted/Cancelled) / Pendiente — `vivienda-invitaciones.html` solo modela Pendiente/Aceptada/Cancelada, sin el badge "Caducada". También falta la columna "Enviada" (`CreatedAtUtc`) junto a "Caduca" (`ExpiresAtUtc`). Los botones Reenviar/Cancelar sí están bien acotados (dependen del `Status` de dominio = Pending, no de si venció, así que una invitación caducada-pero-Pending real seguiría siendo reenviable/cancelable — esto sí coincide con el mockup).
7. **El nav item "Ajustes" no corresponde a ninguna pantalla del inventario.** Aparece en el sidebar GLOBAL de las 3 pantallas que lo llevan (`dashboard-promotora.html`, `mi-promotora.html`, `promocion-nueva.html`) apuntando a `#`, heredado de la Ronda 1 sin que la Ronda 3 lo evaluara ni le asignara un brief — a diferencia de Login/Register/aceptación de invitación, esto **no fue una exclusión deliberada**, es un hueco del propio inventario. No hay ningún Controller/pantalla real de "Ajustes" en Kiwbi hoy. Requiere decisión: quitarlo del nav (lo más simple, ya que no representa nada implementado) o documentarlo como backlog futuro (p. ej. dentro de Epic 9, preferencias de cuenta/seguridad).

**Sin incidencias (verificado explícitamente):**
- Viviendas — alta/edición (`vivienda-editar.html`): campos y flujo (Planta/Puerta/Tipología/Superficies/Plano en un único formulario, Estado comercial como acción aparte) coinciden exactamente con `HousingUnitsController.Create/Edit`.
- Promociones — alta/edición (`promocion-nueva.html`): Nombre/Ciudad/Dirección/Plano en un único formulario, coincide con `HousingPromotionsController.Create`.
- Personalizaciones — opciones (bloque "Opciones" de `personalizacion-detalle.html`): reglas de "no eliminar la única opción" / "no eliminar la opción por defecto sin fijar otra antes" coinciden con `Customization.RemoveOption` en Domain.
- El resto del grafo de navegación (Resumen/Tipologías/Viviendas/Gremios/Personalizaciones/Progreso, y las páginas "representativas únicas" reutilizadas para Editar/Detalle en varias filas de una tabla) es una simplificación consciente y ya documentada del PoC estático, no una incoherencia — se resolverá de forma natural cuando la Feature 8.2 use rutas reales con `id`.

**Decisión pendiente del usuario:** cuáles de los hallazgos 1-7 se corrigen ahora (en los ficheros estáticos, para que la Feature 8.2 parta de una referencia ya correcta) y cuáles se dejan como nota para resolver directamente al construir la vista Razor real.

### Implementación del mecanismo de branding (2026-09-19)

El mecanismo decidido arriba ya está implementado y validado, por delante del resto de la Feature 8.1 (no requería esperar a más mockups, era puramente técnico):

- **`GetBrandingForHousingUnitUseCase`** (nuevo, `Kiwbi.Application.Developers.GetBrandingForHousingUnit`): dado un `housingUnitId`, resuelve `HousingUnit → HousingPromotion → DeveloperCompany` y reutiliza el mapeo `internal` ya existente de `GetCurrentDeveloperProfileUseCase.ToDto` (mismo patrón de *mapper* interno compartido dentro de Application ya usado en Epics anteriores). Es la única incorporación a Application que exige este Epic — sigue sin haber ningún cambio en Domain ni Infrastructure, y no introduce ningún nuevo endpoint HTTP (el "como mucho un endpoint de CSS dinámico" que contemplaba el alcance original del Epic no ha hecho falta).
- **`TenantBrandingViewComponent`** (nuevo, `Kiwbi.Web/Components/`): invocado desde `_Layout.cshtml` (`@await Component.InvokeAsync("TenantBranding")`), decide qué `DeveloperCompany` aplica según el rol/ruta actual exactamente como se documentó (Promotora vía `ICurrentUser.DeveloperCompanyId` + `GetCurrentDeveloperProfileUseCase`; Comprador solo cuando `RouteData` es `Buyer/HousingUnit/{id}` vía el nuevo use case; cualquier otro caso no renderiza nada). Su vista (`Views/Shared/Components/TenantBranding/Default.cshtml`) emite el mismo bloque `<style>` con `--kiwbi-accent`/`--kiwbi-accent-2` ya validado en el PoC.
- **Prueba mínima de que llega a CSS real:** se añadió una única regla a `site.css` (`.navbar-light .navbar-brand { color: var(--kiwbi-accent, inherit); }`) que recolorea el texto "Kiwbi.Web" de la barra de navegación actual cuando hay branding, y cae a `inherit` (el negro por defecto de Bootstrap) cuando no lo hay. **No es un rediseño de la navegación** — el sidebar/topbar reales de Operativo+Blueprint siguen pendientes de la Feature 8.2/8.3; esto es solo la prueba de que el dato fluye de la base de datos real hasta el CSS renderizado.
- **Validado manualmente end-to-end** (registro de una promotora de prueba vía `/Account/Register`, edición de su color primario a `#C0392B` vía `/DeveloperProfile/EditBranding`, captura de pantalla confirmando el cambio de color del navbar-brand) y verificado que al cerrar sesión el navbar-brand vuelve al negro por defecto (sin fuga de branding a páginas anónimas). El *build* completo y los 305 tests de Domain+Application (131+174) siguen en verde tras el cambio. La ruta de Comprador (`Buyer/HousingUnit`) comparte exactamente el mismo patrón de código que la de Promotora (ya cubierta end-to-end) y se validó por revisión de código, no por click-through completo (habría exigido sembrar una Promoción/Vivienda/invitación de comprador aceptada solo para esta prueba).

### Cómo proceder después de generar en v0.dev (flujo ya validado en la ronda 1)

Esto ya no es teórico: es exactamente el flujo que funcionó para "Operativo · Sidebar denso". A partir de ahora, para cada brief de esta sección:

1. **Genera el mockup en v0.dev usando el brief.** Recomendado: continuar en el **mismo hilo/proyecto** de v0.dev que ya generó Operativo/Editorial/Blueprint/Cálido (así v0 parte del mismo lenguaje visual sin tener que reexplicárselo). Si prefieres empezar un hilo nuevo, pega primero el párrafo "Dirección ya acordada" que encabeza cada brief siguiente, para anclarlo al mismo estilo antes de pedirle la pantalla nueva.
2. **A diferencia de la ronda 1, no hace falta pedir 3-4 variaciones divergentes.** Ya no estamos explorando estética, sino aplicando la dirección ya elegida a una pantalla concreta: pide **un único mockup coherente** con esa dirección. Solo pide 2 variantes si hay una duda de *layout* genuina y acotada (p. ej. "¿la ficha de cada Personalización se ve mejor como fila de tabla expandible o como tarjeta?"), nunca para volver a comparar paletas o tipografías ya decididas.
3. **Cuando el resultado te convenza** (puedes pedir ajustes dentro del propio chat de v0.dev antes de darlo por bueno), descarga el proyecto completo (el botón de descarga/exportación que ofrezca la interfaz de v0.dev en ese momento) a una carpeta local. Puede ser la misma `C:\Users\cgran\Documents\identidad-visual-kiwbi` (v0.dev permite seguir añadiendo pantallas al mismo proyecto) o una carpeta nueva si prefieres mantenerlas separadas — cualquiera de las dos me vale.
4. **Dime la ruta absoluta de esa carpeta** (aunque sea la misma de siempre, confírmamelo para saber que ya está actualizada) **y qué pantalla/variación concreta quieres que revise** si generaste más de una. No hace falta que la copies dentro de `C:\Kiwbi`: leo archivos fuera del workspace por ruta absoluta, tal como hice con `identidad-visual-kiwbi`.
5. **A partir de ahí, estos son mis pasos** (los mismos que ya hice con Operativo/Blueprint): reviso el código fuente real que generó v0 (no una captura) para extraer con precisión paleta/tipografía/estructura, lo contrasto con lo ya decidido (paleta Slate, Inter, sidebar híbrido, alcance del acento de marca, mapeo a los datos/enums reales de Kiwbi), lo traduzco a Bootstrap 5 real ampliando `wwwroot/design-preview/` con un fichero nuevo (o reutilizando el patrón de `viviendas.html`), y lo verifico en el navegador (capturas + interacciones, como ya hice con el colapso del sidebar y el interruptor de marca) antes de documentar la dirección final de esa pantalla en este Epic.

### Ronda 2 (pantallas priorizadas) — briefs (2026-09-19)

Tres briefs, uno por cada pantalla priorizada pendiente (el Layout/Navegación base y el listado de Viviendas ya quedaron resueltos por el PoC). Cada uno incluye primero un párrafo de "dirección ya acordada" (para anclar el estilo si usas un hilo nuevo de v0.dev) y después el contenido específico de esa pantalla, con los datos/columnas reales de Kiwbi para que el mockup salga ya útil, no genérico.

#### Brief A — Dashboard Promotora (`HousingPromotions/Index`)

```
Dirección ya acordada (no la cambies, ya está decidida): sigue exactamente el
mismo sistema visual de "Operativo · Sidebar denso" que ya generaste antes -
sidebar con dos estados (expandido ~224px con logo+nombre de tenant+labels,
colapsado a icon-rail ~64px solo iconos), topbar con buscador/notificaciones/
avatar, paleta base neutra fría tipo Slate (fondo #f8fafc, bordes #e2e8f0,
texto #0f172a/#64748b), tipografía Inter, radios de 0.5rem (botones/inputs) y
0.75rem (cards/tablas). El acento de marca del tenant solo se aplica en:
logo, item de nav activo, botón primario, punto de notificación y barras de
progreso - nunca en fondo general ni texto.

Pantalla a diseñar: el Dashboard de la Promotora, punto de entrada tras iniciar
sesión. Es el listado de sus Promociones (edificios en construcción), no de
Viviendas individuales - reutiliza la misma fila de 4 KPI cards ya usada en
Viviendas (Promociones activas, Viviendas en curso, Personalizaciones
pendientes, Gremios coordinados), y debajo una tabla de Promociones con estas
columnas reales: Nombre, Ciudad, Dirección, Nº de viviendas, y una acción
"Ver"/"Editar" por fila. Botón principal "Nueva promoción" arriba a la derecha,
igual patrón que "Nueva vivienda" en la pantalla ya generada. Si no hay
ninguna promoción todavía, muestra un estado vacío con el mismo botón.

Genera un único mockup coherente con la dirección ya acordada (no variaciones
divergentes de paleta/tipografía, eso ya está decidido).
```

#### Brief B — CRUD de Personalizaciones (`Customizations/Index` agregado + `Customizations/Create`)

```
Dirección ya acordada (no la cambies): igual que en el Brief A - sistema
"Operativo + sidebar híbrido" ya generado, paleta Slate, Inter, radios
0.5rem/0.75rem, acento de marca solo en logo/nav activo/botón primario/
notificación/progreso.

Pantalla a diseñar: dos vistas de un CRUD real, la de "Personalizaciones" de
Kiwbi (piensa en ello como el catálogo de opciones que un comprador podrá
elegir para su vivienda - p. ej. tipo de suelo, grifería - agrupadas por
Gremio):

1) Listado (vista "agregada", sin filtrar por un Gremio concreto): tabla con
   columnas Nombre, Gremio, Nº de opciones, "Se aplica a" (texto corto tipo
   "Toda la promoción" / "2 tipologías" / "3 viviendas"), y acciones
   Detalle/Eliminar por fila. Botón "Nueva personalización" arriba. Si la
   promoción todavía no tiene ningún Gremio dado de alta, el botón aparece
   deshabilitado con un aviso corto invitando a crear un Gremio primero.

2) Formulario de alta: un <select> para elegir el Gremio, campo de texto para
   el Nombre de la personalización, un bloque separado (visualmente
   diferenciado, es la "opción por defecto") con Nombre de la opción y
   Sobrecoste en euros, un selector de "Se aplica a" con 3 opciones (Toda la
   promoción / Tipología / Vivienda) que al elegir Tipología o Vivienda
   revela un selector múltiple correspondiente. Piensa en cómo estructurar
   este formulario con secciones claras (no todo en una columna plana) dado
   que tiene bastantes campos condicionales.

Genera un único mockup coherente con la dirección ya acordada para ambas
vistas (listado + formulario).
```

#### Brief C — Portal del Comprador (`Buyer/Index` + `Buyer/HousingUnit`)

```
Dirección ya acordada (no la cambies): mismo sistema base que en los briefs
anteriores (paleta Slate, Inter, radios 0.5rem/0.75rem), PERO con una
diferencia importante de alcance de marca respecto al panel de Promotora:

- En el Dashboard del Comprador (pantalla 1) NO se aplica ningún acento de
  marca de tenant - un comprador puede tener viviendas de varias promotoras
  a la vez, así que esta pantalla se queda neutra (sin sidebar de Promotora
  tampoco - el comprador no gestiona nada, solo consulta).
- En el Detalle de Vivienda (pantalla 2) SÍ se aplica el acento de marca de
  la promotora dueña de esa vivienda concreta, con más protagonismo que en
  el panel de Promotora (refuerza la relación promotora-comprador en el
  punto de contacto directo).

Pantalla 1 - Dashboard del Comprador ("Mis viviendas"): una rejilla de
tarjetas, una por cada vivienda que el comprador tiene asignada, con: nombre
de la Promoción, Ciudad, Planta y Puerta, y una miniatura del plano si existe
(si no, un placeholder neutro). Cada tarjeta lleva a la pantalla 2. Sin
sidebar ni densidad de datos - tono más simple y directo que el panel B2B.

Pantalla 2 - Detalle de Vivienda: el plano de la vivienda arriba, y debajo un
listado de Personalizaciones agrupadas por Gremio (nombre del Gremio, fecha
límite de selección, y una indicación clara de si ese Gremio ya venció -
en ese caso las opciones se ven bloqueadas/de solo lectura). Dentro de cada
Gremio, cada Personalización muestra su nombre y sus Opciones disponibles
como tarjetas/botones seleccionables (con su sobrecoste en euros), resaltando
cuál es la opción actualmente elegida/efectiva. Si el Gremio no ha vencido,
las opciones son clicables (selección interactiva); si ha vencido, se ven
igual pero sin poder interactuar.

Genera un único mockup coherente por cada una de las dos pantallas.
```

## Selección del enfoque de implementación

*(Antes titulada "Feature 8.1 - Investigación y selección de enfoque"; se conserva como la decisión de **cómo construir en código** la dirección visual que resulte de la exploración con IA anterior, no como punto de partida del proceso.)*

**Decisión (2026-09-18): Bootstrap 5 a medida.** El PoC de la sección anterior demuestra que la dirección "Operativo + sidebar híbrido" se reproduce con fidelidad usando el Bootstrap 5 ya vendorizado del proyecto más CSS/JS propios (variables CSS para el acento de tenant, clases `kiwbi-*` para sidebar/topbar/KPI cards/tablas/badges), sin necesitar ningún paso de build nuevo ni cambiar de framework. Se descartan las otras dos opciones de la tabla: no aporta suficiente diferenciación adicional adoptar una plantilla open-source de terceros (traería su propio JS a revisar contra HTMX/Alpine, para un resultado que el PoC ya iguala), y migrar a Tailwind introduciría un coste de build y de reescritura total de vistas injustificado dado que el resultado visual ya es indistinguible sin ese cambio.

### Opciones evaluadas

| Opción | Descripción | Ventajas | Inconvenientes |
| --- | --- | --- | --- |
| **Bootstrap 5 a medida** ✅ elegida | Sobrescribir variables Sass/CSS custom properties del Bootstrap ya usado, sin cambiar de framework. | Cero curva de aprendizaje, cero cambios de build (hoy no hay pipeline de compilación de CSS), riesgo mínimo. | Visualmente sigue "pareciendo Bootstrap" salvo inversión notable en overrides; menos diferenciador. |
| **Plantilla open-source basada en Bootstrap** (p. ej. Tabler, CoreUI, AdminLTE) | Adoptar una plantilla de panel de administración ya diseñada y con licencia permisiva (MIT), adaptando su markup a las vistas Razor. | Resultado profesional rápido, sigue siendo Bootstrap por debajo (compatible con el stack actual). | Puede traer JS propio que colisione con HTMX/Alpine (a revisar); adaptar cada vista existente al nuevo markup. |
| **Migración a Tailwind CSS** | Sustituir Bootstrap por un framework utility-first. | Máximo control visual y consistencia a largo plazo. | Introduce un paso de build (Tailwind CLI/PostCSS) inexistente hoy; reescritura de clases en todas las vistas; mayor coste de mantenimiento. |

Nota: la fila que originalmente cubría "Generación asistida por IA" como opción de implementación se sustituye por la nueva sección "Feature 8.1 - Exploración visual asistida por IA" de más arriba, que la trata como el punto de partida del proceso (ideación) en vez de como una alternativa más de implementación.

### Mecanismo de inyección del branding por tenant

Decisión tomada el 2026-09-19, sustituyendo la antigua sección "Consideración transversal a decidir en esta Feature" (que solo enumeraba opciones típicas sin resolver ninguna). Contexto relevante encontrado al analizar la implementación real: `ICurrentUser` hoy solo expone `DeveloperCompanyId` (no el `DeveloperCompany` completo ni su `Branding`), y no existe en `Kiwbi.Web` ningún mecanismo cross-cutting ya establecido (ni middleware, ni base controller, ni action filters) para inyectar datos comunes en `_Layout.cshtml` — esta Feature introduce el primero.

1. **Cómo se calcula y expone a `_Layout.cshtml`: View Component.** Nuevo `TenantBrandingViewComponent` (Web), invocado directamente desde `_Layout.cshtml` (`@await Component.InvokeAsync("TenantBranding")`), que resuelve el `DeveloperCompany`/`Branding` aplicable (ver punto 3) y devuelve una vista parcial con el bloque `<style>`. Se descartan middleware (más invasivo, corre en toda request no solo las que renderizan HTML) y ViewBag por Controller (obligaría a repetir la resolución en los ~11 controllers existentes); un View Component es el mecanismo idiomático de ASP.NET Core MVC para un widget de layout con lógica/inyección de dependencias propia, sin tocar ningún Controller existente.
2. **Cómo se materializa en CSS: bloque `<style>` inline**, generado en cada request por el propio View Component, con `:root { --kiwbi-accent: ...; --kiwbi-accent-2: ...; }` — exactamente el mecanismo ya validado en `wwwroot/design-preview/viviendas.html`. Se descarta un endpoint `.css` dinámico dedicado: con solo 2 colores hexadecimales el beneficio de caché HTTP es marginal, y evita tener que resolver invalidación de caché (ETag/versionado de URL) cuando una promotora edita su branding — con inline, el siguiente request ya refleja el cambio sin gestión adicional.
3. **Qué `DeveloperCompany` se usa según el rol/pantalla:**
   - Vistas de Promotora (`DeveloperAdmin`): `ICurrentUser.DeveloperCompanyId` es inequívoco, igual que en el resto de use cases ya existentes.
   - Vistas de Comprador (`Buyer`): un comprador puede tener viviendas de varias promotoras a la vez (`HousingUnitBuyer` es N:M), por lo que **no hay un único tenant** en `Buyer/Index` (dashboard agregado de todas sus viviendas) — esa vista se queda con el tema neutro de Kiwbi, sin acento de marca. El acento **solo se aplica en `Buyer/HousingUnit(id)`** (detalle de una vivienda concreta), donde `HousingUnit → HousingPromotion → DeveloperCompany` siempre resuelve a un único tenant sin ambigüedad; además refuerza la relación promotora-comprador justo en el punto de contacto directo, como ya apuntaba este mismo documento.
   - Landing page / páginas sin sesión: sin branding de ningún tenant (tema neutro de Kiwbi).
4. **Logo real vs. "mark" con inicial:** cuando `Branding.LogoPath` tiene valor, el View Component renderiza `<img>` con el logo real; si es `null` (caso por defecto desde Epic 1), se mantiene el fallback ya usado en el PoC (cuadrado de color de acento + inicial del nombre de la promotora).
5. **Validación de contraste del color de marca: diferida**, no se añade ninguna regla nueva en `BrandColor`/`Branding` (que hoy solo valida formato `#RRGGBB`) ni en el formulario de edición. El riesgo queda acotado porque el acento solo se usa en elementos pequeños y controlados (botón, nav activo, logo, barra de progreso), nunca como fondo de texto extenso. **Anotado como pendiente en `05-development-roadmap.md`** para revisitarlo si en el futuro se detectan casos reales de mala legibilidad.
6. **Rendimiento:** el View Component añade una consulta ligera (`DeveloperCompany`/`HousingPromotion` ya se cargan vía repositorio simple) por request; dado el volumen esperado (panel de administración de bajo tráfico, mismo argumento ya aceptado para el N+1 de Epic 7), no se añade caching adicional en esta Feature.


## Feature 8.2 - Sistema de diseño base

*(Plan de acción pendiente de detallar tras cerrar la Feature 8.1; dependerá de la herramienta elegida.)*

Alcance esperado, independientemente de la herramienta:
- Tokens de color (primario/secundario del tenant + paleta neutra/semántica: éxito, aviso, peligro — reutilizando los ya usados para los badges de `HousingUnitStatus` en Epic 2).
- Tipografía y espaciados base.
- Layout general (`_Layout.cshtml`, navegación) con la nueva identidad.
- Componentes comunes: botones, formularios, tablas, badges, cards, alerts, mensajes de validación.

## Feature 8.3 - Aplicación a las vistas existentes

*(Plan de acción pendiente de detallar tras cerrar la Feature 8.1; consistirá en recorrer las vistas de los Epics 1-6 ya implementados aplicando los nuevos componentes/clases, sin alterar la lógica de los Controllers ni los ViewModels.)*

## Consideraciones de Testing y Notas de la IA

- Este Epic no introduce lógica de negocio, por lo que no se esperan tests de Domain ni de Application. La validación es visual/manual (revisión de cada vista tras el retoque).
- Si el mecanismo de branding dinámico elegido en la Feature 8.1 introduce algún endpoint o servicio en `Kiwbi.Web` (p. ej. para servir CSS por tenant), evaluar en su momento si necesita tests (por ejemplo, que un tenant nunca vea los colores de otro).
- Mantener la separación Español (vistas/mensajes) / Inglés (código) ya establecida en el resto del proyecto.
- La Feature 8.1 es explícitamente iterativa y multi-sesión: no se espera cerrarla en una única conversación. Cada sesión puede limitarse a revisar la última tanda de mockups/capturas traída por el usuario, afinar el brief de la siguiente ronda, o consolidar por escrito una dirección ya acordada, sin necesidad de completar todo el plan de acción de golpe.
- Antes de programar este Epic, el usuario debe aprobar este documento, y en particular la decisión de la Feature 8.1 antes de empezar la 8.2.
