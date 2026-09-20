# Epic 11: Cierre de Circuitos Pendientes, Calidad y Publicación del PoC

## Estado

- Estado: **documento validado por el usuario (2026-09-21). Las 2 preguntas de negocio abiertas están resueltas** (ver Decisiones). Repositorio confirmado **público**. Frente A (11.2/11.3) y la parte de 11.8/11.9 que no depende de cuentas externas ya están en implementación por el agente; Frente B (11.4-11.7) lo gestiona el usuario directamente creando las cuentas de los servicios cloud con los prompts de este documento.
- Origen: continuación directa del análisis crítico de estado del proyecto (2026-09-20/21) sobre los hallazgos 3.1-3.11 detectados al revisar qué falta para considerar Kiwbi un PoC/MVP presentable. Sustituye al "Epic 9 de cierre de MVP" mencionado en una sesión anterior (2026-09-18) que quedó huérfano al renumerarse Epic 9 como "Landing y Auth" — este documento retoma y cierra formalmente ese hueco (hallazgo 3.11).
- Depende de: ninguno de los Epics funcionales a nivel de datos (igual que Epics 7-9). Reutiliza `IEmailSender`/`IFileStorageService` (puertos ya existentes desde Epic 3/2) para las Features 11.4/11.5 sin tocar su forma, solo su implementación en `Kiwbi.Infrastructure`.

## Contexto

Tras el cierre del Epic 10, se hizo una auditoría completa de la aplicación (Controllers, Application, Domain, Infrastructure, tests) para valorar qué falta para pasar de "funciona en la máquina de desarrollo" a "se puede enseñar/entregar como PoC real". Se identificaron 11 puntos (numerados 3.1-3.11 durante el análisis). El usuario ha resuelto cada uno; este documento formaliza esas decisiones en Features planificables, agrupadas en dos frentes de trabajo independientes:

- **Frente A — Correcciones y flujos que faltan** (Features 11.1-11.3): cambios de código acotados a `Kiwbi.Web`/`Kiwbi.Application`, sin infraestructura nueva.
- **Frente B — Publicación real del PoC** (Features 11.4-11.9): llevar la app a un entorno público gratuito con envío real de correo, más la automatización (CI + calidad de código) alrededor de GitHub.

## Decisiones confirmadas por el usuario

1. **Prioridad absoluta: coste $0.** Sin necesidad de SLA, escalabilidad, trazas ni garantías — es un entorno público únicamente para revisar el PoC.
2. **Auditoría de acciones manuales (Confirmar/Pagar): descartada** (hallazgo 3.5). Es un PoC, no un MVP con múltiples usuarios por tenant todavía.
3. **Avisos/recordatorios (cutoff próximo, elección confirmada): descartados** (hallazgo 3.6). Esto simplifica 11.5: solo necesitamos que funcione el correo de invitación (Magic Link), ninguna plantilla adicional.
4. **Validación de contraste del `BrandColor`: descartada** (hallazgo 3.9).
5. **Multi-usuario por tenant: no se implementa, queda como futurible** (hallazgo 3.7) — anotado en la sección "Backlog" al final de este documento, sin Feature asociada.
6. **Stack de publicación aceptado:** Render (hosting) + Neon (Postgres) + Cloudflare R2 (ficheros) + Brevo (email) — todos con plan gratuito permanente, sin necesidad de dominio propio (remitente genérico).
7. **Cuentas de los servicios de la nube:** las crea el usuario directamente (no el agente), guiándose con los prompts de la sección correspondiente de cada Feature.
8. **Menú global:** reducido a "Promociones" + "Mi promotora", sin "Ajustes" (hallazgo 3.1) — **ya implementado** (Feature 11.1, ver más abajo), fuera del resto del plan.
9. **Feature 11.3 (2026-09-21):** no se bloquea la desvinculación de un comprador en ningún caso. Si la vivienda ya tiene elecciones `Confirmed`/`Paid`, se muestra un mensaje de confirmación explícito advirtiendo de ello ("esta vivienda ya tiene gestiones de personalización confirmadas/pagadas para este comprador, ¿seguro que quieres desvincularlo igualmente?"); si el usuario confirma, se procede igual. Sin mensaje especial si no hay ninguna elección en esos estados (confirmación genérica, igual que el resto de acciones destructivas de la app).
10. **Feature 11.9 (2026-09-21):** repositorio confirmado **público** → se activan las dos capas: Capa 1 (analizadores Roslyn + `dotnet format`) y Capa 2 (SonarCloud, gratis sin límite de líneas al ser público) + adicionalmente **CodeQL** nativo de GitHub (escaneo de seguridad, gratis para repos públicos, sin cuenta externa ni secreto — se activa solo con un workflow, cero fricción). Ver "Recomendación" en la Feature 11.9 más abajo.

