# Kiwbi - Development Roadmap & User Flows

Este documento define el flujo de uso de la aplicación y divide el MVP en "Epics" (Módulos funcionales) ordenados cronológicamente para el desarrollo. 

## 1. Flujo de Usuario (User Journey - MVP)

**Flujo de la Promotora (Admin):**
1. Llega a la Landing Page -> Se registra/Loguea.
2. Configura su perfil (Logo, Nombre, Colores).
3. Crea una "Promoción" (Edificio, ubicación, plano general).
4. Define las "Tipologías" (ej. Ático, Bajo) y da de alta las "Viviendas" (Piso, Puerta, M2, plano específico).
5. Crea los "Gremios" para la promoción y les asigna una fecha límite (`Cut-off date`).
6. Crea "Personalizaciones" dentro de cada Gremio y las asigna (a toda la promoción, a una tipología o a una vivienda concreta).
7. Define las "Opciones" para cada personalización (precio, cuál es por defecto).
8. Asigna el correo de un Comprador a una Vivienda -> Kiwbi envía un "Magic Link".
9. Revisa el estado de las elecciones de los compradores y marca pagos/confirmaciones manualmente.
10. Exporta el "Libro de Obra" (PDF/Excel ordenado por gremios) para enviarlo a la constructora.

**Flujo del Comprador:**
1. Recibe el correo de invitación de su Promotora.
2. Hace clic en el Magic Link -> Establece su contraseña -> Queda vinculado a su Vivienda.
3. Entra a su Dashboard y ve la/s promoción/es y vivienda/s que ha comprado.
4. Entra al detalle de la vivienda: Ve el plano, y un listado de personalizaciones agrupadas por Gremio.
5. Visualiza qué opciones tiene disponibles, los sobrecostes y la fecha límite.
6. Selecciona las opciones que desea (la UI se actualiza sin recargar vía HTMX).
7. Si la fecha límite ha pasado, ve las opciones bloqueadas (solo lectura).

---

## 2. Plan de Desarrollo (Epics del MVP)

El desarrollo debe seguir este orden estricto para no tener bloqueos de dependencias en la base de datos. Los Epics 1 a 6 conforman el núcleo funcional del MVP descrito en el flujo de usuario anterior. Los Epics 7 y 8 son mejoras transversales (navegación e imagen visual) sin dependencias de base de datos entre sí ni con los Epics 1-6; se documentan al final para no interrumpir el avance del núcleo funcional, pero pueden abordarse en el momento que convenga. El Epic 8b es la corrección de un defecto funcional preexistente (Epic 3) detectado durante la verificación manual de la Feature 8.2, pendiente de resolver antes de dar por cerrado el MVP. El Epic 9 es el diseño e implementación de la Landing pública y del flujo de acceso (Login, Registro y aceptación de invitación de comprador), otra mejora transversal sin dependencias de base de datos con los Epics 1-6. El Epic 10 nació como una tarea de QA manual pendiente sobre la Feature 6.3, pero su alcance se amplió (2026-09-20) a un seeder de datos de demo reutilizable e idempotente, cobertura de tests automáticos sobre la generación real de Excel/PDF, y la verificación manual original como último paso.

### EPIC 1: Foundation & Promotora Tenant
- **Feature 1.1:** Setup del proyecto (Clean Architecture, EF Core, PostgreSQL).
- **Feature 1.2:** Autenticación básica (Identity) y modelo de Promotora (Tenant).
- **Feature 1.3:** Dashboard básico de Promotora (Gestión de perfil y branding básico).

### EPIC 2: Real Estate Core (Catálogo)
- **Feature 2.1:** CRUD de Promociones (Upload de imágenes/planos).
- **Feature 2.2:** CRUD de Tipologías y Viviendas.
- **Feature 2.3:** Vista resumen de la promoción (Listado de viviendas con sus estados iniciales).

### EPIC 3: Customization Engine (El Motor)
- **Feature 3.1:** CRUD de Gremios con `FechaLimiteSeleccion`.
- **Feature 3.2:** CRUD de Personalizaciones y asociación condicional (por Promoción, Tipología o Vivienda).
- **Feature 3.3:** CRUD de Opciones por personalización (con selector de `IsDefault` y sobrecoste).

### EPIC 4: Onboarding B2B2C (Magic Links)
- **Feature 4.1:** Generación de token y envío de email al asignar un correo a una Vivienda.
- **Feature 4.2:** Flujo de aceptación del comprador, registro en Identity y vinculación de `UserId` con `ViviendaId`.

