# Epic 09: Landing Page y Experiencia de Acceso (Auth)

## Estado

- Estado: **implementado y verificado end-to-end en navegador (2026-09-20)**. Continuación directa del Epic 8 (mismo sistema de diseño Kiwbi, mismo flujo brief → v0.dev → traducción a Bootstrap 5 real ya validado en ese Epic), aplicada a las pantallas que la Ronda 3 del Epic 8 dejó explícitamente "fuera de prioridad": Landing pública, Login, Registro y Aceptación de invitación de comprador.
- Depende de: ninguno de los Epics funcionales (1-6) a nivel de datos. Reutiliza el mecanismo de branding por tenant ya implementado en el Epic 8 (`GetBrandingForHousingUnitUseCase`) para la Aceptación de invitación.

## Contexto y objetivo

`HomeController.Index` es hoy literalmente la plantilla por defecto de ASP.NET Core (`Views/Home/Index.cshtml` sin contenido propio) — Kiwbi nunca ha tenido una Landing pública. `Account/Login`, `Account/Register` y las tres vistas de `OnboardingController` (`Accept`, `InvalidInvitation`, `Welcome`) usan el Bootstrap "de fábrica" sin ningún estilo del sistema de diseño ya aplicado al resto de la aplicación (Epic 8).

Objetivo: dar a estas 6 pantallas una identidad visual coherente con el resto de Kiwbi, resolviendo antes un problema que no existía en el Epic 8 — **estas pantallas se sirven antes de que se conozca ningún tenant** (Landing/Login/Registro) o, en el caso de la invitación, con un tenant que sí se conoce pero que no es el usuario autenticado todavía. Kiwbi necesita, por primera vez, una identidad de marca propia (no de ninguna promotora).

## Decisiones

1. **Color de marca propio de Kiwbi:** verde-teal profesional, independiente de cualquier tenant — `#0f766e` (teal-700) como primario y `#2dd4bf` (teal-400) como acento más claro. Nuevas variables CSS `--kiwbi-brand-primary`/`--kiwbi-brand-accent` en `kiwbi.css`, distintas de `--kiwbi-accent`/`--kiwbi-accent-2` (que siguen siendo exclusivamente el acento por tenant, resuelto por `TenantBrandingViewComponent`). Se usa en Landing y Login/Registro; la Aceptación de invitación usa en su lugar el acento de la promotora invitante (ver punto 3).
2. **CTA de la Landing:** un único camino de conversión, autoservicio — "Crear cuenta gratis" → `/Account/Register`. Sin flujo de pago/demo (decisión explícita del usuario, 2026-09-20: el MVP se presenta sin monetización todavía; se revisará con el comité tras validar el enfoque).
3. **Aceptación de invitación con acento del tenant invitante:** a diferencia de Landing/Login/Registro, en el momento en que el token de la invitación resuelve correctamente ya se conoce sin ambigüedad `HousingUnit → HousingPromotion → DeveloperCompany` — mismo dato que ya resuelve `GetBrandingForHousingUnitUseCase` (Epic 8) para el detalle de Vivienda del comprador. Se reutiliza tal cual esa use case desde `OnboardingController.Accept`, sin tocar Application/Domain. Si el token no resuelve (invitación inválida/no encontrada), no hay tenant que aplicar — pantalla neutra con la marca propia de Kiwbi.
4. **KPIs de la Landing:** cifras ilustrativas de ejemplo (mismo criterio que el resto de datos de muestra usados en el Epic 8), marcadas explícitamente como estimadas en la propia página.
5. **Agrupación de briefs:** 3 briefs en vez de uno por pantalla o uno único — Landing (contenido nuevo, necesita su propia ronda), Login+Registro (comparten exactamente la misma tarjeta centrada) y Aceptación de invitación (un único flujo de 3 estados). Generados en el mismo hilo/proyecto de v0.dev que el resto del Epic 8 (`C:\Users\cgran\Documents\identidad-visual-kiwbi`), y revisados los tres juntos en una sola sesión en vez de uno a uno, ya que el propio flujo ya establecido en el Epic 8 no exige revisar cada pantalla antes de generar la siguiente.

