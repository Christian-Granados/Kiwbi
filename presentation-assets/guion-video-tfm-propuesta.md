# Guion del vídeo del TFM - Propuesta para validación

> Estado: guion validado. Base aprobada para definir los slides y redactar el prompt para Gemini Pro.

## 1. Objetivo de la defensa

La defensa debe conseguir que el tribunal entienda, en este orden:

1. Qué problema real aborda Kiwbi.
2. Por qué el proceso es más complejo de lo que parece.
3. Qué circuito resuelve el producto y para quién.
4. Qué incluye realmente el MVP y qué queda fuera.
5. Cómo se ha construido con criterios técnicos y de calidad.
6. Que el circuito funciona de extremo a extremo en una aplicación desplegada.
7. Que el siguiente paso no es añadir funciones indiscriminadamente, sino validar y ajustar el producto con profesionales del sector.

La idea central de toda la presentación será:

> Cada elección de un comprador termina convirtiéndose en una instrucción de obra. El reto es conseguir que esa instrucción sea correcta, aplicable a esa vivienda, recibida a tiempo y trazable.

## 2. Enfoque recomendado

### Formato

- Duración objetivo: **10-12 minutos**.
- Introducción ante cámara: **25-30 segundos**.
- Parte 1 - Presentación con slides: **4-5 minutos**.
- Parte 2 - Demostración de la aplicación: **5-6 minutos**.
- Cierre ante cámara: **25-30 segundos**.

Las especificaciones de BigSchool no establecen una duración máxima. El criterio será mantener una defensa breve y directa: explicar solo lo necesario para entender el problema, el aprendizaje y el circuito que después se demostrará. Los tiempos de cada bloque son límites orientativos, no objetivos que haya que llenar.

Toda la narración se realizará en **primera persona singular**. El proyecto se presentará como trabajo propio: `he desarrollado`, `he utilizado`, `mi objetivo` y `mi siguiente paso`.

### Criterio visual

Los slides deben explicar; la demo debe demostrar. Por eso no conviene llenar la presentación de capturas de cada pantalla.

La recomendación es usar en los slides:

- Diagramas de proceso.
- Jerarquías y relaciones entre conceptos.
- Comparaciones visuales entre el proceso fragmentado y el circuito centralizado.
- Una representación clara del alcance del MVP.
- Un único esquema técnico sencillo.
- Como máximo, una composición breve de producto con dos o tres recortes de interfaz si se considera necesario para que la presentación también funcione como documento independiente.

No se deben incluir métricas de ahorro, reducción de errores o adopción que todavía no hayan sido medidas. La necesidad parte de carencias trasladadas por contactos profesionales relacionados con promotoras, pero esto se presentará como el origen y contraste inicial del problema, no como una validación de mercado ya concluida.

## 3. Estructura general

### Apertura - Presentar el recorrido

Una introducción breve ante cámara para presentar al autor, situar el TFM y anticipar el recorrido sin explicar todavía la solución.

### Acto 1 - Entender el problema y la propuesta

Ocho slides para situar al tribunal antes de abrir la aplicación.

### Acto 2 - Ver el circuito funcionando

Una demostración guiada que conecta ambos lados del modelo B2B2C:

1. La Promotora configura y supervisa.
2. El Comprador consulta y elige.
3. Kiwbi aplica reglas de alcance y plazo.
4. La Promotora confirma y genera el Libro de Obra.

### Cierre

Volver ante cámara, recuperar la promesa inicial y presentar el **MVP funcional, pendiente de validación con el sector**, como resultado del aprendizaje y del desarrollo realizado durante el TFM.

---

## 4. Introducción ante cámara

**Tiempo:** 25-30 segundos.

**Plano:** plano medio, cámara a la altura de los ojos y fondo sencillo. Hablar directamente a cámara. No mostrar todavía slides ni la aplicación.

**Narración propuesta:**

