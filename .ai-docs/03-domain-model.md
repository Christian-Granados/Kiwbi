# Kiwbi - Domain Model & Business Rules

## 1. Entidades Principales y Jerarquía
- **Promotora:** Entidad raíz del tenant. Tiene Promociones.
- **Promoción:** Edificio o conjunto residencial (Nombre, Ciudad, Dirección, Imagen/Plano general).
  - *Tiene múltiples* **Viviendas**.
  - *Tiene múltiples* **Gremios/Categorías**.
- **Tipología (opcional):** Agrupación lógica de viviendas dentro de una promoción (ej. "Ático Tipo A", "Bajo 2 Habitaciones").
- **Vivienda:** Piso concreto. Pertenece a una Promoción (y opcionalmente a una Tipología). Tiene Planta, Puerta, M2 construidos/útiles, Plano específico.
  - *Se vincula a* uno o varios **Compradores** (Identity Users).

## 2. Jerarquía de Personalizaciones
1. **Gremio/Categoría:** Ej. "Carpintería", "Fontanería". Pertenece a la Promoción.
   - **Regla Crítica:** Aquí se define la `FechaLimiteSeleccion`.
2. **Personalización:** Ej. "Suelo", "Grifos". Pertenece a un Gremio.
   - Puede asignarse a nivel de: Toda la Promoción, Una Tipología concreta, o Una Vivienda concreta.
3. **Opción:** Ej. "Parquet Roble (Base, +0€)", "Porcelánico Premium (+500€)". Pertenece a una Personalización. Una de ellas debe ser `IsDefault = true`.
4. **Elección (HomeCustomizationChoice):** Entidad transaccional que relaciona una Vivienda, una Personalización y la Opción elegida por el comprador.

## 3. Estados y Reglas de Negocio
- **Estado de la Elección (Manuales gestionados por Promotora):** 
  - `Pending` (El comprador no ha elegido nada aún).
  - `Selected` (Comprador ha elegido una opción).
  - `Confirmed` (Promotora acepta la elección).
  - `Paid` (Promotora marca como cobrado el sobrecoste, si aplica).
- **Regla del Gremio (Cut-off Date):** Si `DateTime.UtcNow > Gremio.FechaLimiteSeleccion`, la Personalización queda bloqueada. Las elecciones en estado `Pending` pasan automáticamente (o en lectura) a la opción donde `IsDefault == true`. El usuario no puede modificar su elección a través de HTMX/Web.
- **Onboarding (Magic Link):** Cuando la Promotora añade un email a una Vivienda, se crea un registro de invitación pendiente. Al aceptar vía Magic Link, el email se registra en Identity y se enlaza la `ViviendaId` con el `UserId`.