## Feature 9.1 — Landing pública

### Brief

```
Diseña la Landing Page pública de "Kiwbi", una aplicación B2B2C para promotoras
inmobiliarias que gestionan la personalización de viviendas en construcción
(piensa en ella como el sitio al que llega alguien desde un buscador o un
anuncio, ANTES de iniciar sesión - es la primera impresión de la marca).

Identidad visual: NO reutilices el sidebar del panel de administración (esto
no es una pantalla logueada). Usa un color de marca propio de Kiwbi (no el de
ningún tenant): verde-teal profesional (#0F766E primario, #2DD4BF como acento
más claro), sobre la misma base neutra fría tipo Slate (fondo #f8fafc/blanco,
texto #0f172a/#64748b), tipografía Inter. Tono: profesional, confiable,
moderno; ni "corporativo aburrido" ni "startup juguetona" - se manejan
decisiones de compra de vivienda, transmite seriedad sin ser fría.

Estructura de la página (de arriba a abajo):
1. Navbar simple: logo "Kiwbi" a la izquierda, enlaces "Iniciar sesión" y
   botón destacado "Crear cuenta gratis" a la derecha.
2. Hero: titular potente sobre el problema real que resuelve (gestionar a
   mano, por email/Excel, la personalización de acabados de un edificio en
   construcción con decenas de viviendas y varios gremios), subtítulo con la
   propuesta de valor, botón principal "Crear cuenta gratis" + enlace
   secundario "Ver cómo funciona" (ancla a la sección 4). Puedes acompañarlo
   de una ilustración/mockup simplificado del panel (no hace falta que sea
   pixel-perfect, es decorativo).
3. Franja de KPIs ilustrativos (3-4 cifras grandes con etiqueta corta, deja
   claro que son cifras de ejemplo/estimadas): p.ej. "-70% tiempo dedicado a
   gestionar personalizaciones a mano", "100% trazabilidad de elecciones y
   pagos por vivienda", "0 hojas de cálculo perdidas o desactualizadas".
4. "Cómo funciona" en dos columnas o dos pestañas (Promotora / Comprador),
   cada una con 3-4 pasos numerados y un icono:
   - Promotora: crea tu promoción y viviendas -> define gremios y
     personalizaciones con sus opciones -> invita a tus compradores por
     email (Magic Link) -> exporta el Libro de Obra listo para la
     constructora.
   - Comprador: recibe la invitación por email -> entra y ve su vivienda ->
     elige sus acabados antes de la fecha límite de cada gremio -> sigue el
     estado de sus elecciones y pagos.
5. Sección de beneficios/diferenciadores (3-4 tarjetas con icono): centraliza
   todo en un solo sitio (nada de Excel/email sueltos), cada promotora
   mantiene su propia marca (logo y colores) de cara a sus compradores,
   bloqueo automático por fecha límite de gremio (nadie decide tarde), y
   exportación lista para la constructora agrupada por gremio.
6. CTA final a todo lo ancho: repite "Crear cuenta gratis", con un
   subtítulo tranquilizador (gratuito, sin necesidad de tarjeta, en marcha en
   minutos).
7. Footer simple: logo, enlace a Iniciar sesión, y un par de enlaces
   decorativos (Privacidad, Contacto).

Genera un único mockup coherente de la página completa (puede ser largo,
es una landing con scroll).
```

### Resultado (2026-09-20)

Generado en el mismo proyecto/hilo de v0.dev, como `components/kiwbi/landing.tsx` (`Landing`). Muy fiel al brief, con las 7 secciones pedidas: `Navbar` (logo + Iniciar sesión + Crear cuenta gratis), `Hero` (titular + subtítulo + CTA doble + `PanelMockup` decorativo con datos de ejemplo de una promoción ficticia), `KpiStrip` (3 cifras con nota "cifras ilustrativas y estimadas"), `HowItWorks` (pestañas Promotora/Comprador con 4 pasos cada una, iconos `lucide-react`), `Benefits` (4 tarjetas), `FinalCta` (banda teal con degradado decorativo + 3 checks de refuerzo) y `Footer`. Confirma el color de marca propio decidido (`bg-teal-700`/`text-teal-700` en todo el componente, nunca el acento de ningún tenant).

