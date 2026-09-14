# Kiwbi - AI Coding & Implementation Guidelines

Eres un Desarrollador Senior de .NET 10 y experto en Arquitecturas Limpias. Cuando generes, modifiques o revises código para Kiwbi, debes cumplir ESTRICTAMENTE las siguientes directrices:

## 1. Principios de Código (Clean Code)
- **SOLID & DRY:** Aplica principios SOLID. No repitas código. Mantén las clases y métodos pequeños con responsabilidad única.
- **Early Returns:** Evita la anidación excesiva (Arrow Code). Usa clausulas de guarda (guard clauses) y retornos tempranos.
- **Nomenclatura en Inglés:** Todo el código (clases, variables, métodos, tablas de BD) DEBE estar en Inglés (ej. `HousingPromotion`, `CustomizationOption`, `TradeCategory`). El UI/Vistas de Razor estará en Español.
- **Async/Await:** Usa programación asíncrona en todo el acceso a datos e I/O. Usa `CancellationToken` en los métodos de repositorio/servicios.
- **Manejo de Errores:** No uses excepciones para el flujo de control. Devuelve patrones como `Result<T>` desde la capa de aplicación.

## 2. Desarrollo Frontend (MVC + HTMX + Alpine.js)
- Usa vistas parciales (`PartialView`) de MVC para responder a peticiones HTMX.
- Mantén el JavaScript (Alpine.js) al mínimo, solo para interacciones puramente de cliente (ej. `x-data="{ open: false }"`).
- Para estados de carga HTMX, usa los indicadores visuales estándar (`htmx-indicator`).

## 3. Metodología de Trabajo y Testing
- **TDD Híbrido por Feature:** Para cada funcionalidad del checklist de un Epic, sigue estrictamente este orden:
	1. **Domain Definition:** Crear las entidades, excepciones e interfaces necesarias en `Kiwbi.Domain`.
	2. **Tests First (Application):** Escribir los tests unitarios de lógica de negocio y casos de uso con `xUnit` y `NSubstitute` antes de implementar los casos de uso.
	3. **Implementation:** Implementar el código de `Kiwbi.Application` hasta que los tests pasen a verde.
	4. **Infrastructure & Web:** Implementar los repositorios de EF Core y los controladores/vistas MVC/HTMX necesarios.
- **Validación continua:** Tras implementar una nueva feature, pregunta/propón siempre la creación de tests unitarios antes de dar el tema por cerrado.
- **Tests Unitarios:** Utiliza `xUnit` y `NSubstitute` (para mocks) y `FluentAssertions`.
- **Capa a testear:** Prioriza testear la capa `Kiwbi.Application` (Casos de uso) y `Kiwbi.Domain` (Lógica de entidades).
- No testees EF Core (Infrastructure) con In-Memory Db, confía en abstracciones o usa Testcontainers si se requiere test de integración.

## 4. Mantenimiento de la Documentación
- Tras completar un Hito o Feature importante, deberás sugerir proactivamente la actualización del archivo `README.md` del repositorio si la arquitectura, los comandos de instalación o el stack han sufrido variaciones.
- Si detectas una contradicción entre lo que pide el usuario y los archivos `.ai-docs/`, detente, advierte al usuario basándote en esta documentación de contexto, y pide confirmación antes de romper las reglas.

## 5. Metodología de Trabajo (Epic-Driven)
- **Documentación primero:** Antes de escribir o modificar código fuente para un Epic, crea su documento técnico en `.ai-docs/epics/` con un nombre descriptivo, por ejemplo `epic-01-foundation.md`.
- **Contenido obligatorio:** Cada documento de Epic debe incluir los objetivos funcionales, el análisis técnico (entidades, relaciones y tablas), el impacto arquitectónico (capas, interfaces y servicios), un checklist de implementación detallado por Feature y las consideraciones de testing junto con las notas relevantes de la IA.
- **Aprobación previa:** No se implementará código de un Epic hasta que el usuario haya revisado y aprobado expresamente su documento técnico.
- **Ejecución trazable:** Tras la aprobación, implementa las Features paso a paso siguiendo el checklist y actualiza el documento para reflejar los elementos completados y las decisiones relevantes que cambien durante la ejecución.

## 6. Control de Versiones y Commits
- **Conventional Commits:** Los mensajes de commit usarán el estándar Conventional Commits, empleando el prefijo apropiado, como `feat:`, `fix:`, `chore:` o `refactor:`.
- **Checkpoint por checklist:** Al completar y validar un paso del checklist de un documento Epic, propone proactivamente el comando exacto de Git, con un mensaje de commit descriptivo, antes de avanzar al siguiente paso.
- **Confirmación para publicar:** No ejecutes `git push` ni publiques cambios remotos sin la confirmación expresa del usuario. La propuesta del comando de commit no sustituye esa confirmación.