> Hola, soy Christian Granados y en este vídeo voy a presentar Kiwbi, el proyecto que he desarrollado como Trabajo Fin de Máster. Kiwbi es una plataforma digital que convierte las elecciones del comprador en instrucciones de obra claras y trazables. Primero explicaré el problema que aborda, las reglas principales del proceso y las decisiones con las que he construido el MVP. Después pasaré a la aplicación desplegada para mostrar el circuito completo desde los dos puntos de vista: el de la Promotora y el del Comprador.

**Transición a las slides:**

> Para entender por qué hace falta este circuito, voy a empezar por el problema que existe detrás de cada personalización.

Al terminar la frase, cortar a la diapositiva 1. La narración de la slide debe comenzar inmediatamente, sin saludo adicional ni pausa larga.

---

## 5. Guion de la presentación con slides

### Slide 1 - Kiwbi

**Tiempo:** 20-25 segundos.

**Función:** presentar el proyecto y abrir con el problema, no con la tecnología.

**Visual sugerido:** nombre Kiwbi, subtítulo y una imagen limpia relacionada con la personalización de una vivienda en construcción. Sin captura de la aplicación.

**Texto principal sugerido:**

> Kiwbi
>
> De la elección del comprador a una instrucción de obra clara y trazable.

**Narración propuesta:**

> He desarrollado Kiwbi para abordar un momento muy concreto de la preventa de vivienda: la elección de acabados y extras durante la construcción. A primera vista parece un proceso sencillo, como escoger un suelo o una grifería. Pero cada elección debe terminar convertida en una instrucción de obra correcta, aplicable a una vivienda concreta, recibida dentro de plazo y preparada para que la constructora pueda ejecutarla.

**Transición:**

> Y ahí es donde una elección aparentemente sencilla se convierte en un problema de coordinación.

### Slide 2 - El proceso actual está fragmentado

**Tiempo:** 30-35 segundos.

**Función:** explicar la necesidad sin exageraciones ni datos inventados.

**Visual sugerido:** flujo fragmentado entre Promotora, Comprador y Constructora, atravesado por correo, hojas de cálculo, documentos y revisiones manuales.

**Narración propuesta:**

> La idea parte de carencias que me han trasladado contactos profesionales relacionados con promotoras. En este proceso, la información puede quedar repartida entre hojas de cálculo, correos y documentos actualizados manualmente. La Promotora debe definir opciones y fechas, recibir respuestas, comprobar sobrecostes y consolidar el resultado para obra. El problema no es una herramienta aislada, sino mantener sincronizadas muchas decisiones y conservar su trazabilidad.

**Mensaje que debe quedar:** la dificultad está en coordinar y consolidar, no solo en mostrar un catálogo.

### Slide 3 - La complejidad que hay detrás de una elección

**Tiempo:** 35-40 segundos.

**Función:** evidenciar las reglas del dominio.

**Visual sugerido:** una jerarquía visual:

`Promotora → Promoción → Tipologías/Viviendas → Gremios → Personalizaciones → Opciones → Elecciones`

Añadir alrededor cuatro condicionantes: alcance, fecha límite, sobrecoste y estado.

**Narración propuesta:**

> Una opción no siempre está disponible para todas las viviendas. Puede aplicarse a una promoción completa, solo a una tipología o únicamente a viviendas concretas. Además, cada personalización pertenece a un gremio con su propia fecha límite, puede incluir una opción por defecto y puede tener sobrecoste. Después, cada elección avanza por distintos estados: pendiente, seleccionada, confirmada y pagada. El sistema debe resolver todas estas combinaciones sin mostrar opciones incorrectas ni permitir cambios fuera de plazo.

**Transición:**

> Kiwbi convierte estas reglas dispersas en un único circuito digital.

### Slide 4 - La propuesta: un circuito B2B2C

**Tiempo:** 30-35 segundos.

**Función:** explicar qué hace Kiwbi sin entrar aún en pantallas.

**Visual sugerido:** dos carriles conectados.

- Carril Promotora: configura → invita → supervisa → confirma → exporta.
- Carril Comprador: accede → consulta → compara → elige.
- En el centro: Kiwbi aplica alcance, plazos, opción por defecto y trazabilidad.

