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
- **SaraUI_ComboBox**: lista desplegable compuesta con datos, estados accesibles y transiciones visuales.
- **SaraUI_DatePicker**: selector de fecha nativo con superficie redondeada y estados animados.
- **SaraUI_TabControl**: pestañas animadas con navegación, cierre cancelable y estados accesibles.
- **SaraUI_ScrollBar**: desplazamiento horizontal o vertical con arrastre, teclado y estados animados.
- **SaraUI_PictureBox**: imágenes circulares con bordes degradados.
- **SaraUI_RadioButton**: botón de opción animado con estados de interacción y navegación accesible.
- **SaraUI_Line**: separador horizontal o vertical con trazos, degradados y estados animados.
- **SaraUI_SideBar**: barra lateral expandible con animaciones temporales y estados observables.
- **Paquete de menús**: barra principal, menú contextual, renderer y paleta con temas recursivos.

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

`SaraUI_ComboBox` conserva una instancia nativa de `ComboBox` para selección, enlace de datos, autocompletado y navegación mediante teclado. Su superficie representa los estados normal, hover, presionado, enfocado, desplegado y deshabilitado; al abrir la lista, la flecha y los colores cambian mediante `SaraAnimator`:

```csharp
environmentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
environmentComboBox.PlaceholderText = "Selecciona un entorno";
environmentComboBox.Items.AddRange(new object[] {
    "Desarrollo",
    "Pruebas",
    "Producción"
});
environmentComboBox.AnimationDuration = 220;
environmentComboBox.AnimationEasing = SaraEasing.EaseInOutCubic;
```

El evento `SelectedIndexChanged` sigue la convención de Windows Forms; `OnSelectedIndexChanged` continúa disponible para código existente. `DataSource`, `SelectedItem` y `SelectedValue` aceptan y devuelven `null` cuando la lista no está enlazada o no tiene selección. El estilo `Simple` no es compatible con la superficie compuesta; utilice `DropDown` o `DropDownList`. La lista puede abrirse y cerrarse mediante `OpenDropDown`, `CloseDropDown` o `DroppedDown`.

`SaraUI_DatePicker` conserva el calendario nativo de Windows Forms, sus rangos, formatos, selección y navegación mediante teclado. La superficie personalizada representa los estados normal, hover, presionado, enfocado, desplegado y deshabilitado sin bloquear `F4`, `Alt+↓` ni las teclas utilizadas para cambiar la fecha:

```csharp
appointmentDate.Format = DateTimePickerFormat.Custom;
appointmentDate.CustomFormat = "dd/MM/yyyy";
appointmentDate.MinDate = DateTime.Today;
appointmentDate.MaxDate = DateTime.Today.AddYears(1);
appointmentDate.SkinColor = Color.White;
appointmentDate.TextColor = Color.DimGray;
appointmentDate.BorderColor = Color.MediumSlateBlue;
appointmentDate.FocusBorderColor = Color.HotPink;
appointmentDate.AnimationDuration = 220;
```

El calendario puede controlarse mediante `OpenDropDown` y `CloseDropDown`; el primer método devuelve `false` si el selector está deshabilitado o usa `ShowUpDown`. `IsDropDownOpen`, `VisualState`, `AnimationState` y `DisplayedDropDownProgress` permiten observar el comportamiento sin sustituir la lógica nativa. Las transiciones pueden pausarse, reanudarse o detenerse mediante `PauseAnimation`, `ResumeAnimation` y `StopAnimation`.

`SaraUI_TabControl` conserva `TabPages`, `SelectedIndex`, `SelectedTab`, `Selecting` y `SelectedIndexChanged` de Windows Forms. Anima el indicador de selección y los colores de hover y presión, permite repartir los encabezados con `StretchTabs` y admite iconos tanto por índice como por clave:

```csharp
workspaceTabs.SelectedTabColor = Color.MediumSlateBlue;
workspaceTabs.UnselectedTabColor = Color.FromArgb(230, 230, 240);
workspaceTabs.IndicatorColor = Color.HotPink;
workspaceTabs.StretchTabs = true;
workspaceTabs.ShowCloseButtons = true;
workspaceTabs.AnimationDuration = 320;
workspaceTabs.AnimationEasing = SaraEasing.EaseInOutCubic;
```

