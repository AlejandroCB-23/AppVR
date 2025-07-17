# App-RV

**App-RV** es una herramienta en realidad virtual diseñada para la recolección y análisis de datos oculares durante la ejecución de tareas cognitivas. Desarrollada en Unity y optimizada para el dispositivo HTC Vive Focus 3, esta aplicación implementa un juego serio basado en el paradigma **Go/No-Go**, orientado a contextos de evaluación cognitiva, investigación y análisis del comportamiento visual.

---

## 🧠 ¿Qué hace esta app?

La aplicación integra:

- **Seguimiento ocular en tiempo real** utilizando las capacidades del HTC Vive Focus 3.
- Un juego **Go/No-Go en VR**, donde el usuario debe responder o inhibirse ante estímulos visuales.
- **Registro automático de métricas visuales** como la dirección de la mirada y las posiciones de los ojos.
- Compatibilidad con una herramienta externa de **visualización de mapas de calor**, que representa zonas de atención visual, patrones de exploración y puntos de fijación. Para acceder a ella: https://github.com/AlejandroCB-23/App_Mapas_Calor.git
- Exportación de los datos recolectados para su análisis posterior con la utilización de la app mencionada en el punto anterior.

Esta solución permite evaluar el comportamiento atencional y el control inhibitorio de forma inmersiva y precisa, siendo útil tanto en entornos clínicos como de investigación.

---

## 🛠 Instalación

Sigue estos pasos para instalar y ejecutar la aplicación:

1. **Clona la versión más actualizada de este repositorio**  


2. **Abre el proyecto en Unity**, usando la versión especificada (recomendado: `Unity 2022.3.57f1`).

3. **Elimina los objetos `Main Camera` y `Directional Light`:**  
- Arrastra las escenas desde `Assets/Scenes` al panel `Hierarchy`.  
- Elimina la escena por defecto haciendo clic en los tres puntos (`⋮`) a la derecha del nombre y seleccionando `Remove Scene`.

4. **Activa el seguimiento ocular:**  
- Ve a `Edit → Project Settings → XR Plug-in Management`.  
- En la pestaña `Android`, selecciona `WaveXR`.

5. **Configura las opciones de WaveXR en `WaveXRSettings`:**  
Activa las siguientes características:  
- `Tracker`  
- `Natural Hand`  
- `Eye Tracking`  
- `Eye Expression`  
- `Lip Expression`  
- `Body Tracking`  
- `Scene Perception`  
- `Scene Mesh`  
- `Marker`

6. **(Opcional)** Si usas la app externa para visualizar datos, configura la dirección IP en los siguientes objetos:  
- Escena `Menu` → Objeto `GazeMenu`  
- Escena `ModoAleatorio` → Objetos `GameManagerModoAleatorio`, `EyeTracker`  
- Escena `ModoTest` → Objetos `GameManager`, `EyeTracker`

7. **Conecta el dispositivo VR al ordenador mediante cable USB.**

8. **Compila y ejecuta el proyecto:**  
- Ve a `File → Build Settings`  
- Cambia la plataforma a `Android`  
- Haz clic en `Build And Run`