### EPIC 5: Buyer Experience (Frontend HTMX)
- **Feature 5.1:** Dashboard del Comprador (Mis Viviendas).
- **Feature 5.2:** Visualizador de Personalizaciones. Lógica de renderizado dinámico (¿Qué opciones le tocan a este piso?).
- **Feature 5.3:** Selección interactiva con HTMX. Guardado en la entidad `Elección`.
- **Feature 5.4:** Bloqueo de selecciones basado en la fecha límite del Gremio.

### EPIC 6: Management & Exporting
- **Feature 6.1:** Panel de la Promotora para ver el progreso de las elecciones por Vivienda.
- **Feature 6.2:** Gestión de estados manuales (Pendiente, Elegido, Confirmado, Pagado).
- **Feature 6.3:** Generación y exportación de reportes (Excel/PDF) agrupados por Gremio.

### EPIC 7: Reorganización de Navegación (Personalizaciones a nivel de Promoción)
- **Feature 7.1:** Punto de entrada de "Nueva Personalización" y su listado accesibles directamente desde la Promoción (agregando los Gremios existentes), en vez de exigir navegar primero a un Gremio concreto. Cambio acotado a `Kiwbi.Web` (routing/vistas), sin impacto en Domain/Application/Infrastructure. Ver `epic-07-customizations-promotion-navigation.md`.

### EPIC 8: Rediseño Visual Global (Design System)
- **Feature 8.1:** Investigación y selección de herramienta/enfoque para un sistema de diseño propio de Kiwbi (tema Bootstrap a medida, plantilla open-source, Tailwind CSS, generación asistida por IA, etc.), integrando el *branding* por tenant ya existente (`DeveloperCompany.Branding`).
- **Feature 8.2:** Implementación del sistema de diseño base (tokens de color/tipografía, layout, componentes comunes) parametrizado por el branding de cada promotora.
- **Feature 8.3:** Aplicación del rediseño a todas las vistas existentes (Epics 1-7).
- **Pendiente (diferido desde la Feature 8.1, 2026-09-19):** validar el contraste/legibilidad del color de marca (`BrandColor`) que elige cada promotora antes de aplicarlo como acento visual; hoy `BrandColor` solo valida el formato `#RRGGBB`, sin ninguna regla de contraste. Ver la decisión y su justificación en `epic-08-visual-redesign.md` ("Mecanismo de inyección del branding por tenant").

### EPIC 8b: Bug — "Añadir opción" en Personalización falla con 500 (DbUpdateConcurrencyException) — ✅ RESUELTO (2026-09-20)
Detectado el 2026-09-20 durante la verificación manual de la Feature 8.2 (Fase 2, pantalla Personalizaciones/Detalle) — **no relacionado con el rediseño visual**, es un defecto preexistente en `AddCustomizationOptionUseCase`/el mapeo EF Core de la colección `Customization.Options` (Epic 3, Feature 3.3), que solo salió a la luz al probar manualmente el formulario "Añadir opción" con datos reales por primera vez.
- **Síntoma:** al añadir una opción nueva a una Personalización ya existente, la petición devuelve 500 con `DbUpdateConcurrencyException: ... expected to affect 1 row(s), but actually affected 0 row(s)`.
- **Causa raíz confirmada:** EF Core clasifica Added-vs-Modified para una entidad descubierta por mutación de una colección de navegación (no por un `Add()` explícito) únicamente según si su clave primaria ya tiene un valor distinto del valor por defecto. Como `BaseEntity.Id` siempre asigna un `Guid.NewGuid()` real en el constructor (nunca generado por la base de datos, nunca `Guid.Empty`), EF asume que la entidad ya existe en la base de datos y genera un `UPDATE ... WHERE Id=@p` para una fila que nunca se insertó (0 filas afectadas). Se confirmó que esto ocurre incluso sin la llamada explícita a `_customizationRepository.Update(...)` (se probó quitándola y el fallo persistía idéntico), descartando esa llamada como causa raíz. El mismo defecto latente también afectaba a `AssignCustomizationToTypologyUseCase`/`AssignCustomizationToUnitUseCase` (ambos añaden una `CustomizationAssignment` nueva), no solo a `AddCustomizationOptionUseCase`.
- **Corrección aplicada:** exclusivamente en `Kiwbi.Infrastructure` (`CustomizationRepository.Update(Customization entity)`): antes de `SaveChanges`, se hace una lectura `AsNoTracking()` de los ids de `Options`/`Assignments` realmente persistidos para ese id de Customization, y se marca explícitamente `EntityState.Added` en cualquier elemento en memoria cuyo id no esté en ese conjunto persistido. Las 8 casos de uso de Customization siguen llamando a `_customizationRepository.Update(customization)` tras la mutación de dominio (se mantiene la forma original de la convención, solo cambió el cuerpo del repositorio).
- **Cobertura de test añadida:** `CustomizationOptionEfCoreMappingTests` (`tests/Kiwbi.Application.Tests/Customizations/AddCustomizationOption/`), un test que usa un `KiwbiDbContext` real respaldado por SQLite en memoria (añadido `Microsoft.EntityFrameworkCore.Sqlite` + una referencia de proyecto de solo test a `Kiwbi.Infrastructure`) para reproducir el fallo real de mapeo EF Core, ya que los tests existentes mockean `ICustomizationRepository` y nunca ejercitan el mapeo real.
- **Verificación:** 306 tests pasando (131 Domain + 175 Application) y verificación manual en la app en ejecución (se añadió correctamente una opción nueva a la Personalización real del reporte de bug original, sin error 500).
- **Prioridad:** resuelto antes de continuar con el Epic 9, sin arrastrar el defecto funcional conocido al cierre del MVP.