`Ctrl+Tab` y `Ctrl+Mayús+Tab` recorren únicamente páginas habilitadas. Cuando se muestran botones de cierre, `Ctrl+W` solicita cerrar la página seleccionada. `TabClosing` permite cancelar la operación y `TabClosed` informa la página retirada. `CloseTab` elimina la página de `TabPages`, pero no llama a `Dispose`; el consumidor conserva la responsabilidad sobre su ciclo de vida. Las animaciones pueden pausarse, reanudarse o detenerse mediante `PauseAnimation`, `ResumeAnimation` y `StopAnimation`.

`SaraUI_FlexPanel` distribuye los controles visibles siguiendo el orden de `Controls` y respeta tanto el `Padding` del contenedor como el `Margin` de cada elemento. Admite filas, columnas, envoltura, seis formas de distribución y alineación transversal:

```csharp
flexPanel.Direction = SaraUI_FlexPanel.FlexDirection.Row;
flexPanel.Justify = SaraUI_FlexPanel.JustifyContent.SpaceEvenly;
flexPanel.WrapContents = SaraUI_FlexPanel.FlexWrap.Wrap;
flexPanel.AlignItems = SaraUI_FlexPanel.FlexAlignment.Center;
flexPanel.ChildSpacing = 12;
flexPanel.AnimationEnabled = true;
flexPanel.AnimationDuration = 600;
```

Los controles con `Dock` permanecen bajo el layout nativo de Windows Forms y el contenido flexible utiliza el espacio restante. `RightToLeft` invierte el avance horizontal y las columnas envueltas. `FlexAlignment.Auto` conserva el comportamiento histórico: inicio en filas y centro en columnas. Una reorganización puede pausarse, reanudarse o finalizar inmediatamente mediante `PauseAnimation`, `ResumeAnimation` y `StopAnimation`.

`SaraUI_ShadowPanel` extiende la distribución de `SaraUI_FlexPanel` con una superficie redondeada, borde opcional y sombra difusa cacheada. Su propiedad `Padding` representa exclusivamente el espacio del contenido; la reserva direccional necesaria para la sombra se calcula de forma independiente y puede consultarse mediante `ShadowInsets`:

```csharp
shadowPanel.Padding = new Padding(24);
shadowPanel.ShadowSize = 18;
shadowPanel.SetShadowOffset(8, 12);
shadowPanel.ShadowOpacity = 120;
shadowPanel.ShadowFocusScale = 0.72f;
shadowPanel.BorderColor = Color.MediumSlateBlue;
shadowPanel.BorderThickness = 1;
```

Los offsets positivos reservan más espacio a la derecha o abajo; los negativos lo hacen a la izquierda o arriba. `SetShadowOffset` permite actualizar ambos ejes con una sola reorganización. `ShadowSize` y `ShadowOpacity` admiten cero para desactivar el efecto sin retirar la superficie. La sombra se reconstruye únicamente cuando cambia su geometría o apariencia, mientras el borde y el fondo se repintan sin conservar recursos GDI abiertos. El panel mantiene las opciones heredadas de dirección, envoltura, alineación y animación del contenido.

`SaraUI_GridPanel` combina pistas fijas y fraccionales y ajusta sus cálculos al espacio disponible sin producir tamaños negativos. Los valores fijos aceptan números o el sufijo `px`; las fracciones usan `fr` y siempre se interpretan con cultura invariable:

```csharp
gridPanel.SetGridTemplate("140px, 1fr, 2fr", "80px, 1fr, 1fr");
gridPanel.SetGaps(16, 12);
gridPanel.Padding = new Padding(20);
gridPanel.AnimationEnabled = true;
gridPanel.AnimationDuration = 500;

SaraUI_GridPanel.SetGridPosition(header, 0, 0, 1, 3);
SaraUI_GridPanel.SetGridPosition(sidebar, 1, 0, 2, 1);
```

