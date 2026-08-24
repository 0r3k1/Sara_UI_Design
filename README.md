# Sara UI Design 🎨

Una biblioteca de controles personalizados para **Windows Forms sobre .NET**, enfocada en diseño moderno, flexibilidad y facilidad de uso.

## ✨ Controles incluidos

- **SaraUI_FlexPanel**: panel con lógica inspirada en CSS Flexbox.
- **SaraUI_ShadowPanel**: panel con sombras proyectadas y bordes redondeados.
- **SaraUI_Button**: botón estilizado con soporte para bordes redondeados.
- **SaraUI_TextBox**: entrada con placeholder, validación e iconos.
- **SaraUI_CircularProgressBar**: progreso circular animado.
- **SaraUI_ToggleButton**: interruptor moderno de encendido y apagado.
- **SaraUI_ComboBox**: lista desplegable personalizable.
- **SaraUI_PictureBox**: imágenes circulares con bordes degradados.
- **SaraUI_RadioButton**: botón de opción con efectos visuales.
- **SaraUI_Line**: separador horizontal o vertical.
- **SaraUI_SideBar**: barra lateral expandible con animaciones temporales y estados observables.

## 🚀 Instalación

### Opción A: NuGet

Desde la terminal:

```powershell
dotnet add package Sara_UI_Design
```

También puedes buscar `Sara_UI_Design` desde el administrador de paquetes NuGet de Visual Studio.

### Opción B: referencia manual

1. Descarga o clona el repositorio.
2. Compila el proyecto para generar `Sara_UI_Design.dll`.
3. Agrega la DLL como referencia en tu proyecto de Windows Forms.
4. Agrega los controles al Cuadro de herramientas de Visual Studio.

## 🛠️ Requisitos actuales

- Windows.
- Windows Forms sobre .NET 8 (`net8.0-windows`) o .NET Framework 4.8 (`net48`).
- No requiere bibliotecas externas para sus animaciones.

## 🎬 Motor de animaciones

La biblioteca incorpora un motor propio compatible con ambos frameworks. El espacio de nombres `Sara_UI_Design.Animations` incluye interpolación numérica, curvas de aceleración, pausa, reanudación, cancelación, repetición y reversa automática.

`SaraUI_CircularProgressBar` utiliza este motor para animar cambios de valor y el modo `Marquee`. La aplicación Demo también contiene una prueba de movimiento aplicada a un control estándar de Windows Forms.

`SaraControlTransitions` permite animar posición, tamaño, límites, colores y opacidad de formularios con una API de alto nivel:

```csharp
SaraControlTransitions transitions = new SaraControlTransitions(components) {
    Target = panelLogin
};

transitions.MoveTo(
    new Point(80, 120),
    new SaraAnimationOptions {
        Duration = 600,
        Easing = SaraEasing.EaseInOutCubic
    });
```

Los controles posicionados mediante `Dock` y los tamaños administrados mediante `AutoSize` deben liberarse de esas reglas de diseño antes de animar su geometría.

`SaraUI_SideBar` utiliza directamente el motor propio para expandirse y contraerse. Su configuración se expresa en tiempo real y no en píxeles por fotograma:

```csharp
sideBar.AnimationDuration = 450;
sideBar.AnimationEasing = SaraEasing.EaseInOutCubic;
sideBar.AutoHideButtonText = true;
sideBar.Toggle();
```

La barra también permite pausar, reanudar y detener una transición. Los textos ocultos de los botones se conservan internamente sin modificar su propiedad `Tag`.

## 🧪 Compilación y demostración

La solución contiene dos proyectos:

- `Sara_UI_Design`: biblioteca de controles compilada para .NET 8 y .NET Framework 4.8.
- `Sara_UI_Design.Demo`: aplicación visual para explorar y comprobar los controles en ambos entornos.

Para compilar toda la solución en modo Release:

```powershell
dotnet restore .\Sara_UI_Design.slnx
dotnet build .\Sara_UI_Design.slnx --configuration Release
```

También puedes establecer `Sara_UI_Design.Demo` como proyecto de inicio desde Visual Studio y elegir el framework que deseas ejecutar.

## 📚 Documentación

La biblioteca genera documentación XML para IntelliSense. Visual Studio puede mostrar descripciones de las propiedades, los eventos y los métodos públicos documentados.

## 📄 Licencia

Este proyecto utiliza la Licencia MIT. Puede usarse, modificarse y distribuirse conservando el aviso de derechos de autor correspondiente.