## Frente A — Correcciones y flujos que faltan

### Feature 11.1 — Menú global simplificado ✅ IMPLEMENTADA (2026-09-21)

Resuelve el hallazgo 3.1. El nav global (`KiwbiSidebarViewComponent`, modo no-contextual) mostraba "Panel" / "Mi promotora" / "Ajustes" (este último un `href="#"` sin pantalla real detrás desde el Epic 8, Ronda 5). Se ha simplificado a solo dos puntos:

- [x] `Views/Shared/Components/KiwbiSidebar/Default.cshtml`: eliminado el ítem "Ajustes"; el antiguo "Panel" renombrado a "Promociones" (es literalmente el listado de `HousingPromotionsController.Index`, la etiqueta "Panel" era ambigua).
- [x] `Views/HousingPromotions/Index.cshtml`: breadcrumb `Panel` → `Promociones` para que coincida con el nuevo nombre del nav.
- [x] Build limpio, sin errores.
- [ ] Pendiente: verificación visual en `dotnet watch` (rápida, se puede hacer junto con cualquier otra Feature de este Epic que requiera levantar la app).

### Feature 11.2 — Recuperación de contraseña (Promotora y Comprador) ✅ IMPLEMENTADA (2026-09-21)

Resuelve el hallazgo 3.2. Sin esto, un usuario que olvida su contraseña (fijada una única vez al registrarse o al aceptar el Magic Link) se queda sin ninguna vía de acceso.

**Objetivo:** flujo estándar de ASP.NET Core Identity — "¿Olvidaste tu contraseña?" en `Login.cshtml` → formulario de email → token de reseteo (`UserManager.GeneratePasswordResetTokenAsync`) enviado por `IEmailSender` (mismo puerto que ya usa Epic 4, aprovecha directamente la Feature 11.5 de este mismo Epic) → pantalla de nueva contraseña → `UserManager.ResetPasswordAsync`.

**Decisiones tomadas:**
- Un único flujo en `AccountController`, sin distinción de rol — `ForgotPassword`/`ResetPassword` de Identity no distinguen Promotora/Comprador, cualquier `ApplicationUser` pasa por el mismo camino.
- Mensaje siempre genérico en `ForgotPasswordConfirmation` ("si existe una cuenta con ese correo...") independientemente de si el email existe o no — evita enumeración de usuarios (OWASP).

**Implementación:** `IAuthenticationService` (`Kiwbi.Application.Developers`) ampliado con `GeneratePasswordResetTokenAsync`/`ResetPasswordAsync`, implementados en `IdentityAuthenticationService` (inyecta ahora también `UserManager<ApplicationUser>`, antes solo tenía `SignInManager`). `IEmailSender` ampliado con `SendPasswordResetEmailAsync` (implementado en `LoggingBuyerInvitationEmailSender` como stub de log, igual que la invitación de comprador, hasta que la Feature 11.5 traiga el envío real). Nuevas use cases `ForgotPasswordUseCase`/`ResetPasswordUseCase` (`Kiwbi.Application.Developers`). `AccountController` ampliado con `ForgotPassword`/`ForgotPasswordConfirmation`/`ResetPassword`/`ResetPasswordConfirmation` (GET/POST), vistas nuevas con `_AuthLayout` (mismo patrón visual del Epic 9), enlace "¿Olvidaste tu contraseña?" añadido a `Login.cshtml`.

