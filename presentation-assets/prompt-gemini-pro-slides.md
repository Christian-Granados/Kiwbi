# Prompt definitivo para generar los slides de Kiwbi en Gemini Pro

## Preparación

Adjuntar junto al prompt el archivo `src/Kiwbi.Web/wwwroot/apple-touch-icon.png` como isotipo oficial de Kiwbi.

No adjuntar las capturas de `presentation-assets/screenshots/`: la interfaz se mostrará después mediante una demostración grabada sobre la aplicación pública y no debe repetirse en los slides.

Copiar en Gemini Pro únicamente el contenido comprendido entre `INICIO DEL PROMPT` y `FIN DEL PROMPT`.

---

## INICIO DEL PROMPT

Actúa como diseñador editorial senior especializado en presentaciones académicas de proyectos tecnológicos.

Genera una presentación completa, editable y lista para exponer del Trabajo Fin de Máster **Kiwbi**, realizado por **Christian Granados** para el **Máster en Desarrollo con IA de BigSchool**.

Debes ejecutar exactamente la especificación siguiente. No propongas alternativas, no hagas preguntas, no reorganices el relato y no tomes decisiones adicionales de contenido.

# 1. Formato obligatorio

- Crea **exactamente 8 diapositivas**, ni una más ni una menos.
- Relación de aspecto: **16:9 panorámica**.
- Idioma: **español de España**.
- Duración prevista de esta parte: **4-5 minutos**.
- La presentación precede a una demostración en vivo de la aplicación pública.
- Incluye las notas del presentador indicadas para cada diapositiva. Las notas no deben aparecer como texto visible dentro de la diapositiva.
- El resultado debe ser editable y exportable a PowerPoint o Google Slides.
- No añadas una diapositiva final de agradecimiento: después de las slides se mostrará la demo y el cierre del vídeo se grabará ante cámara.
- No añadas bibliografía, agenda, índice ni numeración de secciones.
- Añade únicamente un número de diapositiva discreto del 1 al 8 en la esquina inferior derecha.

# 2. Identidad visual obligatoria

Usa el isotipo de Kiwbi adjunto sin modificar su forma ni sus colores.

Paleta exacta:

- Fondo principal: `#F8FAFC`.
- Superficies: `#FFFFFF`.
- Texto principal: `#0F172A`.
- Texto secundario: `#64748B`.
- Bordes y líneas: `#E2E8F0`.
- Verde principal Kiwbi: `#0F766E`.
- Verde de apoyo: `#2DD4BF`.
- Ámbar para alertas o vencimientos: `#D97706`.
- Azul para selección: `#0EA5E9`.
- Violeta para confirmación: `#6366F1`.
- Verde para pago completado: `#10B981`.

Tipografía:

- Usa **Inter** en toda la presentación.
- Títulos: 30-34 pt, peso 600 o 700.
- Texto principal: 19-22 pt.
- Etiquetas auxiliares: nunca menos de 15 pt.
- No uses texto condensado, cursivas decorativas ni mayúsculas sostenidas en párrafos.

Estilo:

- Sobrio, contemporáneo, técnico y académico.
- Mucho espacio en blanco y jerarquía visual clara.
- Formas geométricas sencillas, líneas finas e iconos lineales consistentes.
- Radio máximo de esquinas: 8 px.
- Usa el verde Kiwbi como acento, no como fondo dominante en todas las diapositivas.
- Combina fondos blancos y gris muy claro para evitar una presentación monocromática.
- Usa los colores de estado únicamente cuando tengan significado funcional.
- Mantén una retícula, márgenes y posición del título consistentes en las diapositivas 2-8.
- Sin animaciones, transiciones llamativas ni elementos decorativos sin función.

# 3. Prohibiciones

