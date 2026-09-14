# Kiwbi - Business Context (Source of Truth)

## 1. Visión General
Kiwbi es una aplicación web B2B2C diseñada para el sector inmobiliario. Su objetivo principal es permitir a las Promotoras gestionar y centralizar las personalizaciones (extras y acabados) de los pisos en venta, y permitir a los Compradores visualizar y seleccionar dichas opciones de forma digital.

## 2. Roles del Sistema
- **Promotora (Admin):** Crea promociones, define tipologías de viviendas, configura las opciones de personalización, gestiona los gremios y las fechas límite. Envía invitaciones a compradores y valida/gestiona los estados de las selecciones (pagos y aceptaciones manuales).
- **Comprador (Usuario final):** Accede mediante invitación (Magic Link). Solo ve la información de sus viviendas vinculadas. Navega por las personalizaciones disponibles, ve sobrecostes (si los hay) y elige las opciones antes de la fecha límite establecida por la Promotora.

## 3. Alcance del MVP (Fase 1)
El MVP se centra estrictamente en la **preventa y configuración durante la construcción**:
- Gestión del tenant de la Promotora (datos básicos).
- Creación de Promociones, Viviendas (y Tipologías).
- Definición de Gremios (con fechas límite) y Personalizaciones (por promoción, tipología o vivienda).
- Flujo de Onboarding B2B2C: Registro del piso por la Promotora -> Envío de Magic Link -> El Comprador entra y ya está vinculado a su vivienda.
- Selección de opciones por parte del comprador (con visualización de miniaturas/planos).
- Exportación de listados definitivos (PDF/Excel) ordenados por gremios para entregar a la constructora.

## 4. Fuera del MVP (Para futuras fases - NO implementar ahora)
- Acceso directo al sistema para la Constructora.
- Pasarelas de pago integradas (los estados de pago se gestionan manualmente por la Promotora).
- Módulo de incidencias post-entrega (ticketing, subida de fotos de desperfectos, etc.).