Las posiciones y spans nuevos se conservan en metadatos asociados al control y no modifican `Control.Tag`. Para mantener compatibilidad, un `Tag` textual con formato `"fila,columna"` todavía se interpreta como posición histórica. Los controles sin posición explícita ocupan las celdas libres siguiendo el orden de `Controls`; los que no encuentran espacio permanecen fuera del área visible y `UnplacedControlCount` informa cuántos son. `Margin`, `Padding`, alineaciones, `RightToLeft` y controles con `Dock` se procesan de forma explícita. Una reorganización puede pausarse, reanudarse o finalizar mediante `PauseAnimation`, `ResumeAnimation` y `StopAnimation`; el arrastre opcional del formulario usa captura del ratón y se cancela de forma segura.

`SaraUI_PictureBox` conserva la API de `PictureBox` para `Image`, `ImageLocation`, `InitialImage`, `ErrorImage`, `WaitOnLoad` y `SizeMode`, y añade recorte circular o rectangular con bordes redondeados y degradados:

```csharp
avatar.Image = profileImage;
avatar.SizeMode = PictureBoxSizeMode.Zoom;
avatar.IsCircular = true;
avatar.MaintainCircularAspectRatio = true;
avatar.ClipToShape = true;
avatar.BorderSize = 6;
avatar.BorderColor = Color.MediumSlateBlue;
avatar.BorderColor2 = Color.HotPink;
avatar.AnimationDuration = 180;
```

La proporción circular continúa gobernada por el ancho para conservar compatibilidad; `MaintainCircularAspectRatio = false` permite elipses cuando un contenedor administra ambas dimensiones. El grosor y el radio efectivos se limitan al espacio disponible, por lo que los tamaños mínimos no producen geometría negativa. La región solo se reconstruye al cambiar forma o tamaño y se libera sin destruir regiones externas. Los estados de hover, foco y deshabilitación pueden personalizar sus colores y utilizan el motor propio; la imagen sigue perteneciendo al consumidor y el control no la desecha al reemplazarla.

`SaraUI_Line` conserva `Orientation`, `LineWidth`, `LineColor` y `LineStyle`, y añade alineación transversal, degradado opcional, `Padding`, remates independientes y escalado opcional del grosor según el DPI:

```csharp
separator.Orientation = SaraUI_Line.LineOrientation.Horizontal;
separator.Alignment = SaraUI_Line.LineAlignment.Center;
separator.Padding = new Padding(16, 8, 16, 8);
separator.LineWidth = 4;
separator.LineColor = Color.MediumSlateBlue;
separator.LineColor2 = Color.HotPink;
separator.StartCap = LineCap.Round;
separator.EndCap = LineCap.Round;
```

En líneas horizontales, `RightToLeft` invierte el inicio y el final lógicos, incluido el sentido del degradado y de los remates. El grosor efectivo se limita al espacio transversal disponible para evitar geometría inválida en tamaños mínimos. `DashStyle.Custom` y `LineCap.Custom` se rechazan porque requieren recursos externos que el control no administra. De manera predeterminada, el separador no participa en el orden de tabulación; si se activa `TabStop`, puede recibir foco, mostrar una guía accesible y animar sus colores. Las transiciones de hover, foco y deshabilitación se pueden pausar, reanudar o detener mediante `PauseAnimation`, `ResumeAnimation` y `StopAnimation`.

El paquete de menús está compuesto por `SaraUI_MenuStrip`, `SaraUI_DropdownMenu`, `SaraUI_MenuRenderer` y `SaraUI_MenuColorTable`. La barra y el menú contextual administran la configuración pública; el renderer se limita a dibujar y la tabla de colores describe la paleta utilizada por Windows Forms:

```csharp
mainMenu.PrimaryColor = Color.MediumSlateBlue;
mainMenu.MenuItemTextColor = Color.Gainsboro;
mainMenu.DropDownItemHeight = 36;
mainMenu.SelectionCornerRadius = 8;
mainMenu.SelectionOpacity = 48;

contextMenu.PrimaryColor = mainMenu.PrimaryColor;
contextMenu.MenuItemHeight = 36;
contextMenu.ShowImageMargin = true;
contextMenu.RefreshTheme();
```