- No inventes métricas, porcentajes, testimonios, cifras de negocio, ahorros ni datos de mercado.
- No presentes Kiwbi como producto validado, empresa consolidada o solución comercial terminada.
- No uses la expresión “PoC”. La denominación correcta es **MVP funcional pendiente de validación con el sector**.
- No afirmes que Kiwbi incorpora inteligencia artificial como funcionalidad del producto. La IA se ha utilizado durante el proceso de desarrollo.
- No conviertas la presentación en un pitch comercial ni incluyas precios, mercado potencial, competidores o llamadas a inversión.
- No uses capturas de la aplicación. La interfaz se enseñará inmediatamente después en una demostración real.
- No uses mockups de portátiles, móviles o navegadores.
- No uses imágenes de bancos de recursos con personas posando, apretones de manos o equipos mirando una pantalla.
- No uses ilustraciones 3D, dibujos infantiles, personajes, bombillas, cohetes, dianas, piezas de puzle ni estética de plantilla corporativa genérica.
- No uses fondos morados, azules oscuros dominantes, degradados decorativos, sombras intensas ni grandes bloques de texto.
- No añadas logotipos de tecnologías o proveedores. Escribe sus nombres como texto.
- No alteres, resumas ni amplíes el texto visible especificado para cada diapositiva.
- No muestres las notas del presentador dentro de las diapositivas.

# 4. Estructura exacta por diapositiva

## Diapositiva 1 - Portada

**Título visible exacto:**

Kiwbi

**Subtítulo visible exacto:**

De la elección del comprador a una instrucción de obra clara y trazable

**Pie visible exacto:**

Christian Granados · Trabajo Fin de Máster · Máster en Desarrollo con IA · BigSchool

**Composición obligatoria:**

- Usa una composición a sangre completa.
- Coloca a la derecha una imagen original y fotorrealista de un interior de vivienda contemporánea todavía en construcción, con muestras reales de suelo y acabados sobre un plano arquitectónico en primer término.
- No deben aparecer personas, texto incrustado, marcas comerciales ni maquinaria pesada.
- Aplica una zona limpia clara en el lado izquierdo para el título y subtítulo; no encierres el texto en una tarjeta.
- Coloca el isotipo de Kiwbi encima del título, pequeño pero reconocible.
- El título debe ser el elemento tipográfico dominante.

**Notas del presentador exactas:**

He desarrollado Kiwbi para abordar un momento muy concreto de la preventa de vivienda: la elección de acabados y extras durante la construcción. A primera vista parece un proceso sencillo, como escoger un suelo o una grifería. Pero cada elección debe terminar convertida en una instrucción de obra correcta, aplicable a una vivienda concreta, recibida dentro de plazo y preparada para que la constructora pueda ejecutarla. Y ahí es donde una elección aparentemente sencilla se convierte en un problema de coordinación.

## Diapositiva 2 - El problema

**Título visible exacto:**

El proceso actual está fragmentado

**Frase de apoyo visible exacta:**

La dificultad no está solo en elegir: está en coordinar, actualizar y consolidar.

**Etiquetas visibles exactas:**

- Promotora
- Comprador
- Constructora
- Correos
- Hojas de cálculo
- Documentos
- Revisiones manuales
- Trazabilidad frágil

**Composición obligatoria:**

- Construye un diagrama horizontal de izquierda a derecha.
- Sitúa `Promotora` a la izquierda, `Comprador` en la parte superior derecha y `Constructora` en la parte inferior derecha.
- Entre ellos, representa cuatro canales separados: `Correos`, `Hojas de cálculo`, `Documentos` y `Revisiones manuales`.
- Usa líneas cruzadas y discontinuas para mostrar fragmentación, sin hacer el gráfico ilegible.
- Termina el flujo en una banda ámbar muy clara con el texto `Trazabilidad frágil`.
- No uses una captura ni una fotografía en esta diapositiva.

**Notas del presentador exactas:**

La idea parte de carencias que me han trasladado contactos profesionales relacionados con promotoras. En este proceso, la información puede quedar repartida entre hojas de cálculo, correos y documentos actualizados manualmente. La Promotora debe definir opciones y fechas, recibir respuestas, comprobar sobrecostes y consolidar el resultado para obra. El problema no es una herramienta aislada, sino mantener sincronizadas muchas decisiones y conservar su trazabilidad.

## Diapositiva 3 - Complejidad del dominio

**Título visible exacto:**