**Checklist:**
- [x] Diseñar y aprobar las dos decisiones de arriba.
- [x] `ForgotPassword`/`ForgotPasswordConfirmation`/`ResetPassword`/`ResetPasswordConfirmation` en `AccountController` + vistas con `_AuthLayout`.
- [x] Plantilla de email de reseteo vía `IEmailSender` (stub de log en `Development`, real cuando llegue la Feature 11.5).
- [x] Build limpio, `dotnet format --verify-no-changes` limpio, 324 tests pasando (sin tests nuevos dedicados — flujo de Identity estándar, mismo criterio que Login/Logout, que tampoco tienen tests propios).
- [ ] Pendiente: verificación manual en navegador (solicitar el enlace, capturar el token del log de `dotnet watch` como ya se hace con las invitaciones, completar el reseteo).

### Feature 11.3 — Desvincular/reasignar comprador de una vivienda ✅ IMPLEMENTADA (2026-09-21)

Resuelve el hallazgo 3.4. Hoy `HousingUnitBuyer` es permanente: si una compraventa cae o la vivienda se revende, no hay forma de romper el vínculo ni de invitar a un nuevo comprador a esa misma vivienda.

**Objetivo:** nuevo use case (`Kiwbi.Application.Onboarding`, ej. `UnlinkHousingUnitBuyerUseCase`) que elimina el `HousingUnitBuyer` existente. Las `HomeCustomizationChoice` ya hechas por ese comprador se mantienen tal cual en base de datos (no se borran) pero dejan de ser accesibles para el comprador desvinculado; si luego se invita a otro comprador a la misma vivienda, empezaría "desde cero" viendo el estado real ya guardado (útil de cara al Libro de Obra, que no depende del comprador sino de la vivienda).

**Decisión (2026-09-21):** no se bloquea nunca la desvinculación. El Web comprueba si la vivienda tiene alguna elección `Confirmed`/`Paid` (reutilizando el mismo cálculo que ya usa `HousingPromotionChoicesController`) y, si es así, el `onsubmit="return confirm(...)"` del botón "Desvincular" muestra un texto de advertencia distinto ("esta vivienda ya tiene gestiones de personalización confirmadas/pagadas para este comprador, ¿seguro que quieres desvincularlo igualmente?") en vez del genérico "¿Seguro que quieres desvincular a este comprador?" — mismo patrón ya usado en toda la app para acciones destructivas (JS `confirm()`, sin página de confirmación aparte).

**Implementación:** `HousingUnitBuyerDto` ampliado con `HasConfirmedOrPaidChoices` (calculado en `GetHousingUnitBuyersForHousingUnitUseCase`, que ahora también inyecta `IHomeCustomizationChoiceRepository`). Nueva use case `UnlinkHousingUnitBuyerUseCase` (`Kiwbi.Application.Onboarding.UnlinkHousingUnitBuyer`), mismo patrón de verificación de tenant que `DeleteHousingUnitUseCase` (carga `HousingUnitBuyer` → `HousingUnit` → `HousingPromotion` → compara `DeveloperCompanyId`). `HousingUnitsController` ampliado con la acción `UnlinkBuyer` (POST). `Invitations.cshtml`: nueva columna "Acciones" en la tabla "Compradores vinculados" con el botón "Desvincular" y el mensaje de confirmación condicional.

**Checklist:**
- [x] Decidir la pregunta de negocio (no se bloquea, solo advertencia condicional).
- [x] `IHousingUnitBuyerRepository` + use case + Web (botón "Desvincular" en `Invitations.cshtml`, tabla "Compradores vinculados", con el `confirm()` condicional).
- [x] Tests de Application (`UnlinkHousingUnitBuyerUseCaseTests` + ampliación de `GetHousingUnitBuyersForHousingUnitUseCaseTests` para `HasConfirmedOrPaidChoices`). 324 tests pasando, build y `dotnet format --verify-no-changes` limpios.
- [ ] Pendiente: verificación manual en navegador (desvincular un comprador real de la vivienda seedeada por el demo seeder y confirmar el mensaje de advertencia condicional).

