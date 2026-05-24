[README.txt](https://github.com/user-attachments/files/28200856/README.txt)
# InstaHistory – Experiencia Multimedia Interactiva

## Descripción general

**InstaHistory** es una experiencia multimedia interactiva desarrollada en **Unity**, enfocada en la exploración cultural e histórica mediante mecánicas de interacción, narrativa audiovisual y accesibilidad. El proyecto busca ofrecer una experiencia inmersiva donde el usuario puede recorrer distintos módulos temáticos relacionados con elementos arqueológicos y rituales históricos.

La aplicación integra controles compatibles con **gamepad**, narraciones accesibles, métricas de interacción y múltiples sistemas visuales diseñados bajo una arquitectura modular basada en el patrón **MVC (Model - View - Controller)**.

El sistema está dividido en tres módulos principales:

* **Módulo de Exploración**
* **Módulo Puzzle**
* **Módulo Ritual**

Cada módulo presenta dinámicas distintas de interacción, aprendizaje y exploración audiovisual.

---

# Arquitectura general del proyecto

El proyecto sigue una arquitectura modular basada en el patrón **MVC**, permitiendo separar la lógica de negocio, la representación visual y el control de interacciones del usuario.

## Componentes principales

### Model

Gestiona la lógica interna y el estado de cada módulo:

* Datos de interacción
* Estados de progreso
* Mecánicas de juego
* Configuraciones de accesibilidad
* Variables de rotación, zoom, iluminación y puzzles

### View

Responsable de:

* Interfaz gráfica
* Feedback visual
* Reproducción audiovisual
* Paneles tutoriales
* Efectos visuales y partículas
* Indicadores de progreso

### Controller

Administra:

* Entradas del usuario
* Comunicación entre Model y View
* Flujo de interacción
* Eventos de accesibilidad
* Navegación mediante gamepad

---

# Módulo de Exploración

## Descripción

El módulo de exploración permite al usuario inspeccionar objetos históricos en un entorno interactivo 3D. El usuario puede:

* Rotar objetos
* Hacer zoom
* Ajustar iluminación
* Modificar contraste
* Alternar modos de visualización
* Escuchar narraciones explicativas

El sistema incluye tutoriales progresivos y soporte completo para controles de consola.

---

## Funcionalidades principales

### Interacción con objetos

El usuario puede manipular objetos históricos mediante:

* Rotación libre
* Zoom dinámico
* Control de iluminación
* Ajuste de contraste

Implementado mediante:

* `ObjectController`
* `ObjectModel`
* `ObjectView`

---

### Sistema de tutorial interactivo

El módulo guía al usuario mediante paneles secuenciales que enseñan:

1. Rotación
2. Zoom
3. Iluminación
4. Cambio de modo
5. Ajustes finales

El tutorial avanza automáticamente al detectar interacción del usuario.

---

### Sistema de narraciones

Incluye controles para:

* Pausar narración
* Reanudar narración
* Omitir narración

Integrado con el sistema de accesibilidad del proyecto.

---

## Tecnologías utilizadas

* Unity Input System
* Audio Sources
* Control por Gamepad
* Arquitectura MVC
* Iluminación dinámica en Unity

---

# Módulo Puzzle

## Descripción

El módulo Puzzle consiste en una experiencia de ensamblaje interactivo donde el usuario debe reconstruir piezas arqueológicas mediante mecánicas de selección, movimiento y posicionamiento.

El sistema incluye:

* Cursor controlado por gamepad
* Detección de piezas
* Snap automático
* Narraciones contextuales
* Feedback visual y sonoro
* Registro de métricas

---

## Mecánicas principales

### Selección y movimiento de piezas

El usuario puede:

* Seleccionar piezas
* Arrastrarlas en el entorno
* Rotarlas
* Soltarlas en posiciones válidas

El sistema valida automáticamente la posición correcta mediante distancias de “snap”.

---

### Sistema de pares especiales

Al completar ciertas combinaciones de piezas:

* Se reproducen narraciones específicas
* Se activan partículas visuales
* Se registran eventos métricos
* Se iluminan secciones del puzzle

---

### Sistema de métricas

El módulo registra:

* Interacciones del usuario
* Tiempo de sesión
* Etapas completadas
* Número de intentos
* Progreso general

Estas métricas son gestionadas mediante `MetricsManager`.

---

### Accesibilidad

Incluye:

* Narraciones auditivas
* Cursor visible
* Feedback sonoro
* Compatibilidad con controles de consola

---

## Componentes principales

### Scripts principales

* `PuzzleController`
* `PuzzleModel`
* `PuzzleView`
* `PieceModel`
* `PieceView`

---

## Características técnicas

* Raycasting para selección
* Movimiento interpolado
* Rotación dinámica
* Partículas de unión
* Narraciones sincronizadas
* Control mediante Input System

---

# Módulo Ritual

## Descripción

El módulo Ritual presenta una experiencia interactiva dividida en múltiples fases temáticas relacionadas con rituales históricos y simbólicos.

El módulo combina:

* Minijuegos
* Videos interactivos
* Indicadores de progreso
* Navegación por fases
* Sistema de desbloqueo

---

## Fases principales

### Fase 1 – Inhumación

Puzzle deslizante donde el usuario reorganiza piezas para completar la escena ritual.

---

### Fase 2 – Ofrenda

Minijuego de selección e intercambio de símbolos rituales.

---

### Fase 3 – Tránsito

Sistema de alineación de diales mediante rotación y navegación entre controles.

---

## Sistema de videos

Después de completar cada fase:

* Se reproduce un video narrativo
* El usuario puede omitirlo manteniendo presionado el botón correspondiente
* Se desbloquean nuevas fases

---

## Navegación e indicadores

El usuario puede:

* Abrir un indicador de progreso
* Navegar entre fases desbloqueadas
* Revisar videos previamente completados

---

## Sistema de controles

Compatible con:

* D-Pad
* Stick izquierdo
* Botones A, B, LB, RB y Y

---

## Características técnicas

### Input System avanzado

El sistema implementa:

* Deadzones analógicas
* Cooldowns de entrada
* Detección de hold
* Navegación contextual según estado

---

### Arquitectura del módulo

Scripts principales:

* `RetoRitualController`
* `RetoRitualModel`
* `RetoRitualView`

---

# Sistema de Accesibilidad

## Características implementadas

El proyecto incorpora herramientas de accesibilidad enfocadas en mejorar la experiencia para distintos perfiles de usuario.

### Funciones principales

* Narraciones descriptivas
* Controles simplificados
* Feedback visual y auditivo
* Compatibilidad con gamepad
* Tutoriales interactivos
* Opción de omitir narraciones y videos

---

# Tecnologías utilizadas

## Motor de desarrollo

* Unity Engine

## Lenguaje

* C#

## Sistemas utilizados

* Unity Input System
* Audio Sources
* Particle Systems
* Arquitectura MVC
* Raycasting
* Coroutines
* UI System

---

# Controles generales

| Acción                 | Control                 |
| ---------------------- | ----------------------- |
| Movimiento             | Stick izquierdo / D-Pad |
| Seleccionar            | Botón A                 |
| Cancelar               | Botón B                 |
| Cambiar modo           | Botón Y                 |
| Navegación adicional   | LB / RB                 |
| Pausar narración       | Configurable            |
| Omitir narración/video | Mantener botón          |

---

# Sistema de Analítica, Reportes y Exportación – InstaHistory

## Descripción general

El sistema incorpora un módulo de análisis y generación de reportes orientado a recopilar información sobre el comportamiento de los usuarios durante la interacción con la experiencia multimedia InstaHistory. Su objetivo principal es proporcionar métricas útiles para la evaluación académica e institucional del proyecto, permitiendo visualizar patrones de uso, interacción y participación dentro de la experiencia.

La implementación fue desarrollada en Unity y se integró con el panel administrativo e institucional del sistema, permitiendo consultar, procesar y exportar información automáticamente.

---

# Arquitectura general del módulo

El sistema se divide en varios componentes principales:

### Gestor de métricas

Responsable de registrar y almacenar eventos relevantes del sistema durante la ejecución de la experiencia.

Entre las funciones principales se encuentran:

* Registro de sesiones iniciadas.
* Registro de duración de sesiones.
* Conteo de interacciones realizadas.
* Registro de etapas completadas.
* Identificación de secciones más visitadas.
* Almacenamiento persistente de datos.

La persistencia de información se realiza mediante almacenamiento local utilizando PlayerPrefs.

---

### Gestor de autenticación

Permite controlar el acceso a diferentes paneles según el tipo de usuario.

Tipos de acceso implementados:

*Usuario administrativo*

Permite:

* Administrar recursos.
* Generar reportes administrativos.
* Reiniciar métricas.
* Acceder a funcionalidades internas.

*Usuario institucional*

Permite:

* Visualizar reportes académicos.
* Descargar documentos institucionales.
* Consultar estadísticas de uso.
* Exportar información.

La autenticación utiliza validación de credenciales mediante usuario y contraseña configurables.

---

### Panel institucional

Se implementó una interfaz independiente orientada a usuarios académicos o institucionales.

El panel permite:

* Generar reportes institucionales.
* Exportar información.
* Consultar estadísticas generales.
* Reiniciar métricas.
* Visualizar datos resumidos.

---

# Sistema de métricas

Actualmente el sistema recopila información relacionada con:

### Métricas de uso

* Tiempo total de uso.
* Duración promedio por sesión.
* Número de sesiones registradas.
* Número de interacciones.
* Etapas completadas.

### Métricas de navegación

* Sección más visitada.
* Uso de escenas.
* Interacción dentro de módulos específicos.

### Métricas institucionales

* Usuarios por semana.
* Usuarios por mes.
* Distribución de uso por módulos.
* Estadísticas generales de interacción.

---

# Sistema de generación de reportes PDF

Se desarrolló un generador dinámico de documentos PDF capaz de producir reportes institucionales y administrativos.

Características implementadas:

### Encabezado institucional

* Logo del proyecto.
* Logo institucional.
* Título del reporte.
* Información contextual.

### Resumen de métricas

Incluye:

* Tiempo total de uso.
* Duración promedio.
* Número de sesiones.
* Interacciones registradas.
* Etapas completadas.
* Sección más visitada.

### Interpretación académica

El reporte incluye una sección descriptiva que facilita la lectura e interpretación de resultados por parte de usuarios institucionales.

### Visualización gráfica

El sistema genera gráficos automáticos de barras para representar:

* Secciones más visitadas.
* Usuarios por semana.
* Usuarios por mes.

---

# Sistema de exportación Excel

Se implementó un sistema de generación automática de archivos XLSX compatible con Microsoft Excel.

Características:

### Generación automática de hojas

Se generan múltiples hojas con:

* Resumen de métricas.
* Datos estadísticos.
* Datos estructurados.

### Generación automática de gráficos

El sistema crea:

* Gráficos de barras para secciones visitadas.
* Gráficos de usuarios por semana.
* Gráficos de usuarios por mes.

Los gráficos se generan mediante construcción dinámica de estructuras XML compatibles con OpenXML.

### Organización automática

Los archivos generados incluyen:

* Encabezados estructurados.
* Separación por categorías.
* Etiquetas descriptivas.
* Datos organizados para análisis posterior.

---

# Persistencia de datos

La información recopilada permanece disponible entre ejecuciones del sistema utilizando almacenamiento local.

Datos almacenados:

* Número de visitantes.
* Tiempo acumulado.
* Interacciones.
* Etapas completadas.
* Información estadística.

Esto permite mantener un historial continuo de uso.

---

# Pruebas realizadas

Se realizaron pruebas funcionales enfocadas en:

### Generación de reportes

Se validó:

* Creación correcta de documentos PDF.
* Exportación correcta a Excel.
* Integridad de datos.

### Validación de métricas

Se verificó:

* Registro correcto de sesiones.
* Acumulación de interacciones.
* Conteo de etapas.

### Validación de gráficos

Se comprobó:

* Construcción automática de gráficos.
* Correcta representación visual.
* Correspondencia entre datos y visualización.

### Validación de interfaz

Se evaluó:

* Funcionamiento de botones.
* Navegación entre paneles.
* Correcta respuesta del sistema.

---

# Tecnologías utilizadas

* Unity 6
* C#
* TextMeshPro
* iText PDF
* OpenXML
* XML dinámico
* PlayerPrefs
* Sistema UI de Unity

---

# Posibles extensiones futuras

La arquitectura fue diseñada para permitir la integración de nuevas fuentes de datos y módulos adicionales sin alterar la estructura principal del sistema.

Ejemplos:

* Nuevas escenas.
* Nuevas métricas.
* Nuevos tipos de usuarios.
* Nuevos formatos de exportación.
* Dashboards avanzados.
* Reportes en línea.