La complejidad detrás de una elección

**Cadena central visible exacta:**

Promotora → Promoción → Tipologías y viviendas → Gremios → Personalizaciones → Opciones → Elecciones

**Condicionantes visibles exactos:**

- Alcance: promoción, tipología o vivienda
- Fecha límite por gremio
- Opción por defecto y sobrecoste
- Estado: pendiente, seleccionada, confirmada o pagada

**Composición obligatoria:**

- Representa la cadena central como siete nodos conectados y numerados, ocupando el centro de la diapositiva.
- Distribuye los cuatro condicionantes alrededor de la cadena como llamadas externas conectadas a los nodos relevantes.
- Destaca `Elecciones` con el verde principal Kiwbi.
- Usa pequeños iconos lineales coherentes para edificio, vivienda, herramientas, opciones, calendario, euro y estado.
- Evita cajas de texto extensas o diagramas circulares.

**Notas del presentador exactas:**

Una opción no siempre está disponible para todas las viviendas. Puede aplicarse a una promoción completa, solo a una tipología o únicamente a viviendas concretas. Además, cada personalización pertenece a un gremio con su propia fecha límite, puede incluir una opción por defecto y puede tener sobrecoste. Después, cada elección avanza por distintos estados: pendiente, seleccionada, confirmada y pagada. El sistema debe resolver todas estas combinaciones sin mostrar opciones incorrectas ni permitir cambios fuera de plazo. Kiwbi convierte estas reglas dispersas en un único circuito digital.

## Diapositiva 4 - Propuesta B2B2C

**Título visible exacto:**

Un único circuito para Promotora y Comprador

**Carril superior visible exacto:**

Promotora: Configura → Invita → Supervisa → Confirma → Exporta

**Carril inferior visible exacto:**

Comprador: Accede → Consulta → Compara → Elige

**Banda central visible exacta:**

Kiwbi aplica alcance · plazos · opción por defecto · trazabilidad

**Composición obligatoria:**

- Crea dos carriles horizontales paralelos.
- El carril de la Promotora debe quedar arriba y avanzar de izquierda a derecha en cinco pasos.
- El carril del Comprador debe quedar abajo y avanzar de izquierda a derecha en cuatro pasos.
- Entre ambos, coloca una banda continua verde muy clara con el isotipo y el texto central de Kiwbi.
- Conecta los dos carriles con la banda central mediante líneas verticales finas.
- El resultado debe comunicar coordinación, no una simple lista de funcionalidades.

**Notas del presentador exactas:**

Mi propuesta es una aplicación B2B2C. La Promotora configura sus promociones, viviendas, gremios, personalizaciones y opciones; invita al comprador mediante un enlace seguro; y supervisa el progreso de cada vivienda. El Comprador accede a un portal con su vivienda y ve únicamente las opciones que le corresponden. Kiwbi conecta ambos recorridos, aplica las reglas y conserva un estado único del proceso.

## Diapositiva 5 - Alcance

**Título visible exacto:**

Qué cubre el MVP

**Recorrido principal visible exacto:**

1. Catálogo inmobiliario
2. Personalizaciones
3. Invitación por Magic Link
4. Selección dentro de plazo
5. Seguimiento y estados manuales
6. Libro de Obra en PDF y Excel

**Bloque inferior visible exacto:**

Fuera del MVP

- Pasarela de pago
- Acceso directo para la constructora
- Incidencias posteriores a la entrega

**Composición obligatoria:**

- Representa los seis puntos del recorrido como una única línea horizontal numerada.
- Cada etapa debe tener un icono lineal y un máximo de dos líneas de texto.
- Une las etapas con una línea verde continua que termine en un icono de documento.
- Separa el bloque `Fuera del MVP` en una franja inferior gris clara con tres elementos tachados de forma sutil.
- El bloque fuera de alcance debe ser secundario, pero claramente legible.

**Notas del presentador exactas:**