## Frente B — Publicación real del PoC (gratis)

### Tabla resumen: qué credencial va dónde

| Credencial | Dónde vive | Por qué no en GitHub Secrets |
|---|---|---|
| Connection string de Neon | Variable de entorno de Render (`ConnectionStrings__DefaultConnection`) | Solo la app en ejecución la necesita, no el pipeline de CI |
| Access Key/Secret de R2 + bucket | Variables de entorno de Render | Ídem |
| Credenciales SMTP de Brevo | Variables de entorno de Render | Ídem |
| Ninguna (el workflow de CI no llama a servicios externos) | — | El único secreto de GitHub que podríamos llegar a necesitar es un *deploy hook* de Render, y ni eso: Render se autodespliega vigilando el repo, sin que GitHub necesite saber nada de Render |

### Feature 11.4 — Almacenamiento de ficheros en la nube (Cloudflare R2)

Resuelve el hallazgo 3.8. Hoy `LocalFileStorageService` (`Kiwbi.Infrastructure`) escribe en disco local (`wwwroot/uploads`) — en Render (y en la inmensa mayoría de PaaS gratuitos) el sistema de archivos del contenedor **no persiste** entre despliegues/reinicios, así que cualquier plano/logo subido desaparecería. Se sustituye por un nuevo adaptador `S3FileStorageService` (mismo puerto `IFileStorageService`, cero cambios en Application/Domain/Controllers) contra Cloudflare R2 (API compatible con S3). Esto también resuelve de raíz el logo de marca (hoy un campo de texto/URL): al tener ya subida real de fichero para planos, extender `DeveloperProfileController`/`UpdateDeveloperBrandingCommand` para aceptar un fichero real es una extensión pequeña sobre el mismo mecanismo, no un problema aparte.

**Prompt para pedir la guía a otra IA:**
```
Necesito una guía paso a paso, para alguien sin experiencia previa con
Cloudflare, para configurar Cloudflare R2 como almacenamiento de ficheros
para una aplicación ASP.NET Core (.NET 10) que hoy guarda imágenes
(planos de vivienda, logos) en disco local y necesita migrar a
almacenamiento de objetos porque se va a desplegar en un contenedor sin
disco persistente (Render.com, plan gratuito).

Necesito que la guía cubra:
1. Cómo crear una cuenta gratuita de Cloudflare (si no la tengo) y activar R2.
2. Cómo crear un bucket R2 (nombre sugerido: "kiwbi-uploads" o similar).
3. Cómo generar un "API Token" con permisos de lectura/escritura (S3 API
   token), y qué datos exactos obtengo al final: Account ID, Access Key ID,
   Secret Access Key, y el endpoint S3 (algo como
   https://<account-id>.r2.cloudflarestorage.com).
4. Cómo hacer el bucket accesible públicamente para servir las imágenes
   (dominio r2.dev de desarrollo, sin necesidad de dominio propio) y qué
   URL exacta usaría para acceder a un fichero subido.
5. Confirmar los límites del plan gratuito actual (almacenamiento y
   operaciones/mes) para asegurarme de que una app pequeña de demo
   (decenas de imágenes, tráfico bajo) se mantiene siempre en $0.
6. IMPORTANTE: no necesito código, ya lo implemento yo con el SDK de AWS
   S3 para .NET (AWSSDK.S3) apuntando al endpoint de R2 - solo necesito
   la guía de la consola de Cloudflare paso a paso con capturas o
   descripciones muy concretas de cada botón/menú, y la lista final de
   credenciales/valores que debo anotar para pasárselos después a otra
   herramienta (Render) como variables de entorno.

Al final de la guía, dame un resumen en una tabla con exactamente qué
valor va en cada campo (Account ID / Access Key ID / Secret Access Key /
Endpoint / Nombre del bucket / URL pública).
```

