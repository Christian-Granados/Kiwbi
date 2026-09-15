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
- Investigación y decisión del enfoque/herramienta de diseño (Feature 8.1).
- Definición de tokens de diseño (colores, tipografía, espaciados) y de cómo el branding por tenant los sobreescribe (Feature 8.2).
- Componentes base reutilizables: layout/navegación, botones, formularios, tablas, badges, cards, alerts (Feature 8.2).
- Aplicación del nuevo sistema a las vistas ya existentes de los Epics 1-6 (Feature 8.3).

**Fuera de alcance (explícitamente):**
- Cualquier cambio de Domain/Application/Infrastructure: este Epic es 100% `Kiwbi.Web` (vistas, layout, assets estáticos, y como mucho un endpoint de CSS dinámico si el enfoque elegido lo requiere para aplicar el branding por tenant).
- Rediseño de la lógica de negocio o de los flujos funcionales; solo se retocan la presentación y los estilos, no el comportamiento.
- Las vistas de Epics futuros aún no construidos (Epic 4-6 si no estuvieran implementados en el momento de ejecutar este Epic) se construirán directamente con el nuevo sistema una vez esté listo, sin necesidad de "migrarlas" después.

## Feature 8.1 - Investigación y selección de enfoque

Esta Feature es una tarea de investigación (spike) cuyo entregable es una decisión documentada (actualización de este Epic con la opción elegida y su justificación) antes de poder detallar el plan de acción de las Features 8.2 y 8.3.

### Opciones a evaluar

| Opción | Descripción | Ventajas | Inconvenientes |
| --- | --- | --- | --- |
| **Bootstrap 5 a medida** | Sobrescribir variables Sass/CSS custom properties del Bootstrap ya usado, sin cambiar de framework. | Cero curva de aprendizaje, cero cambios de build (hoy no hay pipeline de compilación de CSS), riesgo mínimo. | Visualmente sigue "pareciendo Bootstrap" salvo inversión notable en overrides; menos diferenciador. |
| **Plantilla open-source basada en Bootstrap** (p. ej. Tabler, CoreUI, AdminLTE) | Adoptar una plantilla de panel de administración ya diseñada y con licencia permisiva (MIT), adaptando su markup a las vistas Razor. | Resultado profesional rápido, sigue siendo Bootstrap por debajo (compatible con el stack actual). | Puede traer JS propio que colisione con HTMX/Alpine (a revisar); adaptar cada vista existente al nuevo markup. |
| **Migración a Tailwind CSS** | Sustituir Bootstrap por un framework utility-first. | Máximo control visual y consistencia a largo plazo. | Introduce un paso de build (Tailwind CLI/PostCSS) inexistente hoy; reescritura de clases en todas las vistas; mayor coste de mantenimiento. |
| **Generación asistida por IA** (v0.dev, Galileo AI, Uizard, Figma + plugins IA, etc.) | Usar herramientas de IA para generar mockups o código de referencia. | Rápido para explorar dirección visual/inspiración. | La mayoría genera React/Tailwind o mockups estáticos, no Razor/MVC directamente; requiere portar manualmente el resultado, no es una "solución final" por sí sola. |

### Consideración transversal a decidir en esta Feature

Cómo aplicar el *branding* por tenant (`PrimaryColor`/`SecondaryColor`/`LogoPath`) al tema visual elegido. Opciones típicas a valorar durante la investigación:
- Variables CSS (`:root { --kiwbi-primary: ...; }`) inyectadas en un `<style>` inline en `_Layout.cshtml`, calculadas desde `ICurrentUser`/`DeveloperCompany` en cada request.
- Un endpoint dedicado que sirva un `.css` dinámico por tenant.
- Limitar el branding dinámico a elementos concretos (logo, color de acento en botones principales) en vez de todo el tema, para reducir el riesgo de romper la legibilidad/contraste con colores arbitrarios elegidos por la promotora.

### Plan de acción de la Feature 8.1

- [ ] Evaluar las opciones de la tabla anterior contra los criterios: coherencia con el stack actual, coste de adopción, mantenibilidad y compatibilidad con HTMX/Alpine.js.
- [ ] Decidir el mecanismo de aplicación del branding por tenant al tema.
- [ ] Documentar la decisión final en este Epic (sustituyendo esta sección por la opción elegida y su justificación) antes de iniciar la Feature 8.2.

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
- Antes de programar este Epic, el usuario debe aprobar este documento, y en particular la decisión de la Feature 8.1 antes de empezar la 8.2.