El MVP cubre el circuito desde la configuración de una promoción hasta la generación del Libro de Obra. Incluye promociones, tipologías, viviendas, gremios con fecha límite, opciones con sobrecoste, invitaciones por Magic Link, selección del comprador, seguimiento por vivienda y exportación final agrupada por gremio. No incluye una pasarela de pago, acceso directo para la constructora ni gestión de incidencias posteriores a la entrega. Los estados de confirmación y pago se gestionan manualmente por la Promotora.

## Diapositiva 6 - Reglas del núcleo

**Título visible exacto:**

Del plazo a una instrucción de obra

**Textos visibles exactos del diagrama:**

- ¿Gremio abierto?
- Sí
- El comprador puede elegir
- Opción seleccionada
- Al vencer: selección bloqueada
- No
- Selección bloqueada
- ¿Existe una elección previa?
- Sí: se conserva
- No: se aplica la opción por defecto
- Opción efectiva
- La Promotora confirma
- Pendiente
- Seleccionada
- Confirmada
- Pagada

**Composición obligatoria:**

- Construye un diagrama de decisión de izquierda a derecha.
- Comienza con `¿Gremio abierto?`.
- Rama superior `Sí`: `El comprador puede elegir` → `Opción seleccionada` → `Al vencer: selección bloqueada`.
- Rama inferior `No`: `Selección bloqueada` → `¿Existe una elección previa?` → `Sí: se conserva` / `No: se aplica la opción por defecto`.
- Une ambas ramas en `Opción efectiva` y, desde ahí, continúa hacia `La Promotora confirma`.
- En la zona inferior, muestra la secuencia de estados `Pendiente → Seleccionada → Confirmada → Pagada` usando gris, azul, violeta y verde respectivamente.
- Usa ámbar únicamente para la rama de plazo vencido.
- El diagrama debe ser comprensible en menos de cinco segundos.

**Notas del presentador exactas:**

El núcleo de Kiwbi no es el CRUD ni el aspecto visual. Es el motor que decide qué personalización corresponde a cada vivienda y qué ocurre cuando vence un plazo. Mientras un gremio está abierto, el comprador puede elegir. Cuando vence, la selección queda bloqueada y, si no existe una elección, la opción por defecto pasa a ser la opción efectiva. La Promotora puede entonces confirmar la decisión y marcarla como pagada cuando corresponda. Así, el estado final puede convertirse en un documento operativo para obra.

## Diapositiva 7 - Construcción técnica

**Título visible exacto:**

Cómo lo he construido

**Capas visibles exactas:**

- Web — ASP.NET Core MVC · Razor · HTMX
- Application — Casos de uso
- Domain — Entidades y reglas de negocio
- Infrastructure — EF Core · Identity · reportes · correo · almacenamiento

**Infraestructura visible exacta:**

PostgreSQL · Docker · Render · Neon · Cloudflare R2 · Brevo

**Indicadores visibles exactos:**

- 325 tests automatizados
- Integración continua
- Análisis estático
- Desplegado y operativo

**URL visible exacta:**

kiwbi.onrender.com

**Composición obligatoria:**

- Divide el espacio útil bajo el título en tres zonas visuales, sin escribir sus nombres: diagrama de arquitectura a la izquierda (70 % del ancho y 68 % de la altura), indicadores de calidad a la derecha (26 % del ancho y 68 % de la altura) y banda de despliegue debajo del diagrama (70 % del ancho y 20 % de la altura).
- Mantén al menos un 4 % de separación entre el diagrama y el panel derecho, y un 3 % entre el diagrama y la banda inferior.
- No añadas subtítulos como `Arquitectura lógica`, `Calidad`, `Negocio`, `Adaptadores` o `Ejecución en producción`.

**Geometría exacta de la arquitectura lógica:**