**Narración propuesta:**

> Mi propuesta es una aplicación B2B2C. La Promotora configura sus promociones, viviendas, gremios, personalizaciones y opciones; invita al comprador mediante un enlace seguro; y supervisa el progreso de cada vivienda. El Comprador accede a un portal con su vivienda y ve únicamente las opciones que le corresponden. Kiwbi conecta ambos recorridos, aplica las reglas y conserva un estado único del proceso.

### Slide 5 - Qué cubre el MVP

**Tiempo:** 35-40 segundos.

**Función:** delimitar el producto de forma honesta.

**Visual sugerido:** recorrido horizontal de seis etapas:

1. Catálogo inmobiliario.
2. Configuración de personalizaciones.
3. Invitación del comprador.
4. Selección dentro de plazo.
5. Seguimiento y estados manuales.
6. Libro de Obra en PDF/Excel.

En una franja separada, mostrar "Fuera del MVP".

**Narración propuesta:**

> El MVP cubre el circuito desde la configuración de una promoción hasta la generación del Libro de Obra. Incluye promociones, tipologías, viviendas, gremios con fecha límite, opciones con sobrecoste, invitaciones por Magic Link, selección del comprador, seguimiento por vivienda y exportación final agrupada por gremio. No incluye una pasarela de pago, acceso directo para la constructora ni gestión de incidencias posteriores a la entrega. Los estados de confirmación y pago se gestionan manualmente por la Promotora.

**Mensaje que debe quedar:** el alcance es coherente y cerrado; no intenta resolver todo el ciclo inmobiliario.

### Slide 6 - Las reglas que convierten un catálogo en una herramienta de obra

**Tiempo:** 35-40 segundos.

**Función:** destacar la lógica diferencial del MVP.

**Visual sugerido:** cuatro bloques conectados:

- Aplicabilidad: promoción, tipología o vivienda.
- Plazo: abierto o vencido.
- Resolución: elección explícita u opción por defecto.
- Seguimiento: pendiente, seleccionada, confirmada y pagada.

**Narración propuesta:**

> El núcleo de Kiwbi no es el CRUD ni el aspecto visual. Es el motor que decide qué personalización corresponde a cada vivienda y qué ocurre cuando vence un plazo. Mientras un gremio está abierto, el comprador puede elegir. Cuando vence, la selección queda bloqueada y, si no existe una elección, la opción por defecto pasa a ser la opción efectiva. La Promotora puede entonces confirmar la decisión y marcarla como pagada cuando corresponda. Así, el estado final puede convertirse en un documento operativo para obra.

### Slide 7 - Cómo lo he construido

**Tiempo:** 40-45 segundos.

**Función:** aportar credibilidad técnica sin convertir la defensa en una clase de arquitectura.

**Visual sugerido:** diagrama sencillo de cuatro capas más una línea de infraestructura.

- Web: ASP.NET Core MVC, Razor, Bootstrap y HTMX.
- Application: casos de uso.
- Domain: entidades y reglas.
- Infrastructure: PostgreSQL, Identity, correo, almacenamiento y reportes.
- Producción: Render, Neon, Cloudflare R2 y Brevo.

**Narración propuesta:**

> He desarrollado la aplicación con .NET 10 y ASP.NET Core MVC siguiendo Clean Architecture. He separado las reglas de dominio, los casos de uso y las integraciones técnicas para mantener responsabilidades claras. En la interfaz he utilizado Razor y HTMX para conseguir interacciones dinámicas sin incorporar una SPA. También he desplegado la solución con Docker y servicios cloud, y he acompañado el desarrollo con 325 tests automatizados, integración continua y análisis estático.

**Nota de narración:** no enumerar todas las librerías menores. El detalle completo ya está en el README.

### Slide 8 - Desarrollo asistido por IA y siguiente etapa

**Tiempo:** 40-45 segundos.

