# Sara UI Design 🎨

Una biblioteca de controles personalizados para **Windows Forms sobre .NET**, enfocada en diseño moderno, flexibilidad y facilidad de uso.

## ✨ Controles incluidos

- **SaraUI_FlexPanel**: panel con lógica inspirada en CSS Flexbox.
- **SaraUI_ShadowPanel**: panel con sombras proyectadas y bordes redondeados.
- **SaraUI_Button**: botón con iconos, estados visuales accesibles y transiciones de color.
- **SaraUI_TextBox**: entrada con placeholder, validación e iconos.
- **SaraUI_CircularProgressBar**: progreso circular animado.
- **SaraUI_ProgressBar**: progreso lineal determinado o Marquee con texto y degradado.
- **SaraUI_ToggleButton**: interruptor animado con estados de interacción y soporte de tres estados.
- **SaraUI_ComboBox**: lista desplegable personalizable.
- **SaraUI_PictureBox**: imágenes circulares con bordes degradados.
- **SaraUI_RadioButton**: botón de opción animado con estados de interacción y navegación accesible.
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

`SaraUI_CircularProgressBar` y `SaraUI_ProgressBar` utilizan este motor para animar cambios de valor y el modo `Marquee`. La aplicación Demo permite compararlos bajo las mismas órdenes y también contiene una prueba de movimiento aplicada a un control estándar de Windows Forms.

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

`SaraUI_Button` incorpora estados observables para interacción con ratón, teclado, foco y modo deshabilitado. Las transiciones utilizan `SaraAnimator`, y `Color.Empty` permite conservar valores automáticos compatibles con la versión anterior:

```csharp
saveButton.IconName = "Check";
saveButton.IconColor = Color.MistyRose;
saveButton.HoverBackColor = Color.SlateBlue;
saveButton.PressedBackColor = Color.DarkSlateBlue;
saveButton.FocusBorderColor = Color.HotPink;
saveButton.AnimationDuration = 180;
```

El botón también respeta `Padding`, `TextAlign`, `RightToLeft`, mnemónicos, elipsis y la guía de foco estándar. Sus propiedades heredadas `AccessibleName`, `AccessibleDescription` y `TabIndex` deben configurarse según el formulario.

`SaraUI_ToggleButton` representa `Unchecked`, `Checked` e `Indeterminate` mediante una transición continua del indicador y los colores. También distingue hover, presión, foco y estado deshabilitado:

```csharp
notificationsToggle.ThreeState = true;
notificationsToggle.AnimationDuration = 220;
notificationsToggle.AnimationEasing = SaraEasing.EaseInOutCubic;
notificationsToggle.IndeterminateBackColor = Color.DarkGoldenrod;
notificationsToggle.Checked = true;
```

El interruptor conserva el evento estándar `CheckedChanged` y agrega estado visual observable, eventos de animación, pausa, reanudación y detención. Cuando `ThreeState` está activo, utilice `CheckStateChanged` para distinguir también el estado indeterminado. `Text` se almacena para accesibilidad y automatización, pero no se dibuja dentro de su superficie compacta; configure además `AccessibleName` y `AccessibleDescription` según el formulario.

`SaraUI_RadioButton` conserva la selección exclusiva y la navegación estándar de Windows Forms, pero anima el indicador y los colores cuando cambia `Checked`. También representa hover, presión, foco y estado deshabilitado:

```csharp
basicOption.CheckedColor = Color.MediumSlateBlue;
basicOption.UncheckedColor = Color.Gray;
basicOption.HoverColor = Color.SlateBlue;
basicOption.FocusBorderColor = Color.HotPink;
basicOption.AnimationDuration = 220;
basicOption.Checked = true;
```

Los RadioButton que deban excluirse entre sí deben agregarse al mismo contenedor, por ejemplo un `Panel` o `GroupBox`. El control respeta `CheckAlign`, `TextAlign`, `Padding`, `RightToLeft`, mnemónicos y elipsis. `RadioSize`, `IndicatorSize` y `TextSpacing` permiten ajustar su geometría. El nombre histórico `UnCheckedColor` continúa disponible para compatibilidad; el código nuevo debe utilizar `UncheckedColor`.

`SaraUI_ProgressBar` separa el valor lógico solicitado del valor interpolado que se está dibujando. Admite progreso determinado, segmento indeterminado, degradado, texto deslizante y dirección de derecha a izquierda:

```csharp
progressBar.AnimationDuration = 700;
progressBar.AnimationEasing = SaraEasing.EaseInOutCubic;
progressBar.ShowValue = TextPosition.Sliding;
progressBar.SymbolAfter = "%";
progressBar.Value = 80;
```

La animación puede pausarse, reanudarse o detenerse con `PauseAnimation`, `ResumeAnimation` y `StopAnimation`. La propiedad `DisplayedValue` permite observar el avance visual, mientras `Value` conserva inmediatamente el destino lógico. El nombre histórico `ShowMaximun` continúa disponible para compatibilidad; el código nuevo debe utilizar `ShowMaximum`.

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