- Usa un lienzo rectangular sin borde exterior. No encierres todo el diagrama en otra tarjeta.
- Organiza cuatro módulos en tres columnas: `Web` a la izquierda; `Application` y `Domain` apilados en el centro; `Infrastructure` a la derecha.
- `Web`: rectángulo vertical de aproximadamente 27 % del ancho del diagrama y 72 % de su altura, centrado verticalmente.
- `Application`: rectángulo horizontal de aproximadamente 34 % del ancho y 29 % de la altura, situado en el centro superior.
- `Domain`: rectángulo horizontal del mismo ancho que Application y 29 % de la altura, situado inmediatamente debajo con una separación equivalente al 14 % de la altura del diagrama.
- `Infrastructure`: rectángulo vertical de aproximadamente 31 % del ancho y 72 % de la altura, centrado verticalmente.
- Los cuatro módulos deben tener fondo blanco, borde de 1,5 px y radio de 8 px. Usa borde `#0F766E` para Web, Application y Domain, y `#94A3B8` para Infrastructure.
- Destaca únicamente Domain con fondo `#ECFDF5` y borde de 2 px. Los demás módulos permanecen blancos.
- Dentro de cada módulo, separa visualmente el nombre de la capa y su contenido sin añadir texto: nombre arriba en 18-20 pt y semibold; descripción debajo en 15-16 pt, color `#64748B`, máximo dos líneas y centrada.
- Usa exactamente estas dos partes dentro de cada módulo, separando por el guion largo del texto suministrado: `Web` / `ASP.NET Core MVC · Razor · HTMX`; `Application` / `Casos de uso`; `Domain` / `Entidades y reglas de negocio`; `Infrastructure` / `EF Core · Identity · reportes · correo · almacenamiento`.
- No dibujes una capa dentro de otra y no uses círculos concéntricos: son cuatro proyectos separados.

**Conexiones exactas entre proyectos:**

- Dibuja solo cuatro flechas sólidas, sin texto ni leyenda, en color `#0F766E`, grosor 2 px y punta triangular pequeña.
- Flecha 1: desde el centro del borde derecho de Web hasta el centro del borde izquierdo de Application.
- Flecha 2: vertical desde el centro del borde inferior de Application hasta el centro del borde superior de Domain.
- Flecha 3: desde el tercio superior del borde izquierdo de Infrastructure hasta el centro del borde derecho de Application.
- Flecha 4: línea ortogonal desde el tercio inferior del borde izquierdo de Infrastructure hasta el centro del borde derecho de Domain.
- Las flechas 3 y 4 deben salir de puntos distintos del borde de Infrastructure y no deben cruzarse.
- No dibujes la referencia de composición `Web → Infrastructure`, porque añadiría otra conexión visual sin aportar valor a la explicación oral.
- No dibujes ninguna flecha saliente desde Domain ni ninguna conexión de Web con servicios externos.

**Banda de ejecución en producción:**

- Crea una banda horizontal con fondo `#F1F5F9`, borde `#E2E8F0` y radio de 8 px.
- Centra dentro de la banda una única línea con el texto exacto `PostgreSQL · Docker · Render · Neon · Cloudflare R2 · Brevo`, en 15-16 pt y color `#475569`.
- Coloca `kiwbi.onrender.com` debajo de esa línea, centrado, en 16-18 pt, semibold y color `#0F766E`.
- No conviertas cada proveedor en una tarjeta, no añadas logotipos y no dibujes conexiones en esta banda.

**Panel de calidad:**

- Usa un único panel vertical blanco con borde `#E2E8F0`, radio de 8 px y cuatro filas de igual altura separadas por líneas horizontales finas.
- Primera fila: `325` en 30-34 pt y semibold; `tests automatizados` debajo en 15 pt.
- Segunda fila: `Integración continua`, centrado verticalmente y acompañado por un icono lineal de check.
- Tercera fila: `Análisis estático`, centrado verticalmente y acompañado por un icono lineal de escudo.
- Cuarta fila: `Desplegado y operativo`, centrado verticalmente y acompañado únicamente por un punto verde de 8 px.
- No añadas explicaciones, nombres de herramientas ni títulos dentro del panel.

**Restricciones específicas del diagrama:**