**Función:** explicar el aprendizaje y el uso de IA durante el desarrollo, cerrando la parte conceptual sin convertirla en una presentación comercial.

**Visual sugerido:** ciclo iterativo:

`Contexto y reglas → diseño por Epics → implementación → tests y revisión → validación manual → documentación`

Después, una flecha hacia: `Contraste con profesionales del sector`.

**Narración propuesta:**

> La IA ha formado parte de mi proceso de desarrollo, pero no ha sustituido mis decisiones. He organizado el proyecto por Epics y he documentado primero el contexto, las reglas, el impacto técnico y los criterios de aceptación. Después he utilizado la IA para explorar soluciones, implementar, revisar y mantener la trazabilidad, contrastando cada avance con tests y verificaciones manuales. El resultado es un MVP funcional pendiente de validación con el sector. Mi siguiente paso será probarlo con profesionales, detectar necesidades no cubiertas y adaptarlo a su operativa real.

**Transición a la demo:**

> Hasta aquí he explicado el circuito. Ahora voy a mostrar cómo una decisión pasa del portal del comprador al control de la Promotora y termina preparada para obra.

---

## 6. Guion de la demostración en vivo

### Preparación previa a la grabación

La demo se grabará íntegramente sobre la aplicación publicada en **https://kiwbi.onrender.com**. Al comenzar la demostración se mostrará brevemente la barra de direcciones para dejar claro que se está utilizando el despliegue real, con su base de datos, almacenamiento y correo de producción, y no una versión preparada en local.

Los datos utilizados seguirán siendo datos de demostración, pero todas las acciones se ejecutarán contra el entorno público. Los cortes de edición servirán para ordenar la explicación, eliminar silencios o repetir una toma, pero nunca para simular resultados ni mezclar ejecuciones locales con la demostración publicada.

El vídeo se grabará y editará **por bloques**, no como una toma única:

1. Introducción ante cámara.
2. Presentación con slides, dividida en uno o varios fragmentos.
3. Demostración de la aplicación, separada por escenas funcionales.
4. Cierre ante cámara.

Esta estructura permite repetir únicamente el fragmento que falle, eliminar silencios y tiempos de carga, y montar después el resultado final.

Antes de grabar:

1. Abrir `https://kiwbi.onrender.com` con antelación para despertar la instancia gratuita de Render y comprobar que responde correctamente.
2. Verificar en producción que las cuentas y datos necesarios para el recorrido siguen disponibles, sin ejecutar todavía las transiciones irreversibles reservadas para la grabación.
3. Abrir dos sesiones de navegador independientes sobre la URL pública:
   - Sesión Promotora: `demo@kiwbi.test`.
   - Sesión Comprador: `buyer4@kiwbi.test`.
4. Dejar ambas sesiones autenticadas y las pestañas necesarias preparadas.
5. Preparar una tercera pestaña para abrir el PDF exportado desde producción.
6. Desactivar notificaciones del sistema y ocultar marcadores o información personal del navegador.
7. Usar una resolución estable, preferiblemente 1920x1080, y un zoom que mantenga legibles tablas y textos.
8. Ensayar el recorrido y la narración sin consumir la acción irreversible de confirmación; esa transición se ejecutará únicamente durante la toma definitiva.

No deben mostrarse contraseñas, variables de entorno, logs con enlaces privados ni paneles de proveedores cloud.

### Chuleta minimalista para grabar la demo

**Inicio - PROMOTORA** (`demo@kiwbi.test`)

1. **Promociones** → enseñar las tres promociones y sus distintos momentos.
2. **Puerta Azul → Gremios y Personalizaciones** → mostrar plazos abiertos/vencidos y distintos alcances. No editar.
3. **Puerta Azul → Viviendas → 3ºA y 3ºB → Invitaciones** → mostrar una caducada y otra pendiente. No enviar.

**CAMBIO A COMPRADOR** (`buyer4@kiwbi.test`)

4. **Mis viviendas → Puerta Azul → 2ºB → Mecanismos eléctricos** → comparar vencido/abierto y elegir una opción. Mostrar actualización sin recarga.