**Checklist:**
- [ ] Usuario ejecuta el prompt de arriba y crea la cuenta/bucket/token.
- [ ] Usuario comparte las credenciales (Account ID, Access Key ID, Secret Access Key, endpoint, bucket, URL pública) — se configurarán como variables de entorno de Render, nunca como secreto de GitHub ni committeadas en `appsettings.json`.
- [ ] Implementar `S3FileStorageService` (`Kiwbi.Infrastructure`, paquete `AWSSDK.S3`), registrado en `Production` vía configuración; mantener `LocalFileStorageService` para `Development`.
- [ ] Extender `UpdateDeveloperBrandingCommand`/`DeveloperProfileController` a subida real de fichero para el logo.
- [ ] Tests de Application (mock de `IFileStorageService`, sin llamar a R2 real).

### Feature 11.5 — Envío real de correo (Brevo)

Resuelve el hallazgo 3.3 (parte de correo). Sustituye `LoggingBuyerInvitationEmailSender` por un `SmtpEmailSender : IEmailSender` real, solo en `Production` (se mantiene el adaptador de logging en `Development`, sigue siendo útil para no gastar cuota de envíos reales durante el desarrollo). Dado que 3.6 descarta avisos/recordatorios, el único correo real que necesitamos es el de invitación (Magic Link) ya existente — ninguna plantilla nueva.

**Prompt para pedir la guía a otra IA:**
```
Necesito una guía paso a paso, para alguien sin experiencia previa con
Brevo (antes Sendinblue), para configurar el envío de correos
transaccionales reales (no marketing) desde una aplicación ASP.NET Core
(.NET 10) vía SMTP, para un proyecto de demo/PoC sin presupuesto (el plan
gratuito de Brevo, 300 emails/día, es suficiente).

Necesito que la guía cubra:
1. Cómo crear una cuenta gratuita de Brevo.
2. Si hace falta verificar un remitente/dominio para poder enviar
   correos sin que caigan en spam, y qué pasa si NO tengo un dominio
   propio (¿puedo usar un remitente genérico de prueba, o Brevo obliga a
   verificar al menos un email remitente concreto aunque no sea un
   dominio completo?).
3. Cómo obtener las credenciales SMTP exactas: servidor SMTP, puerto,
   usuario (login SMTP) y contraseña/clave SMTP (no la contraseña de la
   cuenta, sino la clave SMTP específica que genera Brevo).
4. Confirmar los límites reales del plan gratuito actual (correos/día,
   si caduca la cuenta por inactividad, si requiere tarjeta de crédito
   para el alta).
5. IMPORTANTE: no necesito código, ya lo implemento yo en C# con
   MailKit/SmtpClient contra esas credenciales SMTP - solo necesito la
   guía de la consola de Brevo paso a paso y la lista final exacta de
   valores que debo anotar.

Al final, dame un resumen en una tabla con: Servidor SMTP / Puerto /
Usuario SMTP / Clave SMTP / Remitente autorizado a usar.
```

**Checklist:**
- [ ] Usuario ejecuta el prompt y crea la cuenta/credenciales SMTP.
- [ ] Usuario comparte los valores (van a variables de entorno de Render, nunca a `appsettings.json` ni a GitHub).
- [ ] Implementar `SmtpEmailSender` (paquete `MailKit` o `System.Net.Mail.SmtpClient`), registrado condicionalmente por entorno en `Kiwbi.Infrastructure`/`DependencyInjection.cs`.
- [ ] Verificación manual: invitar a un comprador real (email propio del usuario) desde el entorno desplegado y confirmar que llega el correo.

### Feature 11.6 — Base de datos gestionada (Neon Postgres)

Resuelve la mitad de datos de 3.10. Sustituye el Postgres local de `docker-compose.yml` (solo para desarrollo) por un proyecto Neon para el entorno publicado.