Traducido a Razor real (no a `wwwroot/design-preview/`, a diferencia del Epic 8: aquí se implementó directamente sobre la vista real `Views/Home/Index.cshtml`, ya que el propio Epic 8 estableció que la Fase 2 de aplicación no necesita pasar por el PoC estático una vez el patrón ya está validado):
- Nuevo layout `Views/Shared/_LandingLayout.cshtml` (documento HTML completo, sin sidebar/topbar del panel — la propia vista aporta su navbar y footer).
- `Views/Home/Index.cshtml` reescrita con las 7 secciones, usando Bootstrap 5 (grid/flex utilities) + nuevas clases `kiwbi-pub-*` en `kiwbi.css`. Las pestañas Promotora/Comprador de "Cómo funciona" se resolvieron con un patrón `<input type="radio">` + `<label>` oculto (sin JS), ya que es un simple cambio de contenido visible, no una petición al servidor — coherente con "no SPA" y sin añadir JS nuevo a `kiwbi.js` para algo puramente decorativo.
- El botón "Crear cuenta gratis" enlaza a `Account/Register` real; "Iniciar sesión" a `Account/Login` real; "Ver cómo funciona" es un ancla `#como-funciona` en la misma página.

## Feature 9.2 — Login y Registro de Promotora

### Brief

```
Dirección: mismo color de marca propio de Kiwbi que en la Landing (teal
#0F766E/#2DD4BF sobre base Slate, Inter) - NO el sidebar del panel logueado,
esto es pre-login. Diseño de tarjeta centrada, minimalista, tipo SaaS
moderno: fondo suave (a doble color o con un patrón/gradiente sutil de marca
detrás), logo "Kiwbi" arriba de la tarjeta, tarjeta blanca con sombra suave
y radio 0.75rem.

Pantalla 1 - Login: campos Correo electrónico y Contraseña, checkbox
"Recordarme", botón principal "Iniciar sesión" a todo el ancho, enlace
"¿Todavía no tienes cuenta? Crea una gratis" hacia el Registro. Sin
"recuperar contraseña" todavía (no implementado, no lo incluyas como
funcional, puedes omitirlo).

Pantalla 2 - Registro: campos Nombre de la promotora, Correo electrónico,
Contraseña y Confirmar contraseña, botón principal "Crear cuenta gratis" a
todo el ancho, enlace "¿Ya tienes cuenta? Inicia sesión". El registro es
inmediato y de autoservicio (sin aprobación manual ni pago): puedes añadir
una nota breve tranquilizadora tipo "Empieza a usar Kiwbi en minutos, sin
tarjeta de crédito".

Genera un único mockup coherente para ambas pantallas (comparten la misma
tarjeta/layout, solo cambian los campos).
```

### Resultado (2026-09-20)

Generado como `components/kiwbi/auth.tsx` (`Auth`), con Login/Registro conmutados por un segmented-control de dos botones dentro de la propia tarjeta (estado de React) — igual patrón que Brief B/E/F del Epic 8 al conmutar listado/formulario. Muy fiel: fondo con degradado teal sutil + rejilla decorativa, tarjeta blanca con sombra, campos con icono (`Mail`/`Lock`/`Building2`), toggle de mostrar/ocultar contraseña, checkbox "Recordarme" solo en Login, nota de refuerzo "Empieza a usar Kiwbi en minutos, sin tarjeta de crédito" solo en Registro.

Traducido, con la misma decisión de arquitectura ya tomada varias veces en el Epic 8 (Kiwbi es no-SPA: dos páginas reales enlazadas, no un toggle de estado), a las vistas reales `Views/Account/Login.cshtml` y `Views/Account/Register.cshtml`:
- Nuevo layout `Views/Shared/_AuthLayout.cshtml` (tarjeta centrada + fondo decorativo con degradado teal + rejilla sutil, reutilizado también por las 3 vistas de la Feature 9.3), con un segmented-tab real (`<a>`, no un `<button>` de JS) entre "Iniciar sesión" y "Crear cuenta" en la cabecera de la tarjeta.
- Campos y validación (`asp-for`/`asp-validation-for`) sin cambios respecto a los `LoginViewModel`/`RegisterViewModel` ya existentes — solo cambia el marcado/clases, ninguna lógica de `AccountController`.
- Sin el toggle de mostrar/ocultar contraseña del mockup (exigiría JS nuevo para un beneficio menor); se deja como posible mejora futura, no bloqueante.

