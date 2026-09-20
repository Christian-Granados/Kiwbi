# Guía de datos de demo (Feature 10.1)

Este documento describe el tenant de demo sembrado por `DemoDataSeeder` (`src/Kiwbi.Web/DemoSeeding/`), pensado tanto para la verificación manual del Epic 10 como para una demo real del producto. Es **idempotente**: puede volver a ejecutarse antes de cada demo sin dejar duplicados (borra el tenant demo anterior por completo y lo vuelve a crear desde cero).

## Cómo (re)generar los datos

Con Postgres en marcha (`docker-compose up`) y las migraciones aplicadas:

```powershell
dotnet run --project src/Kiwbi.Web -- --seed-demo
```

El proceso registra el progreso por log y termina sin arrancar Kestrel. Repetirlo antes de cada demo es seguro.

## Credenciales

| Rol | Email | Contraseña |
|---|---|---|
| Promotora ("Kiwbi Demo") | `demo@kiwbi.test` | `DemoKiwbi!2026` |
| Comprador 1 | `buyer1@kiwbi.test` | `DemoBuyer!2026` |
| Comprador 2 | `buyer2@kiwbi.test` | `DemoBuyer!2026` |
| Comprador 3 | `buyer3@kiwbi.test` | `DemoBuyer!2026` |
| Comprador 4 | `buyer4@kiwbi.test` | `DemoBuyer!2026` |

## Las 3 Promociones

### 1. Residencial Vistalar (Valencia) — ACABADA / entregada

3 Tipologías (2 dormitorios, 3 dormitorios, Ático), 8 Viviendas, todas **Vendida**. 2 Gremios, ambos **vencidos** (Alicatados -60 días, Carpintería -45 días):

- **Alicatados**: "Suelo salón" (toda la promoción), "Azulejo baño" (tipología "2 dormitorios").
- **Carpintería**: "Puerta de entrada" (toda la promoción), "Armario empotrado" (solo viviendas 1ºA/1ºB).

- **1ºA — Comprador 1** (`buyer1@kiwbi.test`): las 4 personalizaciones aplicables en estado **Pagada**. Caso "todo cerrado".
- **1ºB — Comprador 2** (`buyer2@kiwbi.test`): 3 de las 4 personalizaciones en **Pagada**; "Armario empotrado" se dejó deliberadamente **sin elección** (Gremio Carpintería ya vencido) — ideal para pulsar "Confirmar" en vivo durante la demo.
- Resto de viviendas (2ºA, 2ºB, 3ºA, 3ºB, 4ºA, 5ºA): Vendidas, sin comprador vinculado todavía (vendida, pendiente de invitar).

### 2. Residencial Puerta Azul (Sevilla) — CASI TERMINADA

2 Tipologías (2 dormitorios, 3 dormitorios), 6 Viviendas (4 Vendidas, 2 Reservadas). 3 Gremios: Alicatados (-40 días, vencido), Carpintería (-20 días, vencido), Electricidad (+30 días, **abierto**):

- **Alicatados**: "Suelo salón" (toda la promoción), "Grifería baño" (tipología "2 dormitorios").
- **Carpintería**: "Puerta de entrada" (toda la promoción), "Persiana motorizada" (solo viviendas 2ºA/2ºB).
- **Electricidad**: "Mecanismos eléctricos" (toda la promoción).

- **2ºA — Comprador 3** (`buyer3@kiwbi.test`): Gremios vencidos ya **Confirmada**/**Seleccionada**, Gremio abierto en **Seleccionada** — caso "en curso, casi todo decidido".
- **2ºB — Comprador 4** (`buyer4@kiwbi.test`): el caso más variado — "Suelo salón" (Alicatados, vencido) **sin decidir todavía** (Pendiente con opción por defecto como efectiva), "Puerta de entrada" ya **Confirmada** y "Persiana motorizada" **Seleccionada**, y "Mecanismos eléctricos" (Electricidad, abierto) **sin decidir** (Pendiente, sin opción efectiva al no haber vencido).
- **3ºA (Reservada)**: invitación de comprador enviada hace tiempo, nunca aceptada, ya **Caducada** (`lead.caducado@kiwbi-demo.test`).
- **3ºB (Reservada)**: invitación enviada recientemente, todavía **Pendiente**/vigente (`lead.pendiente@kiwbi-demo.test`) — muestra el onboarding en marcha.

### 3. Residencial Nuevos Pinos (Zaragoza) — EN DEFINICIÓN

2 Tipologías (2 dormitorios, 3 dormitorios), 5 Viviendas, todas **Disponible**. 1 Gremio "Alicatados" con fecha límite muy lejana (+180 días). 1 Personalización "Suelo salón" (toda la promoción) recién creada con su opción por defecto + 1 opción adicional. Cero invitaciones, cero compradores, cero elecciones — fase puramente de catálogo/configuración.

## Guion sugerido para la demo

1. Login como `demo@kiwbi.test` → Panel de promociones (3 promociones en distintos estados).
2. **Residencial Vistalar**: abrir Progreso → ver 1ºA totalmente pagado y 1ºB con "Armario empotrado" pendiente → pulsar "Confirmar" en vivo (el Gremio ya está vencido, así que se asigna la opción por defecto y se confirma al momento) → exportar el reporte Excel/PDF y mostrar la agrupación por Gremio.
3. **Residencial Puerta Azul**: mostrar el Gremio "Electricidad" todavía abierto (no bloqueado), la vivienda 3ºA con la invitación caducada y 3ºB con la invitación pendiente, y reenviar/cancelar una invitación en vivo.
4. **Residencial Nuevos Pinos**: mostrar el catálogo recién creado (Tipologías/Viviendas/Gremio/Personalización) como ejemplo de una promoción en fase de configuración inicial.
5. Login como `buyer1@kiwbi.test` o `buyer3@kiwbi.test` para mostrar el Portal del Comprador (Mis viviendas → detalle con las personalizaciones agrupadas por Gremio).