**CAMBIO A PROMOTORA** (`demo@kiwbi.test`)

5. **Puerta Azul → Progreso → refrescar** → localizar la elección recién realizada y mostrar la conexión entre ambos roles.
6. **Vistalar → Progreso → 1ºB → Armario empotrado** → mostrar opción por defecto y pulsar `Confirmar`.
7. **Vistalar → Progreso → Exportar PDF** → abrir el Libro de Obra y mostrar su agrupación por gremio y vivienda.

**Salida** → mantener el PDF visible, decir la frase de transición y cortar al cierre ante cámara.

### Demo 1 - Tres promociones, tres momentos del proceso

**Tiempo:** 25-30 segundos.

**Sesión:** Promotora.

**Acción:** abrir el panel de promociones.

**Narración propuesta:**

> Entro como responsable de una Promotora. Los datos de demo representan tres promociones en diferentes momentos: una en definición, otra en curso y otra ya cerrada. Esto permite que el mismo sistema acompañe el proceso desde la configuración inicial hasta la entrega de las decisiones a obra.

**Qué demostrar:** visión multi-promoción y contexto de negocio. No entrar todavía en formularios de alta.

### Demo 2 - Configuración y reglas de aplicabilidad

**Tiempo:** 40-45 segundos.

**Sesión:** Promotora.

**Acción:** entrar en `Residencial Puerta Azul` y recorrer brevemente Gremios y Personalizaciones.

**Narración propuesta:**

> Dentro de una promoción, la información se organiza según la estructura real del proceso. Los gremios tienen fechas límite distintas. En este caso, Alicatados y Carpintería ya están vencidos, mientras que Electricidad continúa abierto. Las personalizaciones pueden aplicarse a toda la promoción, a una tipología o a viviendas concretas. Por eso dos compradores de la misma promoción no tienen necesariamente las mismas opciones.

**Qué demostrar:** una fecha vencida, una abierta y al menos dos alcances distintos. Evitar editar datos durante este bloque.

### Demo 3 - Invitaciones y vínculo con la vivienda

**Tiempo:** 25-30 segundos.

**Sesión:** Promotora.

**Acción:** mostrar las invitaciones de las viviendas 3ºA y 3ºB de `Residencial Puerta Azul`.

**Narración propuesta:**

> La Promotora no crea manualmente la cuenta del comprador. Asigna un correo a la vivienda y Kiwbi envía un Magic Link. Aquí vemos una invitación caducada y otra todavía pendiente. El enlace permite que el comprador cree o reutilice su cuenta y quede vinculado directamente a la vivienda correcta.

**Qué demostrar:** estados de invitación. No enviar un correo real durante la grabación para no depender de un servicio externo.

### Demo 4 - Experiencia del comprador y selección con HTMX

**Tiempo:** 60-70 segundos.

**Sesión:** Comprador 4 (`buyer4@kiwbi.test`).

**Acción:** abrir `Mis viviendas`, entrar en la vivienda 2ºB de `Residencial Puerta Azul` y seleccionar una opción de `Mecanismos eléctricos`.

**Narración propuesta:**

> Cambio ahora al punto de vista del comprador. Solo aparecen las viviendas vinculadas a su cuenta. Dentro de la vivienda, las personalizaciones están agrupadas por gremio y muestran sus opciones y sobrecostes. Las partidas vencidas son de solo lectura. Electricidad, en cambio, sigue abierta, así que puedo realizar una elección. La actualización se hace mediante HTMX, sin recargar toda la página, pero la validación del plazo y de la aplicabilidad también se realiza en el servidor.

**Qué demostrar:**

- Separación de datos por comprador.
- Personalizaciones agrupadas por gremio.
- Gremio vencido bloqueado.
- Gremio abierto editable.
- Selección visible sin recarga completa.

### Demo 5 - Seguimiento desde la Promotora

**Tiempo:** 40-45 segundos.