## Feature 9.3 — Aceptación de invitación de comprador

### Brief

```
Dirección: mismo layout minimalista de tarjeta centrada que Login/Registro,
PERO aquí SÍ aplica el acento de marca de la promotora concreta que invitó a
este comprador (no el verde-teal de Kiwbi) - es el mismo mecanismo ya usado
en el Detalle de Vivienda del comprador, la promotora ya se conoce en cuanto
se resuelve el token de la invitación.

Tres estados de una misma pantalla (genera los tres):

1) Invitación válida (formulario a rellenar): cabecera con el nombre de la
   promotora (y su logo si lo tiene) + un resumen de la vivienda a la que
   se está vinculando (Promoción, Planta, Puerta) + el correo electrónico ya
   fijado (de solo lectura, no editable). Campos Contraseña y Confirmar
   contraseña, botón principal "Aceptar invitación y crear mi cuenta".

2) Invitación inválida/caducada (sin formulario): mensaje claro de que el
   enlace ya no es válido (caducado, cancelado, o ya usado), sin ningún dato
   de la vivienda (el token no se pudo resolver), con un enlace de vuelta a
   la home pública de Kiwbi.

3) Bienvenida (tras aceptar con éxito): mensaje de confirmación con el
   nombre de la promotora, y un botón para ir a iniciar sesión / a su panel
   de comprador.

Genera un único mockup coherente que muestre los tres estados (puede ser una
sola composición con las tres variantes, o tres pantallas pequeñas juntas).
```

### Resultado (2026-09-20)

Generado como `components/kiwbi/accept-invite.tsx` (`AcceptInvite`), con un selector de estado de demo (Válida/Caducada/Bienvenida) y los tres estados pedidos: `ValidCard` (marca de la promotora + resumen de vivienda + email de solo lectura + contraseña/confirmar), `InvalidCard` (icono de aviso, sin ningún dato de vivienda, backdrop sin teñir de marca) y `WelcomeCard` (icono de celebración + nombre de la promotora + CTA). El backdrop decorativo se tiñe con el color de la promotora (`tenant.primary`) solo cuando el token resuelve (`tinted = state !== 'invalid'`), exactamente la regla ya decidida.

Traducido a las 3 vistas reales, reutilizando `_AuthLayout.cshtml` de la Feature 9.2 con un parámetro de tinte dinámico:
- `Views/Onboarding/Accept.cshtml`: la rama `CanAccept` reescrita con la cabecera de marca (nombre + logo/iniciales de la promotora), resumen de vivienda y formulario con contraseña/confirmar ya existentes (`asp-for`/`asp-validation-for`, sin tocar `AcceptInvitationViewModel`). Las 3 ramas de "no se puede aceptar" (`Cancelled`/`Accepted`/`IsExpired`) siguen en la misma vista (así estaba ya en el código real, distinto del mockup que solo modela un estado "inválida" genérico) pero con el estilo de tarjeta de aviso ya usado en el resto del Epic.
- `Views/Onboarding/InvalidInvitation.cshtml`: reescrita con la tarjeta de aviso neutra (sin marca de ningún tenant, ya que el token no resolvió) y el enlace de vuelta a la Landing.
- `Views/Onboarding/Welcome.cshtml`: reescrita con la tarjeta de celebración; requiere conocer el nombre de la promotora aunque ya no haya invitación que consultar (la petición POST ya se resolvió) — se pasa vía `TempData` desde `OnboardingController.Accept` (POST) tras un `RedirectToAction(nameof(Welcome))`, sin necesidad de volver a resolver el token.
- **Cambio en `OnboardingController`:** se inyecta `GetBrandingForHousingUnitUseCase` (ya existente, Epic 8) para resolver el `DeveloperCompanyId`/nombre/color de la promotora invitante a partir de `invitation.HousingUnitId`, tanto en el `GET Accept` (para pintar la cabecera y teñir el backdrop) como para propagar el nombre a `Welcome` vía `TempData` tras el `POST` con éxito. Ningún cambio en Application/Domain: se reutiliza la use case tal cual.