**Prompt para pedir la guía a otra IA:**
```
Necesito una guía paso a paso, para alguien sin experiencia previa con
Neon (Postgres serverless), para crear una base de datos PostgreSQL
gratuita para desplegar una aplicación ASP.NET Core (.NET 10 + EF Core +
Npgsql) de demo/PoC sin presupuesto.

Necesito que la guía cubra:
1. Cómo crear una cuenta gratuita de Neon y un proyecto nuevo Postgres.
2. Cómo obtener la cadena de conexión completa (host, puerto, nombre de
   base de datos, usuario, contraseña) y si Neon exige algún parámetro
   especial en la cadena de conexión (por ejemplo `sslmode=require` o
   similar) para que Npgsql conecte correctamente.
3. Confirmar los límites reales del plan gratuito (almacenamiento,
   cómputo, qué pasa si el proyecto está inactivo mucho tiempo - se
   "autosuspende" y tarda unos segundos en despertar en la siguiente
   conexión, ¿es así?, ¿caduca el proyecto entero en algún momento o
   solo se autosuspende sin borrarse?).
4. Si hace falta abrir el acceso a IPs concretas o si por defecto acepta
   conexiones desde cualquier origen (necesito que Render, un servicio
   externo, pueda conectarse).
5. IMPORTANTE: no necesito código, las migraciones de EF Core ya las
   aplico yo automáticamente al arrancar la aplicación - solo necesito
   la guía de la consola de Neon y la cadena de conexión final exacta
   que debo usar.

Al final, dame la cadena de conexión de ejemplo con placeholders claros
para cada parte (host/puerto/db/usuario/contraseña/parámetros extra).
```

**Checklist:**
- [ ] Usuario ejecuta el prompt y crea el proyecto Neon.
- [ ] Usuario comparte la cadena de conexión (va a la variable de entorno de Render `ConnectionStrings__DefaultConnection`, nunca a `appsettings.json`).
- [ ] Confirmar que `Program.cs` aplica migraciones al arrancar (`dbContext.Database.Migrate()`) para no depender de un paso manual.

### Feature 11.7 — Publicación de la aplicación (Render + Dockerfile + auto-deploy desde GitHub)

Resuelve la otra mitad de 3.10. Hoy no existe ningún `Dockerfile` en el repo — hace falta crearlo antes de poder desplegar en Render.

**Prompt para pedir la guía a otra IA:**
```
Necesito una guía paso a paso, para alguien sin experiencia previa con
Render.com, para desplegar gratis una aplicación ASP.NET Core (.NET 10)
contenedorizada con Docker, con base de datos y demás servicios externos
ya alojados en otros proveedores (no necesito la base de datos de
Render). El repositorio está en GitHub y quiero que cada `git push` a la
rama principal despliegue automáticamente sin pasos manuales.

Necesito que la guía cubra:
1. Cómo crear una cuenta gratuita de Render.
2. Cómo conectar mi repositorio de GitHub (indica si el repo es privado
   qué permisos exactos hay que dar a la GitHub App de Render).
3. Cómo crear un "Web Service" nuevo eligiendo "Docker" como entorno de
   ejecución, apuntando a un Dockerfile en la raíz del repo (o en una
   subcarpeta, indícame cómo se configura la ruta si el Dockerfile no
   está en la raíz).
4. Cómo configurar variables de entorno/secretos en el panel de Render
   (necesito varias: cadena de conexión de base de datos, credenciales
   de un bucket S3-compatible, credenciales SMTP, y el propio puerto que
   debe escuchar la app).
5. Confirmar que el auto-deploy en cada push a la rama principal está
   activado por defecto (o cómo activarlo).
6. Explicar el comportamiento del plan gratuito: ¿el servicio "se
   duerme" tras inactividad?, ¿cuánto tarda en responder la primera
   petición tras dormir?, ¿hay límite de horas/mes?
7. Cómo ver los logs de la aplicación en ejecución desde el panel, para
   diagnosticar errores de arranque.
8. IMPORTANTE: no necesito que me escribas el Dockerfile, ya lo hago yo
   - solo necesito la guía de la consola de Render paso a paso y la
   lista final de variables de entorno que tengo que rellenar (nombres
   de variable esperados según cómo yo configure la app, te los paso yo
   si hace falta ajustarlos).

Al final, dame un resumen con: pasos de alta, nombre exacto del menú
donde se configuran variables de entorno, y cómo forzar un redeploy
manual si hiciera falta.
```