- La diapositiva debe contener únicamente el título, los cuatro módulos, las cuatro flechas, la banda de infraestructura, la URL y los cuatro indicadores.
- Las flechas representan dependencias entre proyectos, no el recorrido temporal de una petición, pero esta distinción no debe añadirse como texto visible.
- Mantén legibles los nombres `Web`, `Application`, `Domain` e `Infrastructure`; no añadas el prefijo `Kiwbi.` en los módulos.
- No uses logotipos de proveedores, fragmentos de código, capturas, cilindros 3D, sombras intensas ni efectos de profundidad.
- No añadas tecnologías o relaciones distintas de las descritas.

**Notas del presentador exactas:**

He desarrollado la aplicación con .NET 10 y ASP.NET Core MVC siguiendo Clean Architecture. He separado las reglas de dominio, los casos de uso y las integraciones técnicas para mantener responsabilidades claras. En la interfaz he utilizado Razor y HTMX para conseguir interacciones dinámicas sin incorporar una SPA. También he desplegado la solución con Docker y servicios cloud, y he acompañado el desarrollo con 325 tests automatizados, integración continua y análisis estático.

## Diapositiva 8 - Aprendizaje, IA y siguiente paso

**Título visible exacto:**

IA como herramienta, decisiones como responsabilidad

**Ciclo visible exacto:**

Contexto y reglas → Diseño por Epics → Implementación → Tests y revisión → Validación manual → Documentación

**Mensaje central visible exacto:**

MVP funcional pendiente de validación con el sector

**Siguiente paso visible exacto:**

Contrastar con profesionales · detectar necesidades no cubiertas · adaptar el flujo

**Frase final visible exacta:**

La IA ha acelerado el trabajo; las decisiones, la validación y la responsabilidad han sido mías.

**Composición obligatoria:**

- Divide la diapositiva en dos zonas asimétricas: 60 % para el ciclo de trabajo y 40 % para resultado y siguiente paso.
- Representa el ciclo como seis pasos conectados, sin usar un círculo genérico ni flechas decorativas.
- En la zona derecha, destaca el mensaje central dentro de una superficie blanca con borde verde.
- Debajo, coloca el siguiente paso con una flecha que continúe hacia fuera de la diapositiva, indicando evolución futura.
- Sitúa la frase final en una banda inferior limpia; debe poder leerse mientras comienza la transición a la demostración.
- Esta diapositiva debe cerrar la parte conceptual y enlazar visualmente con el inicio de la demostración.

**Notas del presentador exactas:**

La IA ha formado parte de mi proceso de desarrollo, pero no ha sustituido mis decisiones. He organizado el proyecto por Epics y he documentado primero el contexto, las reglas, el impacto técnico y los criterios de aceptación. Después he utilizado la IA para explorar soluciones, implementar, revisar y mantener la trazabilidad, contrastando cada avance con tests y verificaciones manuales. El resultado es un MVP funcional pendiente de validación con el sector. Mi siguiente paso será probarlo con profesionales, detectar necesidades no cubiertas y adaptarlo a su operativa real. Hasta aquí he explicado el circuito. Ahora voy a mostrar cómo una decisión pasa del portal del comprador al control de la Promotora y termina preparada para obra.

# 5. Comprobación obligatoria antes de entregar

Antes de generar el resultado final, verifica internamente que:

1. Hay exactamente 8 diapositivas.
2. Todos los textos visibles coinciden literalmente con esta especificación.
3. Las notas del presentador están separadas del contenido visible.
4. No se ha añadido ninguna métrica aparte de `325 tests automatizados`.
5. No aparece la palabra `PoC`.
6. No hay capturas ni mockups de la aplicación.
7. La diapositiva 6 representa correctamente las dos ramas de la fecha límite.
8. La diapositiva 7 contiene exactamente cuatro flechas: `Web → Application`, `Application → Domain`, `Infrastructure → Application` e `Infrastructure → Domain`; Domain no tiene flechas salientes y no existe acceso directo de Web a la base de datos.
9. La presentación usa el isotipo adjunto y la paleta indicada.
10. El contenido cabe sin desbordamientos y puede leerse en una grabación a 1080p.
11. No hay decisiones de diseño o contenido pendientes de resolver.

Entrega directamente la presentación terminada. No incluyas explicaciones, alternativas ni recomendaciones fuera de las diapositivas.

## FIN DEL PROMPT