**Sesión:** Promotora.

**Acción:** volver a `Residencial Puerta Azul`, abrir Progreso y refrescar para mostrar la elección recién realizada.

**Narración propuesta:**

> La elección ya forma parte del seguimiento de la Promotora. El panel resume, por vivienda, cuántas personalizaciones siguen pendientes y cuántas están seleccionadas, confirmadas o pagadas. Al entrar en el detalle puede revisar la opción efectiva y actuar sobre ella según el estado y el plazo del gremio.

**Qué demostrar:** conexión real entre ambos roles. No explicar cada fila de la tabla.

### Demo 6 - Vencimiento, opción por defecto y confirmación

**Tiempo:** 45-55 segundos.

**Sesión:** Promotora.

**Acción:** abrir `Residencial Vistalar`, entrar en Progreso, acceder a la vivienda 1ºB y confirmar `Armario empotrado`, que está pendiente dentro de un gremio vencido.

**Narración propuesta:**

> Este segundo caso muestra una regla importante. El plazo de Carpintería ya ha vencido y el comprador no realizó esta elección. Kiwbi presenta la opción por defecto como opción efectiva y permite a la Promotora confirmarla. De este modo, una falta de respuesta no deja una instrucción ambigua cuando llega el momento de cerrar el gremio.

**Qué demostrar:** pulsar `Confirmar` y mostrar el cambio de estado. No es necesario marcar también como pagada; puede mencionarse como siguiente transición disponible.

### Demo 7 - Libro de Obra

**Tiempo:** 40-45 segundos.

**Sesión:** Promotora.

**Acción:** exportar el reporte de `Residencial Vistalar` en PDF, abrirlo y desplazarse brevemente por su estructura.

**Narración propuesta:**

> Cuando la Promotora necesita trasladar el resultado a obra, puede exportar el Libro de Obra. El documento agrupa la información por gremio y vivienda e incluye la personalización, la opción efectiva, el sobrecoste y su estado. El mismo reporte también está disponible en Excel para facilitar el trabajo operativo. La constructora no accede directamente al sistema en este MVP; recibe un resultado consolidado y trazable.

**Qué demostrar:** agrupación por gremio y vivienda. Mostrar solo el PDF; mencionar Excel evita duplicar tiempo sin perder alcance funcional.

**Transición al cierre ante cámara:**

> Este Libro de Obra cierra el recorrido: la elección del comprador ya se ha convertido en información consolidada y preparada para su uso en obra.

Mantener el PDF visible durante esta frase y cortar después al plano de cámara. No introducir una pantalla final entre ambos bloques.

---

## 7. Cierre ante cámara

**Tiempo:** 25-30 segundos.

**Plano:** recuperar el mismo encuadre de la introducción y hablar directamente a cámara. La continuidad visual debe hacer evidente que se cierra el recorrido iniciado al principio.

**Narración propuesta:**

> Con Kiwbi he conectado la configuración de la Promotora, la decisión del Comprador y la documentación para obra en un único circuito. Para mí, este proyecto ha sido una forma de llevar a la práctica lo aprendido durante el máster y convertirlo en una aplicación real. El siguiente paso será validarlo con profesionales del sector y adaptarlo a la operativa real de las promociones inmobiliarias.

> Muchas gracias por vuestro tiempo. Espero que os haya gustado. Un saludo.

## 8. Qué no conviene enseñar en la demo principal

Estas capacidades pueden mencionarse o dejarse para preguntas, pero no aportan suficiente valor narrativo para consumir tiempo en la demostración principal:

- Registro y login completos.
- Recuperación de contraseña.
- Todos los formularios CRUD.
- Edición del branding de la Promotora.
- Subida de cada tipo de imagen o plano.
- Reenvío y cancelación real de correos durante la grabación.
- Exportación de PDF y Excel por separado.
- Paneles de Render, Neon, Cloudflare, Brevo, GitHub Actions o SonarCloud.
- Recorrido exhaustivo por las tres promociones.