**Checklist:**
- [ ] Crear `Dockerfile` multi-stage para `Kiwbi.Web` (build SDK + runtime ASP.NET Core).
- [ ] Usuario ejecuta el prompt y crea el Web Service en Render, conectado al repo.
- [ ] Configurar variables de entorno en Render (connection string de Neon, credenciales R2, credenciales Brevo, `ASPNETCORE_ENVIRONMENT=Production`).
- [ ] Verificar auto-deploy: un push a `main` dispara un despliegue visible en el panel de Render.
- [ ] Smoke test manual del entorno público: login, crear promoción, subir un plano, invitar a un comprador con email real.

### Feature 11.8 — Integración continua (GitHub Actions: build + test)

Resuelve la parte de automatización de calidad ya presente en el roadmap. Workflow `.github/workflows/ci.yml`: en cada push/PR contra `main`, `dotnet restore` + `dotnet build` + `dotnet test` (los 320 tests existentes) como *check* obligatorio antes de fusionar. No necesita ningún secreto — los tests actuales no tocan servicios externos reales (mockean repos/servicios o usan SQLite en memoria).

**No requiere prompt externo** — es configuración de repositorio pura, la implemento yo directamente.

**Checklist:**
- [x] `.github/workflows/ci.yml` (restore/build/test + `dotnet format --verify-no-changes` en cada push/PR contra `main`). Verificado localmente: `dotnet format --verify-no-changes` pasa limpio hoy (0 cambios), `dotnet build` sin warnings.
- [ ] Marcar el check como obligatorio en la protección de la rama `main` — requiere una acción tuya en GitHub (Settings → Branches → Branch protection rules → Require status checks to pass → seleccionar el job `build-and-test`), te aviso cuando el primer workflow haya corrido al menos una vez (GitHub solo deja elegir checks que ya se hayan ejecutado alguna vez).

### Feature 11.9 — Calidad de código automatizada

**Decisión y recomendación (2026-09-21):** repositorio público confirmado → recomiendo activar las **tres** capas, las tres 100% gratis sin límite al ser público, sin solaparse entre sí (cada una cubre un ángulo distinto):

- **Capa 1 (implementada, sin cuentas externas):** `EnableNETAnalyzers=true` explícito vía `Directory.Build.props` (ya era el default del SDK, se deja explícito) + `dotnet format --verify-no-changes` como *gate* real en `ci.yml`. Se probó subir `AnalysisLevel` a `latest-all` y se descartó: ~2.000 warnings, mayoritariamente `CA2007` (`ConfigureAwait`, una regla que Microsoft mismo dice que no aplica a apps ASP.NET Core) — puro ruido sin presupuesto para triarlo, así que se mantiene el nivel de análisis por defecto del SDK (0 warnings hoy).
- **Capa 2 (implementada, pendiente de que crees la cuenta):** SonarCloud vía `dotnet-sonarscanner` (workflow separado `sonarcloud.yml`, no bloquea merges) — detecta duplicación, code smells reales y cobertura de tests, gratis sin límite de líneas al ser público. Preferido sobre la action genérica `sonarsource/sonarcloud-github-action` porque `dotnet-sonarscanner` es el enfoque oficial recomendado para soluciones .NET con varios `.csproj`.
- **Capa 3 (implementada, nueva, cero fricción):** CodeQL nativo de GitHub (`codeql.yml`) — escaneo de seguridad (inyección SQL, XSS, secretos hardcodeados) gratis para repos públicos, sin cuenta externa ni secreto, ya funcionará desde el primer push. Complementa a SonarCloud (seguridad vs. mantenibilidad/code smells) en vez de solapar.

Esta combinación es la recomendación final: máxima cobertura de "buenas prácticas" posible a coste $0 dado que el repo es público, sin necesitar más esfuerzo de mantenimiento que revisar los resultados de vez en cuando (ninguna de las tres bloquea el desarrollo salvo el format-check, que ya pasa limpio).