El tema se propaga a todos los niveles de submenús y puede reconstruirse mediante `RefreshTheme` después de agregar elementos dinámicos. Las alturas opcionales se ajustan al DPI y pueden devolverse al tamaño nativo desactivando `ApplyUniformDropDownItemHeight` o `ApplyUniformItemHeight`. Los estados seleccionado, presionado y deshabilitado se representan sin modificar `ForeColor`, `Text`, `Tag`, `Checked`, `Enabled` ni los atajos del elemento. La navegación, los mnemónicos, las teclas de dirección, `Enter`, `Esc` y los accesos directos continúan bajo la administración nativa de `MenuStrip` y `ContextMenuStrip`.

`SaraUI_ScrollBar` mantiene un valor lógico entero y una posición visual interpolada. Admite orientación horizontal o vertical, clic por páginas en el canal, arrastre con captura del ratón, rueda, flechas, `PageUp`, `PageDown`, `Home` y `End`:

```csharp
contentScroll.SetRange(0, 100);
contentScroll.Orientation = SaraUI_ScrollBar.ScrollOrientation.Vertical;
contentScroll.SmallChange = 5;
contentScroll.LargeChange = 20;
contentScroll.AnimationDuration = 300;
contentScroll.AnimationEasing = SaraEasing.EaseInOutCubic;
contentScroll.Value = 40;
```

`DisplayedValue` permite observar la posición animada sin alterar el destino expuesto por `Value`. Durante el arrastre, el indicador sigue directamente al puntero y genera eventos `Scroll` de tipo `ThumbTrack`; `ValueChanged` se genera una sola vez por cada valor lógico diferente. `RightToLeft` invierte automáticamente una barra horizontal, mientras `ReverseDirection` permite invertir cualquiera de los dos ejes. Las transiciones pueden pausarse, reanudarse o detenerse mediante `PauseAnimation`, `ResumeAnimation` y `StopAnimation`.

`SaraUI_IconLibrary` ofrece un catálogo vectorial clasificado que no depende de fuentes de iconos ni de archivos externos. Los nombres canónicos son los que aparecen en el diseñador; los nombres históricos repetidos continúan resolviéndose como alias para no romper formularios guardados:

```csharp
SaraUI_IconLibrary.DrawIcon(
    "InventoryLoad",
    graphics,
    new Rectangle(0, 0, 32, 32),
    Color.MediumSlateBlue);

bool known = SaraUI_IconLibrary.TryDrawIcon(
    "Update",
    graphics,
    iconBounds,
    Color.DodgerBlue,
    SaraUI_IconLibrary.SaraIconStyle.Rounded);
```

Los casos habituales de una biblioteca o inventario se representan mediante `Add`, `Update`, `Delete`, `Refresh`, `InventoryLoad`, `Code`, `Title`, `Author` y `Genre`. El catálogo también incorpora `Save`, `Undo`, `Redo`, `Import`, `Export`, `Print`, `Archive`, `Attachment`, `DocumentAdd`, `FolderOpen`, `Table`, `List`, `Package`, `Inventory`, `Warehouse`, `Barcode`, `Cart`, `Receipt`, `Book`, `Monitor`, `Server`, `Window` y `Camera`.

Las siluetas mantienen una dirección visual consistente: `Undo` apunta a la izquierda, `Redo` a la derecha y `Refresh` representa un giro horario. `Edit` utiliza un lápiz completo; `Lock`, `Unlock` y `LockKey` diferencian el estado del arco y el ojo de la cerradura. Los símbolos de comunicación, ayuda, conectividad y sistema conservan sus rasgos reconocibles incluso al dibujarse a 16 px. La ventana de demostración permite comprobar cada símbolo a 16, 24, 32 y 48 px antes de incorporarlo a una interfaz.

`GetIconCatalog` devuelve nombre, categoría, descripción, nombre canónico e indicador de alias. `GetAvailableIcons` expone solamente los nombres canónicos, por lo que el selector no muestra duplicados como `Plus`/`Add`, `Trash`/`Delete`, `Reload`/`Refresh` o `Settings`/`Gear`. Esos nombres anteriores, además de equivalentes en español como `Agregar`, `Actualizar`, `Eliminar` y `CargarExistencia`, siguen funcionando al dibujar. Un nombre desconocido o límites demasiado pequeños hacen que `TryDrawIcon` devuelva `false` sin pintar un cuadro rojo ni interrumpir el diseñador.

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