## Implementación (Kiwbi.Web)

- [x] Variables `--kiwbi-brand-primary`/`--kiwbi-brand-accent` + clases `.kiwbi-pub-*` (Landing + Auth) añadidas a `wwwroot/css/kiwbi.css`.
- [x] `Views/Shared/_LandingLayout.cshtml` y `Views/Shared/_AuthLayout.cshtml` creados.
- [x] `Views/Home/Index.cshtml` reescrita con el contenido de la Landing.
- [x] `Views/Account/Login.cshtml` y `Views/Account/Register.cshtml` reescritas con `_AuthLayout`.
- [x] `Views/Onboarding/Accept.cshtml`, `InvalidInvitation.cshtml` y `Welcome.cshtml` reescritas con `_AuthLayout` y acento del tenant invitante.
- [x] `OnboardingController` ampliado con `GetBrandingForHousingUnitUseCase` para resolver la marca de la promotora invitante (GET/POST `Accept`) y propagarla (nombre + color) a `Welcome` vía `TempData`.
- [x] Build limpio (`dotnet build`) sin errores nuevos.
- [x] Verificación visual end-to-end en navegador (`dotnet watch run` + capturas), con datos reales (registro de una promotora nueva, branding Terracota `#C05621`, promoción/vivienda/invitación reales):
  - **Landing** (`/`): hero, panel decorativo, KPIs, pestañas Promotora/Comprador (toggle CSS-only por `<input type="radio">`, sin JS, verificado interactivamente), beneficios, CTA y footer — todo con el acento propio de Kiwbi (teal), coincide con el mockup.
  - **Login**/**Registro**: tarjeta centrada con el mismo acento teal, tabs de navegación entre ambas pantallas, registro real completado con éxito (redirige a `/DeveloperProfile`).
  - **Aceptación de invitación — válida**: cabecera con nombre/iniciales de la promotora invitante y el resumen de vivienda real (Promoción/Planta/Puerta), backdrop y marca teñidos con el color de marca real de la promotora (`#C05621`), aceptación completa (alta de la cuenta compradora real + `SignIn`).
  - **Aceptación de invitación — ya aceptada**: mismo tenant conocido, mensaje correcto con enlace a Login.
  - **Aceptación de invitación — token inválido** (`InvalidInvitation`): sin ningún tenant, backdrop neutro con el teal de Kiwbi.
  - **Bienvenida**: nombre de la promotora y color de marca propagados correctamente vía `TempData` desde el POST de aceptación.
- **Defecto encontrado y corregido durante la verificación:** el color de marca de la promotora (`--kiwbi-pub-glow`) se fijaba originalmente como estilo inline en `.kiwbi-pub-auth-backdrop`, un div hermano del contenido de la tarjeta — al no ser antepasado común, la marca/ícono de la tarjeta (`.kiwbi-pub-invite-mark`) no heredaba la variable y siempre caía al teal por defecto. Corregido moviendo el estilo inline al elemento `<main class="kiwbi-pub-auth-shell">` (antepasado real de ambos), sin cambios de marcado en las vistas. Verificado visualmente tras la corrección.

## Consideraciones de Testing y Notas de la IA

- Epic sin lógica de negocio nueva (salvo la lectura de branding ya existente reutilizada tal cual); no se esperan tests de Domain/Application adicionales. `OnboardingController` sigue sin tests unitarios propios (no los tenía antes de este Epic).
- Mantener la separación Español (vistas/mensajes) / Inglés (código) ya establecida en el resto del proyecto.
- Al arrancar la aplicación en depuración, la ruta por defecto (`{controller=Home}/{action=Index}`) ya lleva a la Landing sin redirecciones adicionales, para cualquier usuario (autenticado o no) — cumple el requisito de "aterrizar siempre en la Landing" sin cambios de enrutamiento.
