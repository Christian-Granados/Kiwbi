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
- [ ] Una vez implementado el mecanismo de branding, IA redacta los briefs del resto del subconjunto priorizado de pantallas: Dashboard Promotora, una pantalla CRUD representativa (p. ej. listado + formulario de Personalizaciones), Dashboard/Detalle Comprador (`Buyer/Index` y `Buyer/HousingUnit`) — el Layout/Navegación base y una pantalla de listado (Viviendas) ya quedan cubiertos por el PoC.
- [ ] Usuario genera mockups de esas pantallas ya alineados con la dirección acordada (nuevas rondas según haga falta).
- [ ] IA revisa, itera si hace falta, traduce a Bootstrap 5 real (ampliando el PoC o creando nuevos ficheros de referencia bajo `wwwroot/design-preview/`), y documenta en este Epic la dirección visual final por pantalla.
- [ ] Cerrar la Feature 8.1 dejando el PoC + mockups + decisiones documentadas como entrada de la Feature 8.2 (que convertirá esto en tokens/componentes reales integrados en las vistas Razor, con datos reales en vez de contenido de muestra).

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