Estas funciones ayudan a demostrar madurez del proyecto, pero la defensa debe priorizar el circuito de negocio.

## 9. Plan de contingencia para la grabación

- Grabar todas las interacciones contra `https://kiwbi.onrender.com` y mantener visible la URL al inicio de la demo.
- Despertar la instancia de Render y comprobar el recorrido completo antes de comenzar a capturar.
- Reservar la confirmación irreversible de `Armario empotrado` para su bloque definitivo; grabar primero este bloque si existe cualquier duda sobre tener que repetir la sesión.
- Tener identificado un segundo caso de datos publicado para repetir las acciones reversibles, sin recurrir a una ejecución local.
- Si el entorno público no está disponible, posponer la grabación; no reemplazar la demostración por una versión local.
- Hacer una grabación de prueba solo de la demo para comprobar tiempos, zoom y legibilidad.
- Si una interacción falla, repetir la toma desde el último bloque siempre que el estado publicado lo permita; el vídeo final no necesita ser una única toma continua.

## 10. Afirmaciones que deben mantenerse precisas

- Presentar Kiwbi como **MVP funcional pendiente de validación con el sector**, no como producto validado comercialmente.
- Explicar que la necesidad parte de carencias trasladadas por contactos profesionales relacionados con promotoras, sin convertir ese contraste inicial en una validación formal de mercado.
- Hablar de riesgos del proceso manual, no de porcentajes de mejora todavía no medidos.
- Indicar que la confirmación y el pago son estados manuales; Kiwbi no procesa pagos.
- Indicar que la constructora recibe el reporte; no tiene acceso directo en el MVP.
- Indicar que el bloqueo se produce por la fecha límite del gremio.
- Explicar que las opciones pueden asignarse por promoción, tipología o vivienda.
- Diferenciar el uso de IA durante el desarrollo de una supuesta funcionalidad de IA dentro del producto: Kiwbi no incorpora IA como función de negocio en este MVP.

## 11. Decisiones validadas para esta versión

Esta propuesta ya incorpora las siguientes decisiones:

1. No existe un límite de duración indicado; se prioriza una presentación breve y directa de 10-12 minutos.
2. Toda la narración se realiza en primera persona singular.
3. El origen del problema puede apoyarse en carencias trasladadas por contactos profesionales relacionados con promotoras.
4. La defensa se centra en el aprendizaje y el desarrollo del TFM, no en vender el producto.
5. La denominación estable es **MVP funcional pendiente de validación con el sector**.
6. La introducción y el cierre muestran al autor ante cámara; las slides y la demostración muestran únicamente la pantalla.
7. El vídeo se graba por bloques y se monta posteriormente con herramientas gratuitas; no se plantea como una toma única.
8. Toda la demostración se realiza sobre la aplicación pública desplegada; el entorno local no se utiliza como sustituto durante la grabación.

## 12. Fuentes contrastadas para esta propuesta

- `.ai-docs/01-business-context.md`: problema, roles y alcance del MVP.
- `.ai-docs/02-architecture-and-stack.md`: arquitectura y stack base.
- `.ai-docs/03-domain-model.md`: jerarquía funcional y reglas de negocio.
- `.ai-docs/04-ai-coding-guidelines.md`: metodología de trabajo y validación.
- `.ai-docs/05-development-roadmap.md`: funcionalidades implementadas y evolución del proyecto.
- `.ai-docs/Documentacion-TFM-2.md`: requisitos formales de slides, vídeo y captura de pantalla.
- `.ai-docs/demo-data-guide.md`: estados y recorrido reproducible de la demostración.
- `README.md`: estado actual, despliegue, infraestructura y número de tests.
- Código de `Kiwbi.Domain`, `Kiwbi.Application`, `Kiwbi.Infrastructure` y `Kiwbi.Web`: contraste de reglas, casos de uso, adaptadores y flujo web descritos en la documentación.

El contenido exacto de los slides y las instrucciones de generación quedan definidos en `presentation-assets/prompt-gemini-pro-slides.md`.