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

El desarrollo debe seguir este orden estricto para no tener bloqueos de dependencias en la base de datos:

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