### EPIC 9: Landing Page y Experiencia de Acceso (Auth)
Continuación natural del Epic 8 (mismo sistema de diseño Kiwbi ya establecido) para las pantallas explícitamente dejadas fuera del inventario de la Feature 8.1 ("Ronda 3", marcadas *fuera de prioridad por ahora*): la Landing pública (`HomeController.Index`, hoy la plantilla por defecto de ASP.NET Core sin contenido propio) y el flujo de acceso (`AccountController.Login/Register`, `OnboardingController.Accept/InvalidInvitation/Welcome`). Se documenta como Epic propio (en vez de una Feature más del Epic 8) porque introduce una identidad visual pública nueva (marca propia de Kiwbi, sin ningún tenant todavía resuelto) distinta de la del panel ya diseñado. Sin dependencias de base de datos con los Epics 1-6. Ver `epic-09-landing-and-auth.md`.
- **Feature 9.1:** Landing pública (marketing, sin login): hero, KPIs ilustrativos, "cómo funciona" (Promotora/Comprador), beneficios, CTA de registro autoservicio y footer.
- **Feature 9.2:** Login y Registro de Promotora con el nuevo layout de tarjeta centrada y marca propia de Kiwbi.
- **Feature 9.3:** Aceptación de invitación de comprador (Magic Link), con acento de marca de la promotora invitante ya que el tenant es conocido desde el token.

### EPIC 10: Datos de Demo y Verificación de Exportación de Reportes (Cierre de la Feature 6.3)
La Feature 6.3 (Epic 6) implementó y testeó unitariamente la generación de reportes, mockeando siempre `IHousingPromotionReportGenerator`; la generación real de bytes con ClosedXML/QuestPDF nunca se ejecutó contra datos reales, quedando pendiente en el checklist de cierre de Epic 6. El alcance de este Epic se amplió (2026-09-20) más allá de esa verificación manual: además de cerrarla, construye un seeder de datos de demo reutilizable e idempotente (pensado también para servir de base a una demo del producto) y añade cobertura de tests automáticos reales sobre la generación de Excel/PDF, hoy inexistente. Requiere una instancia de PostgreSQL en ejecución (`docker-compose up`). Ver `epic-10-demo-data-and-report-verification.md`.
- **Feature 10.1:** Seeder de datos de demo (CLI, idempotente): una Promotora con 3 Promociones en distintos estados de avance, cubriendo los cuatro estados de elección (`Pending`/`Selected`/`Confirmed`/`Paid`), las tres situaciones de Gremio (vencido y resuelto, vencido con pendientes, y abierto) y varios estados de invitación de comprador (incluida una caducada). Documento aparte `demo-data-guide.md` con credenciales de la Promotora y de los Compradores relevantes, y el guion de la demo.
- **Feature 10.2:** Cobertura de tests automáticos sobre la exportación a Excel (generador real ClosedXML con aserciones de celda, casos límite del cálculo de reporte, test a nivel de Controller) y verificación manual final (`HousingPromotionChoicesController.ExportExcel`): descargar el `.xlsx`, abrirlo y comprobar agrupación por Gremio/Vivienda, opciones efectivas/sobrecostes/estados correctos, y exclusión de Personalizaciones no aplicables a cada Vivienda.
- **Feature 10.3:** Cobertura de tests automáticos sobre la exportación a PDF (smoke test de bytes/cabecera del generador real QuestPDF, test a nivel de Controller) y verificación manual final (`ExportPdf`): descargar el `.pdf`, abrirlo y comprobar el mismo contenido/agrupación que en 10.2, además de que la maquetación de QuestPDF (tablas, cabeceras, paginación) es legible y no corta contenido.
- **Feature 10.4:** Corregir cualquier defecto detectado en la verificación manual de 10.2/10.3 (en `HousingPromotionReportGenerator`, `ExportHousingPromotionReportUseCase` o el punto que corresponda), repetir la verificación hasta confirmar que ambos ficheros son correctos, y actualizar el checklist de cierre de Epic 6 y la memoria de repositorio con el resultado.