**Prompt para pedir la guía a otra IA (solo si se decide activar la Capa 2):**
```
Necesito una guía paso a paso, para alguien sin experiencia previa con
SonarCloud, para añadir análisis estático de calidad de código (code
smells, duplicación, cobertura) a un repositorio de GitHub con una
solución .NET 10 (ASP.NET Core + xUnit), usando GitHub Actions.

Necesito que la guía cubra:
1. Cómo crear una cuenta gratuita de SonarCloud enlazada a mi cuenta de
   GitHub (login con GitHub).
2. Cómo importar mi organización/repositorio de GitHub en SonarCloud
   (mi repositorio es PÚBLICO - confírmame que el plan gratuito no tiene
   ningún límite de líneas de código en ese caso).
3. Qué token/secreto exacto tengo que generar en SonarCloud (SONAR_TOKEN)
   y cómo añadirlo como "Repository secret" en GitHub (Settings → Secrets
   and variables → Actions → New repository secret).
4. Cómo es el archivo de workflow de GitHub Actions oficial recomendado
   por SonarCloud para un proyecto .NET (uso `sonarsource/sonarcloud-github-action`
   o el enfoque con `dotnet-sonarscanner` - dime cuál recomiendas y por
   qué para un proyecto con varios csproj en una misma solución).
5. Confirmar los límites reales del plan gratuito actual (líneas de
   código, número de análisis al mes, si caduca por inactividad).

Al final, dame el YAML de ejemplo del workflow y la lista exacta de
secretos de GitHub que debo crear.
```

**Checklist:**
- [x] Visibilidad del repositorio decidida: **pública** — las 3 capas se activan.
- [x] Capa 1: `Directory.Build.props` (`EnableNETAnalyzers` explícito) + `dotnet format --verify-no-changes` ya en `ci.yml` (Feature 11.8).
- [x] Capa 3: `.github/workflows/codeql.yml` creado (escaneo de seguridad nativo, sin secretos, correrá desde el primer push).
- [ ] Capa 2: `.github/workflows/sonarcloud.yml` ya creado, pero **fallará hasta que crees la cuenta SonarCloud y añadas el secreto `SONAR_TOKEN`** (Settings → Secrets and variables → Actions → New repository secret) — usuario ejecuta el prompt de arriba cuando quiera activarla; no bloquea nada mientras tanto (workflow separado, no obligatorio).

## Backlog (sin Feature asociada, no implementar en este Epic)

- **3.7 — Multi-usuario por tenant:** una Promotora hoy es un único `ApplicationUser`. Anotado como futurible; requeriría un modelo de "miembros del equipo" nuevo en Domain/Application, fuera del alcance de este Epic.

## Descartado (no reabrir sin nueva decisión explícita)

- 3.5 — Auditoría de acciones manuales.
- 3.6 — Notificaciones/recordatorios proactivos.
- 3.9 — Validación de contraste de `BrandColor`.

## Próximos pasos

1. ~~Usuario valida este documento completo~~ ✅ (2026-09-21).
2. ~~Resolver las 2 preguntas de negocio abiertas~~ ✅ (2026-09-21, ver Decisiones 9-10).
3. **En curso (agente):** 11.8 ✅ implementado, 11.9 Capa 1/Capa 3 ✅ implementadas, Capa 2 creada pero pendiente de `SONAR_TOKEN`. 11.2 (recuperación de contraseña) y 11.3 (desvincular comprador) en implementación.
4. **En curso (usuario):** ejecutar los prompts de 11.4/11.5/11.6/11.7 contra otra IA, crear las cuentas/recursos, y compartir las credenciales resultantes para configurarlas como variables de entorno de Render (Feature 11.7) una vez exista el Web Service.
5. Cuando el usuario tenga las 4 cuentas de Frente B listas, el agente implementa `S3FileStorageService`/`SmtpEmailSender`/`Dockerfile`/migración de connection string en ese orden.
