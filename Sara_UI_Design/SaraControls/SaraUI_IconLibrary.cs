using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Sara_UI_Design.SaraControls {
    /// <summary>
    /// Catálogo de iconos vectoriales independientes de fuente para controles WinForms.
    /// </summary>
    public static class SaraUI_IconLibrary {
        private delegate void IconRenderer(Graphics graphics, Rectangle bounds, Color color);

        private sealed class IconDefinition {
            public IconDefinition(
                string name,
                SaraIconCategory category,
                string description,
                IconRenderer renderer,
                string canonicalName,
                bool isAlias) {
                Name = name;
                Category = category;
                Description = description;
                Renderer = renderer;
                CanonicalName = canonicalName;
                IsAlias = isAlias;
            }

            public string Name { get; }
            public SaraIconCategory Category { get; }
            public string Description { get; }
            public IconRenderer Renderer { get; }
            public string CanonicalName { get; }
            public bool IsAlias { get; }
        }

        private static readonly Dictionary<string, IconDefinition> IconCatalog = CreateIconCatalog();

        /// <summary>Define la presentación exterior aplicada a un icono.</summary>
        public enum SaraIconStyle {
            /// <summary>Dibuja únicamente el símbolo vectorial.</summary>
            Outline,
            /// <summary>Usa la variante rellena cuando el símbolo dispone de ella.</summary>
            Filled,
            /// <summary>Rodea el símbolo con un contorno circular.</summary>
            Circle,
            /// <summary>Rodea el símbolo con un contorno cuadrado.</summary>
            Square,
            /// <summary>Rodea el símbolo con un rectángulo redondeado.</summary>
            Rounded
        }

        /// <summary>Clasifica los iconos por su uso habitual en una interfaz.</summary>
        public enum SaraIconCategory {
            /// <summary>Operaciones que modifican o administran información.</summary>
            Actions,
            /// <summary>Desplazamiento y navegación.</summary>
            Navigation,
            /// <summary>Archivos, carpetas y documentos.</summary>
            Files,
            /// <summary>Datos, código, informes y visualizaciones.</summary>
            Data,
            /// <summary>Existencias, almacenes y productos.</summary>
            Inventory,
            /// <summary>Libros y datos bibliográficos.</summary>
            Catalog,
            /// <summary>Personas, cuentas y permisos.</summary>
            People,
            /// <summary>Mensajería y comunicación.</summary>
            Communication,
            /// <summary>Estados, avisos y validaciones.</summary>
            Status,
            /// <summary>Reproducción y contenido multimedia.</summary>
            Media,
            /// <summary>Ventanas, dispositivos y configuración.</summary>
            System,
            /// <summary>Compras, pagos y documentos comerciales.</summary>
            Commerce
        }

        /// <summary>Describe un elemento disponible en el catálogo de iconos.</summary>
        public sealed class SaraIconInfo {
            internal SaraIconInfo(
                string name,
                SaraIconCategory category,
                string description,
                string canonicalName,
                bool isAlias) {
                Name = name;
                Category = category;
                Description = description;
                CanonicalName = canonicalName;
                IsAlias = isAlias;
            }

            /// <summary>Obtiene el nombre aceptado por el motor de dibujo.</summary>
            public string Name { get; }

            /// <summary>Obtiene la categoría funcional.</summary>
            public SaraIconCategory Category { get; }

            /// <summary>Obtiene una descripción breve del significado recomendado.</summary>
            public string Description { get; }

            /// <summary>Obtiene el nombre principal que comparte el mismo dibujo.</summary>
            public string CanonicalName { get; }

            /// <summary>Indica si el nombre se conserva únicamente por compatibilidad.</summary>
            public bool IsAlias { get; }
        }

        /// <summary>Dibuja un icono con el estilo exterior indicado.</summary>
        public static void DrawIcon(string iconName, Graphics g, Rectangle rect, Color color, SaraIconStyle style) {
            TryDrawIcon(iconName, g, rect, color, style);
        }

        /// <summary>Dibuja un icono sin contenedor exterior.</summary>
        public static void DrawIcon(string iconName, Graphics g, Rectangle rect, Color color) {
            TryDrawIcon(iconName, g, rect, color, SaraIconStyle.Outline);
        }

        /// <summary>Intenta dibujar un icono sin producir marcadores de error en la interfaz.</summary>
        /// <returns><see langword="true"/> si el nombre y los límites permitieron dibujarlo.</returns>
        public static bool TryDrawIcon(string? iconName, Graphics g, Rectangle rect, Color color) {
            return TryDrawIcon(iconName, g, rect, color, SaraIconStyle.Outline);
        }

        /// <summary>Intenta dibujar un icono con el estilo exterior indicado.</summary>
        /// <returns><see langword="true"/> si el nombre y los límites permitieron dibujarlo.</returns>
        public static bool TryDrawIcon(
            string? iconName,
            Graphics g,
            Rectangle rect,
            Color color,
            SaraIconStyle style) {
            if(g == null) {
                throw new ArgumentNullException(nameof(g));
            }

            string normalizedName = iconName?.Trim() ?? string.Empty;
            if(normalizedName.Length == 0 ||
                string.Equals(normalizedName, "None", StringComparison.OrdinalIgnoreCase) ||
                rect.Width < 4 || rect.Height < 4 ||
                !IconCatalog.TryGetValue(normalizedName, out IconDefinition? definition)) {
                return false;
            }

            GraphicsState state = g.Save();
            try {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                switch(style) {
                    case SaraIconStyle.Filled:
                        DrawFilled(definition, g, rect, color);
                        break;
                    case SaraIconStyle.Circle:
                    case SaraIconStyle.Square:
                    case SaraIconStyle.Rounded:
                        DrawContainer(definition, g, rect, color, style);
                        break;
                    default:
                        definition.Renderer(g, NormalizeBounds(rect), color);
                        break;
                }
                return true;
            } catch(ArgumentException) {
                return false;
            } catch(System.Runtime.InteropServices.ExternalException) {
                return false;
            } finally {
                g.Restore(state);
            }
        }

        /// <summary>Obtiene los nombres canónicos ordenados para mostrarlos en el diseñador.</summary>
        public static List<string> GetAvailableIcons() {
            return IconCatalog.Values
                .Where(icon => !icon.IsAlias)
                .Select(icon => icon.Name)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Obtiene los nombres canónicos de una categoría concreta.</summary>
        public static List<string> GetAvailableIcons(SaraIconCategory category) {
            return IconCatalog.Values
                .Where(icon => !icon.IsAlias && icon.Category == category)
                .Select(icon => icon.Name)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Obtiene una instantánea descriptiva del catálogo.</summary>
        public static IReadOnlyList<SaraIconInfo> GetIconCatalog(bool includeAliases = false) {
            return IconCatalog.Values
                .Where(icon => includeAliases || !icon.IsAlias)
                .OrderBy(icon => icon.Category)
                .ThenBy(icon => icon.Name, StringComparer.OrdinalIgnoreCase)
                .Select(CreateIconInfo)
                .ToList();
        }

        /// <summary>Busca la descripción y el nombre canónico de un icono o alias.</summary>
        public static bool TryGetIconInfo(string? iconName, out SaraIconInfo? iconInfo) {
            iconInfo = null;
            string normalizedName = iconName?.Trim() ?? string.Empty;
            if(normalizedName.Length == 0 ||
                !IconCatalog.TryGetValue(normalizedName, out IconDefinition? definition)) {
                return false;
            }
            iconInfo = CreateIconInfo(definition);
            return true;
        }

        private static SaraIconInfo CreateIconInfo(IconDefinition definition) {
            return new SaraIconInfo(
                definition.Name,
                definition.Category,
                definition.Description,
                definition.CanonicalName,
                definition.IsAlias);
        }

        private static void DrawFilled(IconDefinition definition, Graphics g, Rectangle rect, Color color) {
            if(definition.CanonicalName == "User") {
                DrawUserFilled(g, NormalizeBounds(rect), color);
                return;
            }

            if(definition.CanonicalName == "Heart" || definition.CanonicalName == "Star") {
                using SolidBrush brush = new SolidBrush(color);
                using GraphicsPath path = definition.CanonicalName == "Heart"
                    ? CreateHeartPath(NormalizeBounds(rect))
                    : CreateStarPath(NormalizeBounds(rect));
                g.FillPath(brush, path);
                return;
            }

            definition.Renderer(g, NormalizeBounds(rect), color);
        }

        private static void DrawContainer(
            IconDefinition definition,
            Graphics g,
            Rectangle rect,
            Color color,
            SaraIconStyle style) {
            Rectangle outer = NormalizeBounds(rect);
            using(Pen pen = CreatePen(color, outer, 1.7f)) {
                if(style == SaraIconStyle.Circle) {
                    g.DrawEllipse(pen, outer);
                } else if(style == SaraIconStyle.Square) {
                    g.DrawRectangle(pen, outer);
                } else {
                    using GraphicsPath path = CreateRoundedPath(
                        outer,
                        Math.Max(2f, Math.Min(outer.Width, outer.Height) * 0.22f));
                    g.DrawPath(pen, path);
                }
            }

            int inset = Math.Max(3, Math.Min(outer.Width, outer.Height) / 6);
            Rectangle inner = Rectangle.Inflate(outer, -inset, -inset);
            if(inner.Width >= 4 && inner.Height >= 4) {
                definition.Renderer(g, inner, color);
            }
        }

        private static Rectangle NormalizeBounds(Rectangle rect) {
            return new Rectangle(rect.X, rect.Y, Math.Max(1, rect.Width - 1), Math.Max(1, rect.Height - 1));
        }

        private static Pen CreatePen(Color color, Rectangle rect, float relativeWidth = 2f) {
            float scale = Math.Max(0.75f, Math.Min(rect.Width, rect.Height) / 24f);
            return new Pen(color, Math.Max(1f, relativeWidth * scale)) {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, float radius) {
            GraphicsPath path = new GraphicsPath();
            float diameter = Math.Max(1f, Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height)));
            RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateHeartPath(Rectangle rect) {
            GraphicsPath path = new GraphicsPath();
            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;
            path.AddBezier(x + w / 2f, y + h, x + w * 0.05f, y + h * 0.62f,
                x - w * 0.02f, y + h * 0.20f, x + w * 0.25f, y + h * 0.12f);
            path.AddBezier(x + w * 0.25f, y + h * 0.12f, x + w * 0.42f, y + h * 0.06f,
                x + w * 0.48f, y + h * 0.20f, x + w / 2f, y + h * 0.28f);
            path.AddBezier(x + w / 2f, y + h * 0.28f, x + w * 0.52f, y + h * 0.20f,
                x + w * 0.58f, y + h * 0.06f, x + w * 0.75f, y + h * 0.12f);
            path.AddBezier(x + w * 0.75f, y + h * 0.12f, x + w * 1.02f, y + h * 0.20f,
                x + w * 0.95f, y + h * 0.62f, x + w / 2f, y + h);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateStarPath(Rectangle rect) {
            GraphicsPath path = new GraphicsPath();
            PointF[] points = new PointF[10];
            float centerX = rect.X + rect.Width / 2f;
            float centerY = rect.Y + rect.Height / 2f;
            for(int index = 0; index < points.Length; index++) {
                double angle = -Math.PI / 2d + index * Math.PI / 5d;
                float radius = index % 2 == 0 ? 0.5f : 0.22f;
                points[index] = new PointF(
                    centerX + (float)Math.Cos(angle) * rect.Width * radius,
                    centerY + (float)Math.Sin(angle) * rect.Height * radius);
            }
            path.AddPolygon(points);
            return path;
        }

        /// <summary>Conversor conservado para formularios que usaban el tipo anidado original.</summary>
        public class IconNameConverter:global::Sara_UI_Design.SaraControls.IconNameConverter {
        }

        private static Dictionary<string, IconDefinition> CreateIconCatalog() {
            var icons = new Dictionary<string, IconDefinition>(StringComparer.OrdinalIgnoreCase);
            AddIcons(icons);
            AddAliases(icons);
            return icons;
        }

        private static void Register(
            Dictionary<string, IconDefinition> icons,
            string name,
            SaraIconCategory category,
            string description,
            IconRenderer renderer) {
            icons.Add(name, new IconDefinition(name, category, description, renderer, name, false));
        }

        private static void RegisterAlias(
            Dictionary<string, IconDefinition> icons,
            string alias,
            string canonicalName) {
            IconDefinition canonical = icons[canonicalName];
            icons.Add(alias, new IconDefinition(
                alias,
                canonical.Category,
                $"Alias compatible de {canonicalName}.",
                canonical.Renderer,
                canonicalName,
                true));
        }

        private static void AddIcons(Dictionary<string, IconDefinition> icons) {
            AddExistingIcons(icons);
            AddExpandedIcons(icons);
        }

        private static void AddAliases(Dictionary<string, IconDefinition> icons) {
            RegisterAlias(icons, "ArrowRight2", "ArrowRight");
            RegisterAlias(icons, "Box", "Package");
            RegisterAlias(icons, "Pin", "MapPin");
            RegisterAlias(icons, "Plus", "Add");
            RegisterAlias(icons, "PlusCircle", "AddCircle");
            RegisterAlias(icons, "PlusSquare", "AddSquare");
            RegisterAlias(icons, "Reload", "Refresh");
            RegisterAlias(icons, "Settings", "Gear");
            RegisterAlias(icons, "Trash", "Delete");
            RegisterAlias(icons, "TrashAlt", "Delete");
            RegisterAlias(icons, "X", "Close");
            RegisterAlias(icons, "Agregar", "Add");
            RegisterAlias(icons, "Actualizar", "Update");
            RegisterAlias(icons, "Eliminar", "Delete");
            RegisterAlias(icons, "Refrescar", "Refresh");
            RegisterAlias(icons, "CargarExistencia", "InventoryLoad");
            RegisterAlias(icons, "Codigo", "Code");
            RegisterAlias(icons, "Titulo", "Title");
            RegisterAlias(icons, "Autor", "Author");
            RegisterAlias(icons, "Genero", "Genre");
        }

        private static void AddExistingIcons(Dictionary<string, IconDefinition> icons) {
            Register(icons, "Add", SaraIconCategory.Actions, "Agregar un elemento.", DrawAdd);
            Register(icons, "AddCircle", SaraIconCategory.Actions, "Agregar dentro de un círculo.", DrawAddCircle);
            Register(icons, "AddSquare", SaraIconCategory.Actions, "Agregar dentro de un cuadro.", DrawAddSquare);
            Register(icons, "Alert", SaraIconCategory.Status, "Alerta que requiere atención.", DrawAlert);
            Register(icons, "ArrowDown", SaraIconCategory.Navigation, "Desplazarse hacia abajo.", DrawArrowDown);
            Register(icons, "ArrowLeft", SaraIconCategory.Navigation, "Regresar o desplazarse a la izquierda.", DrawArrowLeft);
            Register(icons, "ArrowRight", SaraIconCategory.Navigation, "Avanzar o desplazarse a la derecha.", DrawArrowRight);
            Register(icons, "ArrowUp", SaraIconCategory.Navigation, "Desplazarse hacia arriba.", DrawArrowUp);
            Register(icons, "Bell", SaraIconCategory.Communication, "Notificaciones.", DrawBell);
            Register(icons, "Bookmark", SaraIconCategory.Navigation, "Marcador o favorito.", DrawBookmark);
            Register(icons, "Bug", SaraIconCategory.System, "Error de software.", DrawBug);
            Register(icons, "Bulb", SaraIconCategory.Status, "Idea o sugerencia.", DrawBulb);
            Register(icons, "Calendar", SaraIconCategory.Data, "Fecha o calendario.", DrawCalendar);
            Register(icons, "Check", SaraIconCategory.Status, "Operación confirmada.", DrawCheck);
            Register(icons, "CheckCircle", SaraIconCategory.Status, "Confirmación circular.", DrawCheckCircle);
            Register(icons, "CheckSquare", SaraIconCategory.Status, "Selección marcada.", DrawCheckSquare);
            Register(icons, "ChevronDown", SaraIconCategory.Navigation, "Expandir una lista.", DrawChevronDown);
            Register(icons, "Clear", SaraIconCategory.Actions, "Limpiar un valor o filtro.", DrawClear);
            Register(icons, "Clipboard", SaraIconCategory.Files, "Portapapeles o tarea.", DrawClipboard);
            Register(icons, "Close", SaraIconCategory.Actions, "Cerrar una vista.", DrawClose);
            Register(icons, "CloseCircle", SaraIconCategory.Actions, "Cerrar o cancelar dentro de un círculo.", DrawCloseCircle);
            Register(icons, "CloseSquare", SaraIconCategory.Actions, "Cerrar o cancelar dentro de un cuadro.", DrawCloseSquare);
            Register(icons, "Cloud", SaraIconCategory.System, "Servicio en la nube.", DrawCloud);
            Register(icons, "Copy", SaraIconCategory.Actions, "Copiar información.", DrawCopy);
            Register(icons, "Dashboard", SaraIconCategory.Data, "Panel de indicadores.", DrawDashboard);
            Register(icons, "Database", SaraIconCategory.Data, "Base de datos.", DrawDatabase);
            Register(icons, "Delete", SaraIconCategory.Actions, "Eliminar un registro.", DrawDelete);
            Register(icons, "DeleteCircle", SaraIconCategory.Actions, "Eliminar dentro de un círculo.", DrawDeleteCircle);
            Register(icons, "Download", SaraIconCategory.Files, "Descargar contenido.", DrawDownload);
            Register(icons, "Edit", SaraIconCategory.Actions, "Editar información.", DrawEdit);
            Register(icons, "EditSquare", SaraIconCategory.Actions, "Editar dentro de un cuadro.", DrawEditSquare);
            Register(icons, "ExitDoor", SaraIconCategory.Navigation, "Salir por una puerta.", DrawExitDoor);
            Register(icons, "Eye", SaraIconCategory.Status, "Mostrar contenido.", DrawEye);
            Register(icons, "EyeOff", SaraIconCategory.Status, "Ocultar contenido.", DrawEyeOff);
            Register(icons, "File", SaraIconCategory.Files, "Archivo genérico.", DrawFile);
            Register(icons, "FileText", SaraIconCategory.Files, "Documento de texto.", DrawFileText);
            Register(icons, "Filter", SaraIconCategory.Data, "Filtrar resultados.", DrawFilter);
            Register(icons, "Folder", SaraIconCategory.Files, "Carpeta.", DrawFolder);
            Register(icons, "Gear", SaraIconCategory.System, "Configuración.", DrawGear);
            Register(icons, "Graph", SaraIconCategory.Data, "Gráfico de tendencia.", DrawGraph);
            Register(icons, "Grid", SaraIconCategory.Data, "Vista de cuadrícula.", DrawGrid);
            Register(icons, "Heart", SaraIconCategory.Status, "Favorito o me gusta.", DrawHeart);
            Register(icons, "Help", SaraIconCategory.Status, "Ayuda.", DrawHelp);
            Register(icons, "Home", SaraIconCategory.Navigation, "Página de inicio.", DrawHome);
            Register(icons, "Image", SaraIconCategory.Media, "Archivo de imagen.", DrawImage);
            Register(icons, "Info", SaraIconCategory.Status, "Información.", DrawInfo);
            Register(icons, "JustifyCenter", SaraIconCategory.Data, "Alinear texto al centro.", DrawJustifyCenter);
            Register(icons, "JustifyLeft", SaraIconCategory.Data, "Alinear texto a la izquierda.", DrawJustifyLeft);
            Register(icons, "Key", SaraIconCategory.System, "Clave o acceso.", DrawKey);
            Register(icons, "Link", SaraIconCategory.Navigation, "Enlace.", DrawLink);
            Register(icons, "Lock", SaraIconCategory.System, "Bloquear.", DrawLock);
            Register(icons, "LockKey", SaraIconCategory.System, "Acceso protegido.", DrawLockKey);
            Register(icons, "Login", SaraIconCategory.People, "Iniciar sesión.", DrawLogin);
            Register(icons, "Logout", SaraIconCategory.People, "Cerrar sesión.", DrawLogout);
            Register(icons, "Mail", SaraIconCategory.Communication, "Correo electrónico.", DrawMail);
            Register(icons, "Map", SaraIconCategory.Navigation, "Mapa.", DrawMap);
            Register(icons, "MapPin", SaraIconCategory.Navigation, "Ubicación.", DrawMapPin);
            Register(icons, "Menu", SaraIconCategory.Navigation, "Menú.", DrawMenu);
            Register(icons, "Message", SaraIconCategory.Communication, "Mensaje o conversación.", DrawMessage);
            Register(icons, "Minus", SaraIconCategory.Actions, "Quitar o reducir.", DrawMinus);
            Register(icons, "MoreHorizontal", SaraIconCategory.Navigation, "Más opciones horizontales.", DrawMoreHorizontal);
            Register(icons, "MoreVertical", SaraIconCategory.Navigation, "Más opciones verticales.", DrawMoreVertical);
            Register(icons, "Notification", SaraIconCategory.Communication, "Notificación nueva.", DrawNotification);
            Register(icons, "Pause", SaraIconCategory.Media, "Pausar.", DrawPause);
            Register(icons, "Phone", SaraIconCategory.Communication, "Teléfono.", DrawPhone);
            Register(icons, "Play", SaraIconCategory.Media, "Reproducir o iniciar.", DrawPlay);
            Register(icons, "Power", SaraIconCategory.System, "Encender o apagar.", DrawPower);
            Register(icons, "QrCode", SaraIconCategory.Data, "Código QR.", DrawQrCode);
            Register(icons, "Refresh", SaraIconCategory.Actions, "Recargar los datos actuales.", DrawRefresh);
            Register(icons, "Report", SaraIconCategory.Data, "Informe.", DrawReport);
            Register(icons, "Search", SaraIconCategory.Actions, "Buscar.", DrawSearch);
            Register(icons, "SearchMinus", SaraIconCategory.Actions, "Alejar la vista.", DrawSearchMinus);
            Register(icons, "SearchPlus", SaraIconCategory.Actions, "Acercar la vista.", DrawSearchPlus);
            Register(icons, "Shield", SaraIconCategory.System, "Seguridad o protección.", DrawShield);
            Register(icons, "Sort", SaraIconCategory.Data, "Ordenar resultados.", DrawSort);
            Register(icons, "Star", SaraIconCategory.Status, "Destacado.", DrawStar);
            Register(icons, "StarHalf", SaraIconCategory.Status, "Calificación parcial.", DrawStarHalf);
            Register(icons, "Stats", SaraIconCategory.Data, "Estadísticas.", DrawStats);
            Register(icons, "Stop", SaraIconCategory.Media, "Detener.", DrawStop);
            Register(icons, "Sync", SaraIconCategory.Actions, "Sincronizar en ambas direcciones.", DrawSync);
            Register(icons, "Tag", SaraIconCategory.Data, "Etiqueta.", DrawTag);
            Register(icons, "Timer", SaraIconCategory.System, "Temporizador.", DrawTimer);
            Register(icons, "ToggleOff", SaraIconCategory.Status, "Interruptor apagado.", DrawToggleOff);
            Register(icons, "ToggleOn", SaraIconCategory.Status, "Interruptor encendido.", DrawToggleOn);
            Register(icons, "Unlock", SaraIconCategory.System, "Desbloquear.", DrawUnlock);
            Register(icons, "Upload", SaraIconCategory.Files, "Subir contenido.", DrawUpload);
            Register(icons, "User", SaraIconCategory.People, "Usuario.", DrawUser);
            Register(icons, "UserCheck", SaraIconCategory.People, "Usuario aprobado.", DrawUserCheck);
            Register(icons, "UserFilled", SaraIconCategory.People, "Usuario relleno.", DrawUserFilled);
            Register(icons, "UserMinus", SaraIconCategory.People, "Quitar usuario.", DrawUserMinus);
            Register(icons, "UserPlus", SaraIconCategory.People, "Agregar usuario.", DrawUserPlus);
            Register(icons, "Users", SaraIconCategory.People, "Grupo de usuarios.", DrawUsers);
            Register(icons, "Video", SaraIconCategory.Media, "Vídeo.", DrawVideo);
            Register(icons, "Volume", SaraIconCategory.Media, "Volumen activado.", DrawVolume);
            Register(icons, "VolumeOff", SaraIconCategory.Media, "Volumen silenciado.", DrawVolumeOff);
            Register(icons, "Warning", SaraIconCategory.Status, "Advertencia.", DrawWarning);
            Register(icons, "Wifi", SaraIconCategory.System, "Red inalámbrica.", DrawWifi);
            Register(icons, "Yen", SaraIconCategory.Commerce, "Moneda.", DrawYen);
            Register(icons, "ZoomIn", SaraIconCategory.Actions, "Acercar la vista.", DrawZoomIn);
            Register(icons, "ZoomOut", SaraIconCategory.Actions, "Alejar la vista.", DrawZoomOut);
        }

        private static void AddExpandedIcons(Dictionary<string, IconDefinition> icons) {
            Register(icons, "Archive", SaraIconCategory.Files, "Archivar documentos o registros.", DrawArchive);
            Register(icons, "Attachment", SaraIconCategory.Files, "Adjuntar un archivo.", DrawAttachment);
            Register(icons, "Author", SaraIconCategory.Catalog, "Autor de una obra.", DrawAuthor);
            Register(icons, "Barcode", SaraIconCategory.Inventory, "Código de barras de un producto.", DrawBarcode);
            Register(icons, "Book", SaraIconCategory.Catalog, "Libro u obra bibliográfica.", DrawBook);
            Register(icons, "Camera", SaraIconCategory.Media, "Capturar una imagen.", DrawCamera);
            Register(icons, "Cart", SaraIconCategory.Commerce, "Carrito de compra.", DrawCart);
            Register(icons, "Code", SaraIconCategory.Data, "Código o identificador de un registro.", DrawCode);
            Register(icons, "DocumentAdd", SaraIconCategory.Files, "Crear un documento.", DrawDocumentAdd);
            Register(icons, "Export", SaraIconCategory.Files, "Exportar un archivo.", DrawExport);
            Register(icons, "FolderOpen", SaraIconCategory.Files, "Carpeta abierta.", DrawFolderOpen);
            Register(icons, "Genre", SaraIconCategory.Catalog, "Género o clasificación de una obra.", DrawGenre);
            Register(icons, "History", SaraIconCategory.Actions, "Historial de cambios.", DrawHistory);
            Register(icons, "Import", SaraIconCategory.Files, "Importar un archivo.", DrawImport);
            Register(icons, "Inventory", SaraIconCategory.Inventory, "Existencias disponibles.", DrawInventory);
            Register(icons, "InventoryLoad", SaraIconCategory.Inventory, "Cargar o recibir existencias.", DrawInventoryLoad);
            Register(icons, "List", SaraIconCategory.Data, "Vista de lista.", DrawList);
            Register(icons, "Monitor", SaraIconCategory.System, "Equipo de escritorio.", DrawMonitor);
            Register(icons, "Package", SaraIconCategory.Inventory, "Paquete o producto.", DrawPackage);
            Register(icons, "Print", SaraIconCategory.Files, "Imprimir.", DrawPrint);
            Register(icons, "Receipt", SaraIconCategory.Commerce, "Factura o recibo.", DrawReceipt);
            Register(icons, "Redo", SaraIconCategory.Actions, "Rehacer el último cambio.", DrawRedo);
            Register(icons, "Save", SaraIconCategory.Actions, "Guardar cambios.", DrawSave);
            Register(icons, "Server", SaraIconCategory.System, "Servidor o servicio.", DrawServer);
            Register(icons, "SourceCode", SaraIconCategory.Data, "Código fuente de software.", DrawSourceCode);
            Register(icons, "Table", SaraIconCategory.Data, "Vista tabular.", DrawTable);
            Register(icons, "Title", SaraIconCategory.Catalog, "Título de una obra.", DrawTitle);
            Register(icons, "Undo", SaraIconCategory.Actions, "Deshacer el último cambio.", DrawUndo);
            Register(icons, "Update", SaraIconCategory.Actions, "Actualizar un registro existente.", DrawUpdate);
            Register(icons, "Warehouse", SaraIconCategory.Inventory, "Almacén.", DrawWarehouse);
            Register(icons, "Window", SaraIconCategory.System, "Ventana de aplicación.", DrawWindow);
        }


        // ==========================
        // ICONOS A
        // ==========================

        /// <summary>Dibuja el icono Add dentro de los límites especificados.</summary>
        public static void DrawAdd(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = CreatePen(color, rect)) {
                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;

                g.DrawLine(pen, cx, rect.Y + rect.Height * 0.2f, cx, rect.Bottom - rect.Height * 0.2f);
                g.DrawLine(pen, rect.X + rect.Width * 0.2f, cy, rect.Right - rect.Width * 0.2f, cy);
            }
        }

        /// <summary>Dibuja el icono AddCircle dentro de los límites especificados.</summary>
        public static void DrawAddCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
                DrawAdd(g, rect, color);
            }
        }

        /// <summary>Dibuja el icono AddSquare dentro de los límites especificados.</summary>
        public static void DrawAddSquare(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
                DrawAdd(g, rect, color);
            }
        }

        // ==========================
        // ICONOS ARROWS
        // ==========================

        /// <summary>Dibuja el icono ArrowLeft dentro de los límites especificados.</summary>
        public static void DrawArrowLeft(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float y = rect.Y + rect.Height / 2f;

                g.DrawLine(pen, rect.Right, y, rect.X + rect.Width * 0.3f, y);
                g.DrawLine(pen, rect.X + rect.Width * 0.3f, y, rect.X + rect.Width * 0.5f, rect.Y);
                g.DrawLine(pen, rect.X + rect.Width * 0.3f, y, rect.X + rect.Width * 0.5f, rect.Bottom);
            }
        }

        /// <summary>Dibuja el icono ArrowRight2 dentro de los límites especificados.</summary>
        public static void DrawArrowRight2(Graphics g, Rectangle rect, Color color) {
            DrawArrowRight(g, rect, color);
        }

        /// <summary>Dibuja el icono ArrowRight dentro de los límites especificados.</summary>
        public static void DrawArrowRight(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = CreatePen(color, rect)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float centerY = rect.Y + rect.Height / 2f;
                g.DrawLine(pen, rect.X + rect.Width * 0.05f, centerY,
                    rect.Right - rect.Width * 0.08f, centerY);
                g.DrawLine(pen, rect.Right - rect.Width * 0.08f, centerY,
                    rect.Right - rect.Width * 0.36f, rect.Y + rect.Height * 0.18f);
                g.DrawLine(pen, rect.Right - rect.Width * 0.08f, centerY,
                    rect.Right - rect.Width * 0.36f, rect.Bottom - rect.Height * 0.18f);
            }
        }

        /// <summary>Dibuja el icono ArrowUp dentro de los límites especificados.</summary>
        public static void DrawArrowUp(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float x = rect.X + rect.Width / 2f;

                g.DrawLine(pen, x, rect.Bottom, x, rect.Y + rect.Height * 0.3f);
                g.DrawLine(pen, x, rect.Y + rect.Height * 0.3f, rect.X, rect.Y + rect.Height * 0.5f);
                g.DrawLine(pen, x, rect.Y + rect.Height * 0.3f, rect.Right, rect.Y + rect.Height * 0.5f);
            }
        }

        /// <summary>Dibuja el icono ArrowDown dentro de los límites especificados.</summary>
        public static void DrawArrowDown(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float x = rect.X + rect.Width / 2f;

                g.DrawLine(pen, x, rect.Y, x, rect.Bottom - rect.Height * 0.3f);
                g.DrawLine(pen, x, rect.Bottom - rect.Height * 0.3f, rect.X, rect.Y + rect.Height * 0.5f);
                g.DrawLine(pen, x, rect.Bottom - rect.Height * 0.3f, rect.Right, rect.Y + rect.Height * 0.5f);
            }
        }

        // ==========================
        // ICONOS ALERT
        // ==========================

        /// <summary>Dibuja el icono Alert dentro de los límites especificados.</summary>
        public static void DrawAlert(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            using SolidBrush brush = new SolidBrush(color);
            g.DrawEllipse(pen, rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.06f,
                rect.Width * 0.88f, rect.Height * 0.88f);
            float centerX = rect.X + rect.Width / 2f;
            g.DrawLine(pen, centerX, rect.Y + rect.Height * 0.24f,
                centerX, rect.Y + rect.Height * 0.60f);
            float dot = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.09f);
            g.FillEllipse(brush, centerX - dot / 2f, rect.Y + rect.Height * 0.73f, dot, dot);
        }

        // ==========================
        // ICONOS B
        // ==========================

        /// <summary>Dibuja el icono Bell dentro de los límites especificados.</summary>
        public static void DrawBell(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            using SolidBrush brush = new SolidBrush(color);
            using GraphicsPath bell = new GraphicsPath();
            float centerX = rect.X + rect.Width * 0.50f;
            float lipY = rect.Y + rect.Height * 0.72f;
            bell.AddBezier(
                rect.X + rect.Width * 0.17f, lipY,
                rect.X + rect.Width * 0.29f, rect.Y + rect.Height * 0.57f,
                rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.18f,
                centerX, rect.Y + rect.Height * 0.14f);
            bell.AddBezier(
                centerX, rect.Y + rect.Height * 0.14f,
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.18f,
                rect.X + rect.Width * 0.71f, rect.Y + rect.Height * 0.57f,
                rect.Right - rect.Width * 0.17f, lipY);
            g.DrawPath(pen, bell);
            g.DrawLine(pen, rect.X + rect.Width * 0.10f, lipY,
                rect.Right - rect.Width * 0.10f, lipY);
            g.DrawLine(pen, centerX, rect.Y + rect.Height * 0.06f,
                centerX, rect.Y + rect.Height * 0.14f);
            float clapper = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.12f);
            g.FillEllipse(brush, centerX - clapper / 2f,
                rect.Y + rect.Height * 0.82f, clapper, clapper);
        }

        /// <summary>Dibuja el icono Bookmark dentro de los límites especificados.</summary>
        public static void DrawBookmark(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            PointF[] bookmark = {
                new PointF(rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.06f),
                new PointF(rect.Right - rect.Width * 0.20f, rect.Y + rect.Height * 0.06f),
                new PointF(rect.Right - rect.Width * 0.20f, rect.Bottom - rect.Height * 0.05f),
                new PointF(rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.72f),
                new PointF(rect.X + rect.Width * 0.20f, rect.Bottom - rect.Height * 0.05f)
            };
            g.DrawPolygon(pen, bookmark);
        }

        /// <summary>Dibuja el icono Bug dentro de los límites especificados.</summary>
        public static void DrawBug(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            RectangleF body = new RectangleF(
                rect.X + rect.Width * 0.28f,
                rect.Y + rect.Height * 0.25f,
                rect.Width * 0.44f,
                rect.Height * 0.65f);
            g.DrawEllipse(pen, body);
            g.DrawArc(pen,
                rect.X + rect.Width * 0.36f,
                rect.Y + rect.Height * 0.09f,
                rect.Width * 0.28f,
                rect.Height * 0.30f,
                180, 180);
            float centerX = rect.X + rect.Width * 0.50f;
            g.DrawLine(pen, centerX, rect.Y + rect.Height * 0.25f,
                centerX, rect.Bottom - rect.Height * 0.10f);
            g.DrawLine(pen, rect.X + rect.Width * 0.40f, rect.Y + rect.Height * 0.13f,
                rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.02f);
            g.DrawLine(pen, rect.X + rect.Width * 0.60f, rect.Y + rect.Height * 0.13f,
                rect.Right - rect.Width * 0.27f, rect.Y + rect.Height * 0.02f);
            float[] legY = { 0.40f, 0.58f, 0.76f };
            foreach(float yRatio in legY) {
                float y = rect.Y + rect.Height * yRatio;
                g.DrawLine(pen, body.Left, y, rect.X + rect.Width * 0.08f,
                    y - rect.Height * 0.08f);
                g.DrawLine(pen, body.Right, y, rect.Right - rect.Width * 0.08f,
                    y - rect.Height * 0.08f);
            }
        }

        // ==========================
        // ICONOS C
        // ==========================

        /// <summary>Dibuja el icono Check dentro de los límites especificados.</summary>
        public static void DrawCheck(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen,
                    rect.X + rect.Width * 0.2f,
                    rect.Y + rect.Height * 0.5f,
                    rect.X + rect.Width * 0.4f,
                    rect.Bottom - rect.Height * 0.2f);

                g.DrawLine(pen,
                    rect.X + rect.Width * 0.4f,
                    rect.Bottom - rect.Height * 0.2f,
                    rect.Right - rect.Width * 0.2f,
                    rect.Y + rect.Height * 0.2f);
            }
        }

        /// <summary>Dibuja el icono CheckCircle dentro de los límites especificados.</summary>
        public static void DrawCheckCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
                DrawCheck(g, rect, color);
            }
        }

        /// <summary>Dibuja el icono CloseCircle dentro de los límites especificados.</summary>
        public static void DrawCloseCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
            }
            Rectangle inner = Rectangle.Inflate(rect, -Math.Max(3, rect.Width / 5),
                -Math.Max(3, rect.Height / 5));
            if(inner.Width >= 4 && inner.Height >= 4) {
                DrawClose(g, inner, color);
            }
        }

        /// <summary>Dibuja el icono Cloud dentro de los límites especificados.</summary>
        public static void DrawCloud(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            using GraphicsPath cloud = new GraphicsPath();
            cloud.StartFigure();
            cloud.AddBezier(
                rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.82f,
                rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.82f,
                rect.X + rect.Width * 0.02f, rect.Y + rect.Height * 0.55f,
                rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.50f);
            cloud.AddBezier(
                rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.50f,
                rect.X + rect.Width * 0.23f, rect.Y + rect.Height * 0.32f,
                rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.24f,
                rect.X + rect.Width * 0.52f, rect.Y + rect.Height * 0.31f);
            cloud.AddBezier(
                rect.X + rect.Width * 0.52f, rect.Y + rect.Height * 0.31f,
                rect.X + rect.Width * 0.62f, rect.Y + rect.Height * 0.05f,
                rect.X + rect.Width * 0.86f, rect.Y + rect.Height * 0.18f,
                rect.X + rect.Width * 0.83f, rect.Y + rect.Height * 0.46f);
            cloud.AddBezier(
                rect.X + rect.Width * 0.83f, rect.Y + rect.Height * 0.46f,
                rect.Right - rect.Width * 0.01f, rect.Y + rect.Height * 0.50f,
                rect.Right - rect.Width * 0.01f, rect.Y + rect.Height * 0.82f,
                rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.82f);
            cloud.AddLine(rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.82f,
                rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.82f);
            cloud.CloseFigure();
            g.DrawPath(pen, cloud);
        }

        /// <summary>Dibuja el icono Copy dentro de los límites especificados.</summary>
        public static void DrawCopy(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen,
                    rect.X + 3,
                    rect.Y,
                    rect.Width - 3,
                    rect.Height - 3);

                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y + 3,
                    rect.Width - 3,
                    rect.Height - 3);
            }
        }

        // ==========================
        // ICONOS D
        // ==========================

        /// <summary>Dibuja el icono Delete dentro de los límites especificados.</summary>
        public static void DrawDelete(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            float left = rect.X + rect.Width * 0.22f;
            float right = rect.Right - rect.Width * 0.22f;
            float top = rect.Y + rect.Height * 0.28f;
            float bottom = rect.Bottom - rect.Height * 0.06f;
            g.DrawLine(pen, rect.X + rect.Width * 0.10f, top,
                rect.Right - rect.Width * 0.10f, top);
            g.DrawLine(pen, rect.X + rect.Width * 0.36f, rect.Y + rect.Height * 0.14f,
                rect.X + rect.Width * 0.64f, rect.Y + rect.Height * 0.14f);
            g.DrawLine(pen, left, top, left + rect.Width * 0.06f, bottom);
            g.DrawLine(pen, right, top, right - rect.Width * 0.06f, bottom);
            g.DrawLine(pen, left + rect.Width * 0.06f, bottom,
                right - rect.Width * 0.06f, bottom);
            g.DrawLine(pen, rect.X + rect.Width * 0.42f, rect.Y + rect.Height * 0.42f,
                rect.X + rect.Width * 0.44f, rect.Y + rect.Height * 0.76f);
            g.DrawLine(pen, rect.X + rect.Width * 0.58f, rect.Y + rect.Height * 0.42f,
                rect.X + rect.Width * 0.56f, rect.Y + rect.Height * 0.76f);
        }

        /// <summary>Dibuja el icono DeleteCircle dentro de los límites especificados.</summary>
        public static void DrawDeleteCircle(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = CreatePen(color, rect, 1.6f)) {
                g.DrawEllipse(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            }
            Rectangle inner = Rectangle.Inflate(rect, -Math.Max(3, rect.Width / 6),
                -Math.Max(3, rect.Height / 6));
            if(inner.Width >= 4 && inner.Height >= 4) {
                DrawDelete(g, inner, color);
            }
        }

        /// <summary>Dibuja el icono Download dentro de los límites especificados.</summary>
        public static void DrawDownload(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2f;

                g.DrawLine(pen, cx, rect.Y, cx, rect.Bottom - rect.Height * 0.3f);

                g.DrawLine(pen,
                    cx,
                    rect.Bottom - rect.Height * 0.3f,
                    rect.X + rect.Width * 0.3f,
                    rect.Bottom - rect.Height * 0.5f);

                g.DrawLine(pen,
                    cx,
                    rect.Bottom - rect.Height * 0.3f,
                    rect.Right - rect.Width * 0.3f,
                    rect.Bottom - rect.Height * 0.5f);

                g.DrawLine(pen,
                    rect.X + rect.Width * 0.2f,
                    rect.Bottom,
                    rect.Right - rect.Width * 0.2f,
                    rect.Bottom);
            }
        }

        /// <summary>Dibuja el icono Database dentro de los límites especificados.</summary>
        public static void DrawDatabase(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height / 3);

                g.DrawLine(pen, rect.X, rect.Y + rect.Height / 6, rect.X, rect.Bottom - rect.Height / 6);
                g.DrawLine(pen, rect.Right, rect.Y + rect.Height / 6, rect.Right, rect.Bottom - rect.Height / 6);

                g.DrawEllipse(pen, rect.X, rect.Bottom - rect.Height / 3, rect.Width, rect.Height / 3);
            }
        }

        // ==========================
        // ICONOS E
        // ==========================

        /// <summary>Dibuja el icono Edit dentro de los límites especificados.</summary>
        public static void DrawEdit(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            using SolidBrush brush = new SolidBrush(color);
            PointF tip = new PointF(
                rect.X + rect.Width * 0.10f,
                rect.Bottom - rect.Height * 0.08f);
            PointF lowerCorner = new PointF(
                rect.X + rect.Width * 0.34f,
                rect.Bottom - rect.Height * 0.13f);
            PointF upperCorner = new PointF(
                rect.X + rect.Width * 0.18f,
                rect.Y + rect.Height * 0.71f);
            PointF eraserTop = new PointF(
                rect.X + rect.Width * 0.68f,
                rect.Y + rect.Height * 0.21f);
            PointF eraserBottom = new PointF(
                rect.X + rect.Width * 0.82f,
                rect.Y + rect.Height * 0.35f);
            PointF[] body = { upperCorner, eraserTop, eraserBottom, lowerCorner };
            g.DrawPolygon(pen, body);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.60f, rect.Y + rect.Height * 0.29f,
                rect.X + rect.Width * 0.74f, rect.Y + rect.Height * 0.43f);
            g.DrawLine(pen, tip, upperCorner);
            g.DrawLine(pen, tip, lowerCorner);
            g.DrawLine(pen, upperCorner, lowerCorner);
            float graphite = Math.Max(1.5f, Math.Min(rect.Width, rect.Height) * 0.07f);
            g.FillEllipse(brush, tip.X - graphite / 2f, tip.Y - graphite / 2f,
                graphite, graphite);
        }

        /// <summary>Dibuja el icono EditSquare dentro de los límites especificados.</summary>
        public static void DrawEditSquare(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            float left = rect.X + rect.Width * 0.06f;
            float top = rect.Y + rect.Height * 0.20f;
            float right = rect.Right - rect.Width * 0.20f;
            float bottom = rect.Bottom - rect.Height * 0.06f;
            g.DrawLine(pen, left, top, rect.X + rect.Width * 0.49f, top);
            g.DrawLine(pen, left, top, left, bottom);
            g.DrawLine(pen, left, bottom, right, bottom);
            g.DrawLine(pen, right, bottom, right, rect.Y + rect.Height * 0.51f);
            Rectangle pencilBounds = new Rectangle(
                rect.X + (int)(rect.Width * 0.25f),
                rect.Y,
                Math.Max(4, (int)(rect.Width * 0.75f)),
                Math.Max(4, (int)(rect.Height * 0.75f)));
            DrawEdit(g, pencilBounds, color);
        }

        /// <summary>Dibuja el icono ExitDoor dentro de los límites especificados.</summary>
        public static void DrawExitDoor(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Marco de la puerta
                g.DrawLines(pen, new Point[] {
                    new Point(rect.X + rect.Width/2, rect.Bottom),
                    new Point(rect.X + rect.Width/2, rect.Y),
                    new Point(rect.Right, rect.Y),
                    new Point(rect.Right, rect.Bottom),
                    new Point(rect.X + rect.Width/2, rect.Bottom)
                });
                // Flecha saliendo
                g.DrawLine(pen, rect.X, rect.Y + rect.Height / 2, rect.X + rect.Width / 2 + 2, rect.Y + rect.Height / 2);
                g.DrawLine(pen, rect.X + 5, rect.Y + rect.Height / 2 - 5, rect.X, rect.Y + rect.Height / 2);
                g.DrawLine(pen, rect.X + 5, rect.Y + rect.Height / 2 + 5, rect.X, rect.Y + rect.Height / 2);
            }
        }

        /// <summary>Dibuja el icono Eye dentro de los límites especificados.</summary>
        public static void DrawEye(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawArc(pen, rect, 0, 180);
                g.DrawArc(pen, rect, 180, 180);

                g.DrawEllipse(pen,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height / 3,
                    rect.Width / 3,
                    rect.Height / 3);
            }
        }

        /// <summary>Dibuja el icono EyeOff dentro de los límites especificados.</summary>
        public static void DrawEyeOff(Graphics g, Rectangle rect, Color color) {
            DrawEye(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Bottom, rect.Right, rect.Y);
            }
        }

        // ==========================
        // ICONOS F
        // ==========================

        /// <summary>Dibuja el icono File dentro de los límites especificados.</summary>
        public static void DrawFile(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
            }
        }

        /// <summary>Dibuja el icono FileText dentro de los límites especificados.</summary>
        public static void DrawFileText(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);

                g.DrawLine(pen, rect.X + 4, rect.Y + 6, rect.Right - 4, rect.Y + 6);
                g.DrawLine(pen, rect.X + 4, rect.Y + 10, rect.Right - 4, rect.Y + 10);
            }
        }

        /// <summary>Dibuja el icono Folder dentro de los límites especificados.</summary>
        public static void DrawFolder(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y + rect.Height / 4,
                    rect.Width,
                    rect.Height * 3 / 4);

                g.DrawLine(pen,
                    rect.X,
                    rect.Y + rect.Height / 4,
                    rect.X + rect.Width / 3,
                    rect.Y);
            }
        }

        /// <summary>Dibuja el icono Filter dentro de los límites especificados.</summary>
        public static void DrawFilter(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                Point p1 = new Point(rect.X, rect.Y);
                Point p2 = new Point(rect.Right, rect.Y);
                Point p3 = new Point(rect.X + rect.Width / 2, rect.Bottom);

                g.DrawPolygon(pen, new[] { p1, p2, p3 });
            }
        }

        // ==========================
        // ICONOS G
        // ==========================

        /// <summary>Dibuja el icono Gear dentro de los límites especificados.</summary>
        public static void DrawGear(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.6f);
            float centerX = rect.X + rect.Width / 2f;
            float centerY = rect.Y + rect.Height / 2f;
            PointF[] teeth = new PointF[32];
            for(int index = 0; index < teeth.Length; index++) {
                double angle = -Math.PI / 2d + index * Math.PI / 16d;
                int toothPosition = index % 4;
                float radius = toothPosition == 1 || toothPosition == 2 ? 0.48f : 0.37f;
                teeth[index] = new PointF(
                    centerX + (float)Math.Cos(angle) * rect.Width * radius,
                    centerY + (float)Math.Sin(angle) * rect.Height * radius);
            }
            g.DrawPolygon(pen, teeth);
            g.DrawEllipse(pen,
                rect.X + rect.Width * 0.34f,
                rect.Y + rect.Height * 0.34f,
                rect.Width * 0.32f,
                rect.Height * 0.32f);
        }

        /// <summary>Dibuja el icono Grid dentro de los límites especificados.</summary>
        public static void DrawGrid(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int w = rect.Width / 3;
                int h = rect.Height / 3;

                for(int i = 0; i < 3; i++) {
                    for(int j = 0; j < 3; j++) {
                        g.DrawRectangle(pen,
                            rect.X + i * w,
                            rect.Y + j * h,
                            w - 2,
                            h - 2);
                    }
                }
            }
        }

        /// <summary>Dibuja el icono Graph dentro de los límites especificados.</summary>
        public static void DrawGraph(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen,
                    rect.X,
                    rect.Bottom,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height / 2);

                g.DrawLine(pen,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height / 2,
                    rect.Right,
                    rect.Y);
            }
        }

        // ==========================
        // ICONOS H
        // ==========================

        /// <summary>Dibuja el icono Heart dentro de los límites especificados.</summary>
        public static void DrawHeart(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2))
            using(GraphicsPath path = new GraphicsPath()) {

                path.AddBezier(
                    rect.X + rect.Width / 2, rect.Bottom,
                    rect.X - rect.Width * 0.2f, rect.Y + rect.Height * 0.6f,
                    rect.X + rect.Width * 0.2f, rect.Y,
                    rect.X + rect.Width / 2, rect.Y + rect.Height * 0.3f);

                path.AddBezier(
                    rect.X + rect.Width / 2, rect.Y + rect.Height * 0.3f,
                    rect.Right - rect.Width * 0.2f, rect.Y,
                    rect.Right + rect.Width * 0.2f, rect.Y + rect.Height * 0.6f,
                    rect.X + rect.Width / 2, rect.Bottom);

                g.DrawPath(pen, path);
            }
        }

        /// <summary>Dibuja el icono Home dentro de los límites especificados.</summary>
        public static void DrawHome(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Point roofTop = new Point(rect.X + rect.Width / 2, rect.Y);
                Point left = new Point(rect.X, rect.Y + rect.Height / 2);
                Point right = new Point(rect.Right, rect.Y + rect.Height / 2);

                g.DrawPolygon(pen, new[] { roofTop, right, left });

                g.DrawRectangle(pen,
                    rect.X + rect.Width / 4,
                    rect.Y + rect.Height / 2,
                    rect.Width / 2,
                    rect.Height / 2);
            }
        }

        /// <summary>Dibuja el icono Help dentro de los límites especificados.</summary>
        public static void DrawHelp(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            using SolidBrush brush = new SolidBrush(color);
            g.DrawEllipse(pen,
                rect.X + rect.Width * 0.05f,
                rect.Y + rect.Height * 0.05f,
                rect.Width * 0.90f,
                rect.Height * 0.90f);
            using GraphicsPath question = new GraphicsPath();
            question.AddBezier(
                rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.34f,
                rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.18f,
                rect.X + rect.Width * 0.70f, rect.Y + rect.Height * 0.19f,
                rect.X + rect.Width * 0.70f, rect.Y + rect.Height * 0.38f);
            question.AddBezier(
                rect.X + rect.Width * 0.70f, rect.Y + rect.Height * 0.38f,
                rect.X + rect.Width * 0.70f, rect.Y + rect.Height * 0.51f,
                rect.X + rect.Width * 0.51f, rect.Y + rect.Height * 0.50f,
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.63f);
            g.DrawPath(pen, question);
            float dot = Math.Max(1.5f, Math.Min(rect.Width, rect.Height) * 0.08f);
            g.FillEllipse(brush,
                rect.X + rect.Width * 0.50f - dot / 2f,
                rect.Y + rect.Height * 0.75f,
                dot, dot);
        }

        // ==========================
        // ICONOS I
        // ==========================

        /// <summary>Dibuja el icono Info dentro de los límites especificados.</summary>
        public static void DrawInfo(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height * 0.4f,
                    rect.X + rect.Width / 2,
                    rect.Bottom - rect.Height * 0.2f);

                g.DrawEllipse(pen,
                    rect.X + rect.Width / 2 - 1,
                    rect.Y + rect.Height * 0.2f,
                    2, 2);
            }
        }

        /// <summary>Dibuja el icono Image dentro de los límites especificados.</summary>
        public static void DrawImage(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);

                // montaña
                g.DrawLine(pen,
                    rect.X,
                    rect.Bottom,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height / 2);

                g.DrawLine(pen,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height / 2,
                    rect.Right,
                    rect.Bottom);

                // sol
                g.DrawEllipse(pen,
                    rect.Right - rect.Width / 4,
                    rect.Y + rect.Height / 6,
                    rect.Width / 8,
                    rect.Height / 8);
            }
        }

        // ==========================
        // ICONOS J
        // ==========================

        /// <summary>Dibuja el icono JustifyLeft dentro de los límites especificados.</summary>
        public static void DrawJustifyLeft(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                for(int i = 0; i < 4; i++) {
                    float y = rect.Y + (i + 1) * rect.Height / 5f;
                    g.DrawLine(pen, rect.X, y, rect.Right - rect.Width * (i % 2 == 0 ? 0 : 0.3f), y);
                }
            }
        }

        /// <summary>Dibuja el icono JustifyCenter dentro de los límites especificados.</summary>
        public static void DrawJustifyCenter(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                for(int i = 0; i < 4; i++) {
                    float y = rect.Y + (i + 1) * rect.Height / 5f;
                    float w = rect.Width * (i % 2 == 0 ? 0.8f : 0.5f);

                    g.DrawLine(pen,
                        rect.X + (rect.Width - w) / 2,
                        y,
                        rect.X + (rect.Width + w) / 2,
                        y);
                }
            }
        }

        // ==========================
        // ICONOS K
        // ==========================

        /// <summary>Dibuja el icono Key dentro de los límites especificados.</summary>
        public static void DrawKey(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            RectangleF ring = new RectangleF(
                rect.X + rect.Width * 0.05f,
                rect.Y + rect.Height * 0.24f,
                rect.Width * 0.36f,
                rect.Height * 0.36f);
            g.DrawEllipse(pen, ring);
            float shaftY = rect.Y + rect.Height * 0.42f;
            g.DrawLine(pen, ring.Right, shaftY,
                rect.Right - rect.Width * 0.06f, shaftY);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.68f, shaftY,
                rect.X + rect.Width * 0.68f, rect.Y + rect.Height * 0.65f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.68f, rect.Y + rect.Height * 0.65f,
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.65f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.65f,
                rect.X + rect.Width * 0.80f, shaftY);
        }

        // ==========================
        // ICONOS L
        // ==========================

        /// <summary>Dibuja el icono Lock dentro de los límites especificados.</summary>
        public static void DrawLock(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            float bodyLeft = rect.X + rect.Width * 0.14f;
            float bodyTop = rect.Y + rect.Height * 0.43f;
            float bodyWidth = rect.Width * 0.72f;
            float bodyHeight = rect.Height * 0.49f;
            g.DrawRectangle(pen, bodyLeft, bodyTop, bodyWidth, bodyHeight);
            RectangleF shackle = new RectangleF(
                rect.X + rect.Width * 0.30f,
                rect.Y + rect.Height * 0.07f,
                rect.Width * 0.40f,
                rect.Height * 0.55f);
            g.DrawArc(pen, shackle, 180, 180);
            g.DrawLine(pen, shackle.Left, shackle.Y + shackle.Height / 2f,
                shackle.Left, bodyTop);
            g.DrawLine(pen, shackle.Right, shackle.Y + shackle.Height / 2f,
                shackle.Right, bodyTop);
        }

        /// <summary>Dibuja el icono Unlock dentro de los límites especificados.</summary>
        public static void DrawUnlock(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            float bodyLeft = rect.X + rect.Width * 0.14f;
            float bodyTop = rect.Y + rect.Height * 0.47f;
            g.DrawRectangle(pen, bodyLeft, bodyTop,
                rect.Width * 0.72f, rect.Height * 0.45f);
            using GraphicsPath shackle = new GraphicsPath();
            shackle.StartFigure();
            shackle.AddLine(
                rect.X + rect.Width * 0.30f, bodyTop,
                rect.X + rect.Width * 0.30f, rect.Y + rect.Height * 0.29f);
            shackle.AddBezier(
                rect.X + rect.Width * 0.30f, rect.Y + rect.Height * 0.29f,
                rect.X + rect.Width * 0.30f, rect.Y + rect.Height * 0.05f,
                rect.X + rect.Width * 0.72f, rect.Y + rect.Height * 0.05f,
                rect.X + rect.Width * 0.72f, rect.Y + rect.Height * 0.29f);
            shackle.AddLine(
                rect.X + rect.Width * 0.72f, rect.Y + rect.Height * 0.29f,
                rect.X + rect.Width * 0.84f, rect.Y + rect.Height * 0.29f);
            g.DrawPath(pen, shackle);
        }

        /// <summary>Dibuja el icono Link dentro de los límites especificados.</summary>
        public static void DrawLink(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Rectangle left = new Rectangle(rect.X, rect.Y + rect.Height / 4, rect.Width / 2, rect.Height / 2);
                Rectangle right = new Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 4, rect.Width / 2, rect.Height / 2);

                g.DrawArc(pen, left, 270, 180);
                g.DrawArc(pen, right, 90, 180);
            }
        }

        // ==========================
        // ICONOS M
        // ==========================

        /// <summary>Dibuja el icono Menu dentro de los límites especificados.</summary>
        public static void DrawMenu(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                for(int i = 0; i < 3; i++) {
                    float y = rect.Y + (i + 1) * rect.Height / 4f;
                    g.DrawLine(pen, rect.X, y, rect.Right, y);
                }
            }
        }

        /// <summary>Dibuja el icono MoreHorizontal dentro de los límites especificados.</summary>
        public static void DrawMoreHorizontal(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(SolidBrush brush = new SolidBrush(color)) {
                float y = rect.Y + rect.Height / 2f;

                for(int i = 0; i < 3; i++) {
                    float x = rect.X + rect.Width * (0.3f + i * 0.2f);
                    g.FillEllipse(brush, x, y, 3, 3);
                }
            }
        }

        /// <summary>Dibuja el icono MoreVertical dentro de los límites especificados.</summary>
        public static void DrawMoreVertical(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(SolidBrush brush = new SolidBrush(color)) {
                float x = rect.X + rect.Width / 2f;

                for(int i = 0; i < 3; i++) {
                    float y = rect.Y + rect.Height * (0.3f + i * 0.2f);
                    g.FillEllipse(brush, x, y, 3, 3);
                }
            }
        }

        /// <summary>Dibuja el icono Minus dentro de los límites especificados.</summary>
        public static void DrawMinus(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                float y = rect.Y + rect.Height / 2f;
                g.DrawLine(pen, rect.X + rect.Width * 0.2f, y, rect.Right - rect.Width * 0.2f, y);
            }
        }

        // ==========================
        // ICONOS MAIL / MESSAGE
        // ==========================

        /// <summary>Dibuja el icono Mail dentro de los límites especificados.</summary>
        public static void DrawMail(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);

                g.DrawLine(pen,
                    rect.X,
                    rect.Y,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2);

                g.DrawLine(pen,
                    rect.Right,
                    rect.Y,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2);
            }
        }

        /// <summary>Dibuja el icono Message dentro de los límites especificados.</summary>
        public static void DrawMessage(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y,
                    rect.Width,
                    rect.Height * 0.7f);

                g.DrawLine(pen,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height * 0.7f,
                    rect.X + rect.Width / 2,
                    rect.Bottom);

                g.DrawLine(pen,
                    rect.X + rect.Width * 2 / 3,
                    rect.Y + rect.Height * 0.7f,
                    rect.X + rect.Width / 2,
                    rect.Bottom);
            }
        }

        // ==========================
        // ICONOS MAP / MEDIA
        // ==========================

        /// <summary>Dibuja el icono Map dentro de los límites especificados.</summary>
        public static void DrawMap(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int w = rect.Width / 3;

                g.DrawLine(pen, rect.X + w, rect.Y, rect.X + w, rect.Bottom);
                g.DrawLine(pen, rect.X + w * 2, rect.Y, rect.X + w * 2, rect.Bottom);
            }
        }

        /// <summary>Dibuja el icono MapPin dentro de los límites especificados.</summary>
        public static void DrawMapPin(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen,
                    rect.X + rect.Width / 4,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height / 2);

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2,
                    rect.X + rect.Width / 2,
                    rect.Bottom);
            }
        }

        // ==========================
        // ICONOS MEDIA (PLAYBACK)
        // ==========================

        /// <summary>Dibuja el icono Play dentro de los límites especificados.</summary>
        public static void DrawPlay(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Point p1 = new Point(rect.X, rect.Y);
                Point p2 = new Point(rect.Right, rect.Y + rect.Height / 2);
                Point p3 = new Point(rect.X, rect.Bottom);

                g.DrawPolygon(pen, new[] { p1, p2, p3 });
            }
        }

        /// <summary>Dibuja el icono Pause dentro de los límites especificados.</summary>
        public static void DrawPause(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int w = rect.Width / 3;

                g.DrawRectangle(pen, rect.X, rect.Y, w, rect.Height);
                g.DrawRectangle(pen, rect.Right - w, rect.Y, w, rect.Height);
            }
        }

        /// <summary>Dibuja el icono Stop dentro de los límites especificados.</summary>
        public static void DrawStop(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
            }
        }

        // ==========================
        // ICONOS PHONE
        // ==========================

        /// <summary>Dibuja el icono Phone dentro de los límites especificados.</summary>
        public static void DrawPhone(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            using GraphicsPath handset = new GraphicsPath();
            handset.StartFigure();
            handset.AddLine(
                rect.X + rect.Width * 0.19f, rect.Y + rect.Height * 0.07f,
                rect.X + rect.Width * 0.36f, rect.Y + rect.Height * 0.23f);
            handset.AddBezier(
                rect.X + rect.Width * 0.36f, rect.Y + rect.Height * 0.23f,
                rect.X + rect.Width * 0.40f, rect.Y + rect.Height * 0.27f,
                rect.X + rect.Width * 0.39f, rect.Y + rect.Height * 0.32f,
                rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.37f);
            handset.AddLine(
                rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.37f,
                rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.44f);
            handset.AddBezier(
                rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.44f,
                rect.X + rect.Width * 0.35f, rect.Y + rect.Height * 0.60f,
                rect.X + rect.Width * 0.46f, rect.Y + rect.Height * 0.70f,
                rect.X + rect.Width * 0.59f, rect.Y + rect.Height * 0.77f);
            handset.AddLine(
                rect.X + rect.Width * 0.59f, rect.Y + rect.Height * 0.77f,
                rect.X + rect.Width * 0.66f, rect.Y + rect.Height * 0.69f);
            handset.AddBezier(
                rect.X + rect.Width * 0.66f, rect.Y + rect.Height * 0.69f,
                rect.X + rect.Width * 0.71f, rect.Y + rect.Height * 0.64f,
                rect.X + rect.Width * 0.76f, rect.Y + rect.Height * 0.64f,
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.68f);
            handset.AddLine(
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.68f,
                rect.X + rect.Width * 0.93f, rect.Y + rect.Height * 0.84f);
            handset.AddBezier(
                rect.X + rect.Width * 0.93f, rect.Y + rect.Height * 0.84f,
                rect.X + rect.Width * 0.87f, rect.Y + rect.Height * 0.95f,
                rect.X + rect.Width * 0.76f, rect.Y + rect.Height * 0.96f,
                rect.X + rect.Width * 0.66f, rect.Y + rect.Height * 0.92f);
            handset.AddBezier(
                rect.X + rect.Width * 0.66f, rect.Y + rect.Height * 0.92f,
                rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.79f,
                rect.X + rect.Width * 0.09f, rect.Y + rect.Height * 0.51f,
                rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.23f);
            handset.AddBezier(
                rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.23f,
                rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.15f,
                rect.X + rect.Width * 0.11f, rect.Y + rect.Height * 0.09f,
                rect.X + rect.Width * 0.19f, rect.Y + rect.Height * 0.07f);
            handset.CloseFigure();
            g.DrawPath(pen, handset);
        }

        // ==========================
        // ICONOS PLUS
        // ==========================

        /// <summary>Dibuja el icono Plus dentro de los límites especificados.</summary>
        public static void DrawPlus(Graphics g, Rectangle rect, Color color) {
            DrawAdd(g, rect, color);
        }

        /// <summary>Dibuja el icono PlusCircle dentro de los límites especificados.</summary>
        public static void DrawPlusCircle(Graphics g, Rectangle rect, Color color) {
            DrawAddCircle(g, rect, color);
        }

        /// <summary>Dibuja el icono PlusSquare dentro de los límites especificados.</summary>
        public static void DrawPlusSquare(Graphics g, Rectangle rect, Color color) {
            DrawAddSquare(g, rect, color);
        }


        // ==========================
        // ICONOS Q
        // ==========================

        /// <summary>Dibuja el icono QrCode dentro de los límites especificados.</summary>
        public static void DrawQrCode(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int size = rect.Width / 4;

                // esquinas
                g.DrawRectangle(pen, rect.X, rect.Y, size, size);
                g.DrawRectangle(pen, rect.Right - size, rect.Y, size, size);
                g.DrawRectangle(pen, rect.X, rect.Bottom - size, size, size);

                // centro
                g.DrawRectangle(pen,
                    rect.X + rect.Width / 3,
                    rect.Y + rect.Height / 3,
                    size,
                    size);
            }
        }

        // ==========================
        // ICONOS R
        // ==========================

        /// <summary>Dibuja el icono Refresh dentro de los límites especificados.</summary>
        public static void DrawRefresh(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.9f);
            RectangleF circle = new RectangleF(
                rect.X + rect.Width * 0.09f,
                rect.Y + rect.Height * 0.09f,
                rect.Width * 0.82f,
                rect.Height * 0.82f);
            g.DrawArc(pen, circle, 90, 270);
            PointF arrowTip = new PointF(
                rect.X + rect.Width * 0.91f,
                rect.Y + rect.Height * 0.50f);
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.72f, rect.Y + rect.Height * 0.34f));
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.91f, rect.Y + rect.Height * 0.25f));
        }

        /// <summary>Dibuja el icono Reload dentro de los límites especificados.</summary>
        public static void DrawReload(Graphics g, Rectangle rect, Color color) {
            DrawRefresh(g, rect, color);
        }

        // ==========================
        // ICONOS S
        // ==========================

        /// <summary>Dibuja el icono SearchPlus dentro de los límites especificados.</summary>
        public static void DrawSearchPlus(Graphics g, Rectangle rect, Color color) {
            DrawSearch(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                int size = rect.Width / 5;

                int x = rect.Right - size;
                int y = rect.Y + size;

                g.DrawLine(pen, x, y, x + size, y);
                g.DrawLine(pen, x + size / 2, y - size / 2, x + size / 2, y + size / 2);
            }
        }

        /// <summary>Dibuja el icono SearchMinus dentro de los límites especificados.</summary>
        public static void DrawSearchMinus(Graphics g, Rectangle rect, Color color) {
            DrawSearch(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                int size = rect.Width / 5;

                int x = rect.Right - size;
                int y = rect.Y + size;

                g.DrawLine(pen, x, y, x + size, y);
            }
        }

        /// <summary>Dibuja el icono Shield dentro de los límites especificados.</summary>
        public static void DrawShield(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            using GraphicsPath shield = new GraphicsPath();
            shield.StartFigure();
            shield.AddLine(
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.05f,
                rect.X + rect.Width * 0.86f, rect.Y + rect.Height * 0.19f);
            shield.AddLine(
                rect.X + rect.Width * 0.86f, rect.Y + rect.Height * 0.19f,
                rect.X + rect.Width * 0.82f, rect.Y + rect.Height * 0.55f);
            shield.AddBezier(
                rect.X + rect.Width * 0.82f, rect.Y + rect.Height * 0.55f,
                rect.X + rect.Width * 0.79f, rect.Y + rect.Height * 0.75f,
                rect.X + rect.Width * 0.62f, rect.Y + rect.Height * 0.89f,
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.95f);
            shield.AddBezier(
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.95f,
                rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.89f,
                rect.X + rect.Width * 0.21f, rect.Y + rect.Height * 0.75f,
                rect.X + rect.Width * 0.18f, rect.Y + rect.Height * 0.55f);
            shield.AddLine(
                rect.X + rect.Width * 0.18f, rect.Y + rect.Height * 0.55f,
                rect.X + rect.Width * 0.14f, rect.Y + rect.Height * 0.19f);
            shield.CloseFigure();
            g.DrawPath(pen, shield);
        }

        /// <summary>Dibuja el icono Star dentro de los límites especificados.</summary>
        public static void DrawStar(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                PointF[] pts = new PointF[10];
                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;
                float rx = rect.Width / 2f;
                float ry = rect.Height / 2f;
                float innerR = 0.45f; // Proporción del centro

                for(int i = 0; i < 10; i++) {
                    float r = (i % 2 == 0) ? 1 : innerR;
                    double angle = Math.PI * i / 5 - Math.PI / 2;
                    pts[i] = new PointF(cx + (float)(Math.Cos(angle) * rx * r),
                                        cy + (float)(Math.Sin(angle) * ry * r));
                }
                g.DrawPolygon(pen, pts);
            }
        }

        /// <summary>Dibuja el icono StarHalf dentro de los límites especificados.</summary>
        public static void DrawStarHalf(Graphics g, Rectangle rect, Color color) {
            DrawStar(g, rect, color);

            using(SolidBrush brush = new SolidBrush(color)) {
                g.FillRectangle(brush,
                    rect.X,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height);
            }
        }

        /// <summary>Dibuja el icono Sort dentro de los límites especificados.</summary>
        public static void DrawSort(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Y);
                g.DrawLine(pen, rect.X + rect.Width * 0.2f, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2);
                g.DrawLine(pen, rect.X + rect.Width * 0.4f, rect.Bottom, rect.Right, rect.Bottom);
            }
        }

        // ==========================
        // ICONOS T
        // ==========================

        /// <summary>Dibuja el icono Tag dentro de los límites especificados.</summary>
        public static void DrawTag(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Point p1 = new Point(rect.X, rect.Y);
                Point p2 = new Point(rect.Right, rect.Y);
                Point p3 = new Point(rect.Right, rect.Bottom);
                Point p4 = new Point(rect.X + rect.Width / 2, rect.Bottom);

                g.DrawPolygon(pen, new[] { p1, p2, p3, p4 });

                g.DrawEllipse(pen,
                    rect.X + rect.Width * 0.7f,
                    rect.Y + rect.Height * 0.2f,
                    3, 3);
            }
        }

        /// <summary>Dibuja el icono TrashAlt dentro de los límites especificados.</summary>
        public static void DrawTrashAlt(Graphics g, Rectangle rect, Color color) {
            DrawDelete(g, rect, color);
        }

        /// <summary>Dibuja el icono Timer dentro de los límites especificados.</summary>
        public static void DrawTimer(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height * 0.2f);

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2,
                    rect.Right - rect.Width * 0.2f,
                    rect.Y + rect.Height / 2);
            }
        }

        /// <summary>Dibuja el icono ToggleOn dentro de los límites especificados.</summary>
        public static void DrawToggleOn(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.6f);
            using SolidBrush brush = new SolidBrush(color);
            Rectangle track = new Rectangle(
                rect.X,
                rect.Y + (int)(rect.Height * 0.18f),
                rect.Width,
                Math.Max(4, (int)(rect.Height * 0.64f)));
            using GraphicsPath trackPath = CreateRoundedPath(track, track.Height / 2f);
            g.DrawPath(pen, trackPath);
            float knob = track.Height * 0.66f;
            g.FillEllipse(brush,
                track.Right - track.Height * 0.83f,
                track.Y + (track.Height - knob) / 2f,
                knob, knob);
        }

        /// <summary>Dibuja el icono ToggleOff dentro de los límites especificados.</summary>
        public static void DrawToggleOff(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.6f);
            using SolidBrush brush = new SolidBrush(color);
            Rectangle track = new Rectangle(
                rect.X,
                rect.Y + (int)(rect.Height * 0.18f),
                rect.Width,
                Math.Max(4, (int)(rect.Height * 0.64f)));
            using GraphicsPath trackPath = CreateRoundedPath(track, track.Height / 2f);
            g.DrawPath(pen, trackPath);
            float knob = track.Height * 0.66f;
            g.FillEllipse(brush,
                track.X + track.Height * 0.17f,
                track.Y + (track.Height - knob) / 2f,
                knob, knob);
        }

        // ==========================
        // ICONOS U
        // ==========================

        /// <summary>Dibuja el icono Upload dentro de los límites especificados.</summary>
        public static void DrawUpload(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2f;

                g.DrawLine(pen, cx, rect.Bottom, cx, rect.Y + rect.Height * 0.3f);

                g.DrawLine(pen,
                    cx,
                    rect.Y + rect.Height * 0.3f,
                    rect.X + rect.Width * 0.3f,
                    rect.Y + rect.Height * 0.5f);

                g.DrawLine(pen,
                    cx,
                    rect.Y + rect.Height * 0.3f,
                    rect.Right - rect.Width * 0.3f,
                    rect.Y + rect.Height * 0.5f);

                g.DrawLine(pen,
                    rect.X + rect.Width * 0.2f,
                    rect.Bottom,
                    rect.Right - rect.Width * 0.2f,
                    rect.Bottom);
            }
        }

        // ==========================
        // ICONOS USER (VARIANTES PRO)
        // ==========================

        /// <summary>Dibuja el icono UserCheck dentro de los límites especificados.</summary>
        public static void DrawUserCheck(Graphics g, Rectangle rect, Color color) {
            DrawUser(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen,
                    rect.Right - rect.Width * 0.3f,
                    rect.Bottom - rect.Height * 0.3f,
                    rect.Right - rect.Width * 0.2f,
                    rect.Bottom - rect.Height * 0.2f);

                g.DrawLine(pen,
                    rect.Right - rect.Width * 0.2f,
                    rect.Bottom - rect.Height * 0.2f,
                    rect.Right,
                    rect.Bottom - rect.Height * 0.4f);
            }
        }

        /// <summary>Dibuja el icono UserMinus dentro de los límites especificados.</summary>
        public static void DrawUserMinus(Graphics g, Rectangle rect, Color color) {
            DrawUser(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                float y = rect.Bottom - rect.Height * 0.25f;

                g.DrawLine(pen,
                    rect.Right - rect.Width * 0.3f,
                    y,
                    rect.Right,
                    y);
            }
        }

        // ==========================
        // ICONOS V
        // ==========================

        /// <summary>Dibuja el icono Video dentro de los límites especificados.</summary>
        public static void DrawVideo(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            g.DrawRectangle(pen,
                rect.X + rect.Width * 0.04f,
                rect.Y + rect.Height * 0.20f,
                rect.Width * 0.61f,
                rect.Height * 0.60f);
            PointF[] lens = {
                new PointF(rect.X + rect.Width * 0.65f, rect.Y + rect.Height * 0.39f),
                new PointF(rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.21f),
                new PointF(rect.Right - rect.Width * 0.04f, rect.Bottom - rect.Height * 0.21f),
                new PointF(rect.X + rect.Width * 0.65f, rect.Y + rect.Height * 0.61f)
            };
            g.DrawPolygon(pen, lens);
        }

        /// <summary>Dibuja el icono Volume dentro de los límites especificados.</summary>
        public static void DrawVolume(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Point p1 = new Point(rect.X, rect.Y + rect.Height / 3);
                Point p2 = new Point(rect.X + rect.Width / 3, rect.Y + rect.Height / 3);
                Point p3 = new Point(rect.X + rect.Width / 2, rect.Y);
                Point p4 = new Point(rect.X + rect.Width / 2, rect.Bottom);
                Point p5 = new Point(rect.X + rect.Width / 3, rect.Y + rect.Height * 2 / 3);
                Point p6 = new Point(rect.X, rect.Y + rect.Height * 2 / 3);

                g.DrawPolygon(pen, new[] { p1, p2, p3, p4, p5, p6 });

                g.DrawArc(pen, rect, -45, 90);
            }
        }

        /// <summary>Dibuja el icono VolumeOff dentro de los límites especificados.</summary>
        public static void DrawVolumeOff(Graphics g, Rectangle rect, Color color) {
            DrawVolume(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
            }
        }

        // ==========================
        // ICONOS W
        // ==========================

        /// <summary>Dibuja el icono Warning dentro de los límites especificados.</summary>
        public static void DrawWarning(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            using SolidBrush brush = new SolidBrush(color);
            PointF top = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height * 0.05f);
            PointF right = new PointF(rect.Right - rect.Width * 0.04f, rect.Bottom - rect.Height * 0.06f);
            PointF left = new PointF(rect.X + rect.Width * 0.04f, rect.Bottom - rect.Height * 0.06f);
            g.DrawPolygon(pen, new[] { top, right, left });
            float centerX = rect.X + rect.Width / 2f;
            g.DrawLine(pen, centerX, rect.Y + rect.Height * 0.32f,
                centerX, rect.Y + rect.Height * 0.62f);
            float dot = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.08f);
            g.FillEllipse(brush, centerX - dot / 2f, rect.Y + rect.Height * 0.73f, dot, dot);
        }

        /// <summary>Dibuja el icono Wifi dentro de los límites especificados.</summary>
        public static void DrawWifi(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            using SolidBrush brush = new SolidBrush(color);
            g.DrawArc(pen,
                rect.X + rect.Width * 0.06f,
                rect.Y + rect.Height * 0.04f,
                rect.Width * 0.88f,
                rect.Height * 0.70f,
                205, 130);
            g.DrawArc(pen,
                rect.X + rect.Width * 0.24f,
                rect.Y + rect.Height * 0.31f,
                rect.Width * 0.52f,
                rect.Height * 0.42f,
                205, 130);
            float dotSize = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.14f);
            g.FillEllipse(brush,
                rect.X + rect.Width * 0.50f - dotSize / 2f,
                rect.Y + rect.Height * 0.79f,
                dotSize, dotSize);
        }

        // ==========================
        // ICONOS X
        // ==========================

        /// <summary>Dibuja el icono X dentro de los límites especificados.</summary>
        public static void DrawX(Graphics g, Rectangle rect, Color color) {
            DrawClose(g, rect, color);
        }

        // ==========================
        // ICONOS Y
        // ==========================

        /// <summary>Dibuja el icono Yen dentro de los límites especificados.</summary>
        public static void DrawYen(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.9f);
            float centerX = rect.X + rect.Width * 0.50f;
            float junctionY = rect.Y + rect.Height * 0.46f;
            g.DrawLine(pen,
                rect.X + rect.Width * 0.12f, rect.Y + rect.Height * 0.06f,
                centerX, junctionY);
            g.DrawLine(pen,
                rect.Right - rect.Width * 0.12f, rect.Y + rect.Height * 0.06f,
                centerX, junctionY);
            g.DrawLine(pen, centerX, junctionY,
                centerX, rect.Bottom - rect.Height * 0.05f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.58f,
                rect.Right - rect.Width * 0.22f, rect.Y + rect.Height * 0.58f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.74f,
                rect.Right - rect.Width * 0.22f, rect.Y + rect.Height * 0.74f);
        }

        // ==========================
        // ICONOS Z
        // ==========================

        /// <summary>Dibuja el icono ZoomIn dentro de los límites especificados.</summary>
        public static void DrawZoomIn(Graphics g, Rectangle rect, Color color) {
            DrawSearch(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2;
                float cy = rect.Y + rect.Height / 2;

                g.DrawLine(pen, cx - 3, cy, cx + 3, cy);
                g.DrawLine(pen, cx, cy - 3, cx, cy + 3);
            }
        }

        /// <summary>Dibuja el icono ZoomOut dentro de los límites especificados.</summary>
        public static void DrawZoomOut(Graphics g, Rectangle rect, Color color) {
            DrawSearch(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2;
                float cy = rect.Y + rect.Height / 2;

                g.DrawLine(pen, cx - 3, cy, cx + 3, cy);
            }
        }



        // ==========================
        // ICONOS UTILIDAD EXTRA
        // ==========================

        /// <summary>Dibuja el icono Close dentro de los límites especificados.</summary>
        public static void DrawClose(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = CreatePen(color, rect, 2.5f)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
            }
        }



        /// <summary>Dibuja el icono Bulb dentro de los límites especificados.</summary>
        public static void DrawBulb(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            using GraphicsPath bulb = new GraphicsPath();
            bulb.StartFigure();
            bulb.AddBezier(
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.08f,
                rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.08f,
                rect.X + rect.Width * 0.17f, rect.Y + rect.Height * 0.25f,
                rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.43f);
            bulb.AddBezier(
                rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.43f,
                rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.55f,
                rect.X + rect.Width * 0.35f, rect.Y + rect.Height * 0.61f,
                rect.X + rect.Width * 0.37f, rect.Y + rect.Height * 0.69f);
            bulb.AddLine(
                rect.X + rect.Width * 0.37f, rect.Y + rect.Height * 0.69f,
                rect.X + rect.Width * 0.63f, rect.Y + rect.Height * 0.69f);
            bulb.AddBezier(
                rect.X + rect.Width * 0.63f, rect.Y + rect.Height * 0.69f,
                rect.X + rect.Width * 0.65f, rect.Y + rect.Height * 0.61f,
                rect.X + rect.Width * 0.78f, rect.Y + rect.Height * 0.55f,
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.43f);
            bulb.AddBezier(
                rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.43f,
                rect.X + rect.Width * 0.83f, rect.Y + rect.Height * 0.25f,
                rect.X + rect.Width * 0.73f, rect.Y + rect.Height * 0.08f,
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.08f);
            g.DrawPath(pen, bulb);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.36f, rect.Y + rect.Height * 0.77f,
                rect.X + rect.Width * 0.64f, rect.Y + rect.Height * 0.77f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.39f, rect.Y + rect.Height * 0.86f,
                rect.X + rect.Width * 0.61f, rect.Y + rect.Height * 0.86f);
            g.DrawArc(pen,
                rect.X + rect.Width * 0.43f, rect.Y + rect.Height * 0.82f,
                rect.Width * 0.14f, rect.Height * 0.13f,
                0, 180);
        }

        /// <summary>Dibuja el icono Report dentro de los límites especificados.</summary>
        public static void DrawReport(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                // Líneas de texto simuladas
                g.DrawLine(pen, rect.X + 4, rect.Y + 6, rect.Right - 4, rect.Y + 6);
                g.DrawLine(pen, rect.X + 4, rect.Y + 10, rect.Right - 4, rect.Y + 10);
                g.DrawLine(pen, rect.X + 4, rect.Y + 14, rect.Right - 8, rect.Y + 14);
            }
        }

        /// <summary>Dibuja el icono Settings dentro de los límites especificados.</summary>
        public static void DrawSettings(Graphics g, Rectangle rect, Color color) {
            DrawGear(g, rect, color);
        }

        /// <summary>Dibuja el icono Calendar dentro de los límites especificados.</summary>
        public static void DrawCalendar(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            float left = rect.X + rect.Width * 0.07f;
            float top = rect.Y + rect.Height * 0.14f;
            float width = rect.Width * 0.86f;
            float height = rect.Height * 0.78f;
            g.DrawRectangle(pen, left, top, width, height);
            g.DrawLine(pen, left, rect.Y + rect.Height * 0.37f,
                left + width, rect.Y + rect.Height * 0.37f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.29f, rect.Y + rect.Height * 0.04f,
                rect.X + rect.Width * 0.29f, rect.Y + rect.Height * 0.24f);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.71f, rect.Y + rect.Height * 0.04f,
                rect.X + rect.Width * 0.71f, rect.Y + rect.Height * 0.24f);
            using SolidBrush brush = new SolidBrush(color);
            float dot = Math.Max(1.5f, Math.Min(rect.Width, rect.Height) * 0.07f);
            for(int row = 0; row < 2; row++) {
                for(int column = 0; column < 3; column++) {
                    g.FillEllipse(brush,
                        rect.X + rect.Width * (0.28f + column * 0.22f) - dot / 2f,
                        rect.Y + rect.Height * (0.55f + row * 0.19f) - dot / 2f,
                        dot, dot);
                }
            }
        }

        /// <summary>Dibuja el icono User dentro de los límites especificados.</summary>
        public static void DrawUser(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int headSize = rect.Width / 2;
                g.DrawEllipse(pen, rect.X + (rect.Width - headSize) / 2, rect.Y, headSize, headSize);
                g.DrawArc(pen, rect.X, rect.Y + (rect.Height / 2), rect.Width, rect.Height, 180, 180);
            }
        }

        /// <summary>Dibuja el icono UserFilled dentro de los límites especificados.</summary>
        public static void DrawUserFilled(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(SolidBrush brush = new SolidBrush(color)) {
                int head = rect.Width / 3;

                g.FillEllipse(brush,
                    rect.X + (rect.Width - head) / 2,
                    rect.Y,
                    head,
                    head);

                g.FillEllipse(brush,
                    rect.X,
                    rect.Y + head,
                    rect.Width,
                    rect.Height - head);
            }
        }

        /// <summary>Dibuja el icono Users dentro de los límites especificados.</summary>
        public static void DrawUsers(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawEllipse(pen,
                rect.X + rect.Width * 0.12f,
                rect.Y + rect.Height * 0.20f,
                rect.Width * 0.24f,
                rect.Height * 0.24f);
            using(GraphicsPath backPerson = new GraphicsPath()) {
                backPerson.AddBezier(
                    rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.78f,
                    rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.57f,
                    rect.X + rect.Width * 0.23f, rect.Y + rect.Height * 0.48f,
                    rect.X + rect.Width * 0.39f, rect.Y + rect.Height * 0.57f);
                g.DrawPath(pen, backPerson);
            }
            g.DrawEllipse(pen,
                rect.X + rect.Width * 0.46f,
                rect.Y + rect.Height * 0.07f,
                rect.Width * 0.29f,
                rect.Height * 0.29f);
            using GraphicsPath frontPerson = new GraphicsPath();
            frontPerson.AddBezier(
                rect.X + rect.Width * 0.28f, rect.Y + rect.Height * 0.91f,
                rect.X + rect.Width * 0.30f, rect.Y + rect.Height * 0.59f,
                rect.X + rect.Width * 0.53f, rect.Y + rect.Height * 0.46f,
                rect.X + rect.Width * 0.61f, rect.Y + rect.Height * 0.46f);
            frontPerson.AddBezier(
                rect.X + rect.Width * 0.61f, rect.Y + rect.Height * 0.46f,
                rect.X + rect.Width * 0.77f, rect.Y + rect.Height * 0.46f,
                rect.X + rect.Width * 0.93f, rect.Y + rect.Height * 0.64f,
                rect.X + rect.Width * 0.94f, rect.Y + rect.Height * 0.91f);
            g.DrawPath(pen, frontPerson);
        }

        /// <summary>Dibuja el icono UserPlus dentro de los límites especificados.</summary>
        public static void DrawUserPlus(Graphics g, Rectangle rect, Color color) {
            DrawUser(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                int size = rect.Width / 4;

                int x = rect.Right - size;
                int y = rect.Y + size;

                g.DrawLine(pen, x, y, x + size, y);
                g.DrawLine(pen, x + size / 2, y - size / 2, x + size / 2, y + size / 2);
            }
        }

        /// <summary>Dibuja el icono Search dentro de los límites especificados.</summary>
        public static void DrawSearch(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int circleSize = (int)(rect.Width * 0.7);
                g.DrawEllipse(pen, rect.X, rect.Y, circleSize, circleSize);
                g.DrawLine(pen, rect.X + (int)(circleSize * 0.8), rect.Y + (int)(circleSize * 0.8), rect.Right, rect.Bottom);
            }
        }

        /// <summary>Dibuja el icono Trash dentro de los límites especificados.</summary>
        public static void DrawTrash(Graphics g, Rectangle rect, Color color) {
            DrawDelete(g, rect, color);
        }

        /// <summary>Dibuja el icono Clear dentro de los límites especificados.</summary>
        public static void DrawClear(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            PointF tip = new PointF(rect.X + rect.Width * 0.12f, rect.Bottom - rect.Height * 0.17f);
            PointF topLeft = new PointF(rect.X + rect.Width * 0.54f, rect.Y + rect.Height * 0.08f);
            PointF topRight = new PointF(rect.Right - rect.Width * 0.07f, rect.Y + rect.Height * 0.42f);
            PointF bottom = new PointF(rect.X + rect.Width * 0.48f, rect.Bottom - rect.Height * 0.17f);
            g.DrawPolygon(pen, new[] { tip, topLeft, topRight, bottom });
            g.DrawLine(pen, rect.X + rect.Width * 0.32f, rect.Y + rect.Height * 0.52f,
                rect.X + rect.Width * 0.63f, rect.Y + rect.Height * 0.79f);
            g.DrawLine(pen, rect.X + rect.Width * 0.08f, rect.Bottom - rect.Height * 0.07f,
                rect.Right - rect.Width * 0.05f, rect.Bottom - rect.Height * 0.07f);
        }

        /// <summary>Dibuja el icono Stats dentro de los límites especificados.</summary>
        public static void DrawStats(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = rect.Width / 4;
                g.DrawLine(pen, rect.X + w, rect.Bottom, rect.X + w, rect.Y + (rect.Height / 2));
                g.DrawLine(pen, rect.X + w * 2, rect.Bottom, rect.X + w * 2, rect.Y);
                g.DrawLine(pen, rect.X + w * 3, rect.Bottom, rect.X + w * 3, rect.Y + (rect.Height / 3));
            }
        }



        /// <summary>Dibuja el icono Logout dentro de los límites especificados.</summary>
        public static void DrawLogout(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            float frameLeft = rect.X + rect.Width * 0.08f;
            float frameRight = rect.X + rect.Width * 0.52f;
            float frameTop = rect.Y + rect.Height * 0.07f;
            float frameBottom = rect.Bottom - rect.Height * 0.07f;
            g.DrawLines(pen, new[] {
                new PointF(frameRight, frameTop),
                new PointF(frameLeft, frameTop),
                new PointF(frameLeft, frameBottom),
                new PointF(frameRight, frameBottom)
            });
            float arrowY = rect.Y + rect.Height * 0.50f;
            PointF arrowTip = new PointF(
                rect.Right - rect.Width * 0.04f, arrowY);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.30f, arrowY,
                arrowTip.X, arrowTip.Y);
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.73f, rect.Y + rect.Height * 0.31f));
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.73f, rect.Y + rect.Height * 0.69f));
        }

        /// <summary>Dibuja el icono Login dentro de los límites especificados.</summary>
        public static void DrawLogin(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            float frameLeft = rect.X + rect.Width * 0.48f;
            float frameRight = rect.Right - rect.Width * 0.08f;
            float frameTop = rect.Y + rect.Height * 0.07f;
            float frameBottom = rect.Bottom - rect.Height * 0.07f;
            g.DrawLines(pen, new[] {
                new PointF(frameLeft, frameTop),
                new PointF(frameRight, frameTop),
                new PointF(frameRight, frameBottom),
                new PointF(frameLeft, frameBottom)
            });
            float arrowY = rect.Y + rect.Height * 0.50f;
            PointF arrowTip = new PointF(
                rect.X + rect.Width * 0.68f, arrowY);
            g.DrawLine(pen,
                rect.X + rect.Width * 0.04f, arrowY,
                arrowTip.X, arrowTip.Y);
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.49f, rect.Y + rect.Height * 0.31f));
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.49f, rect.Y + rect.Height * 0.69f));
        }

        /// <summary>Dibuja el icono LockKey dentro de los límites especificados.</summary>
        public static void DrawLockKey(Graphics g, Rectangle rect, Color color) {
            DrawLock(g, rect, color);
            using Pen pen = CreatePen(color, rect, 1.5f);
            using SolidBrush brush = new SolidBrush(color);
            float keyhole = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.11f);
            float centerX = rect.X + rect.Width * 0.50f;
            float centerY = rect.Y + rect.Height * 0.62f;
            g.FillEllipse(brush, centerX - keyhole / 2f, centerY - keyhole / 2f,
                keyhole, keyhole);
            g.DrawLine(pen, centerX, centerY,
                centerX, rect.Y + rect.Height * 0.80f);
        }

        /// <summary>Dibuja el icono Notification dentro de los límites especificados.</summary>
        public static void DrawNotification(Graphics g, Rectangle rect, Color color) {
            Rectangle bellBounds = new Rectangle(
                rect.X,
                rect.Y + (int)(rect.Height * 0.10f),
                Math.Max(4, (int)(rect.Width * 0.82f)),
                Math.Max(4, (int)(rect.Height * 0.88f)));
            DrawBell(g, bellBounds, color);
            using SolidBrush brush = new SolidBrush(color);
            float badge = Math.Max(3f, Math.Min(rect.Width, rect.Height) * 0.20f);
            g.FillEllipse(brush,
                rect.Right - badge,
                rect.Y + rect.Height * 0.04f,
                badge, badge);
        }

        /// <summary>Dibuja el icono Dashboard dentro de los límites especificados.</summary>
        public static void DrawDashboard(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int w = rect.Width / 2;
                int h = rect.Height / 2;

                g.DrawRectangle(pen, rect.X, rect.Y, w - 2, h - 2);
                g.DrawRectangle(pen, rect.X + w, rect.Y, w - 2, h - 2);
                g.DrawRectangle(pen, rect.X, rect.Y + h, w - 2, h - 2);
                g.DrawRectangle(pen, rect.X + w, rect.Y + h, w - 2, h - 2);
            }
        }

        /// <summary>Dibuja el icono Clipboard dentro de los límites especificados.</summary>
        public static void DrawClipboard(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y + rect.Height * 0.2f,
                    rect.Width,
                    rect.Height * 0.8f);

                g.DrawRectangle(pen,
                    rect.X + rect.Width * 0.25f,
                    rect.Y,
                    rect.Width * 0.5f,
                    rect.Height * 0.3f);
            }
        }

        /// <summary>Dibuja el icono CheckSquare dentro de los límites especificados.</summary>
        public static void DrawCheckSquare(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
                DrawCheck(g, rect, color);
            }
        }

        /// <summary>Dibuja el icono CloseSquare dentro de los límites especificados.</summary>
        public static void DrawCloseSquare(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
                Rectangle inner = Rectangle.Inflate(rect, -Math.Max(2, rect.Width / 6),
                    -Math.Max(2, rect.Height / 6));
                DrawClose(g, inner, color);
            }
        }

        /// <summary>Dibuja el icono Power dentro de los límites especificados.</summary>
        public static void DrawPower(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.9f);
            RectangleF ring = new RectangleF(
                rect.X + rect.Width * 0.09f,
                rect.Y + rect.Height * 0.08f,
                rect.Width * 0.82f,
                rect.Height * 0.84f);
            g.DrawArc(pen, ring, -55, 290);
            float centerX = rect.X + rect.Width * 0.50f;
            g.DrawLine(pen,
                centerX, rect.Y + rect.Height * 0.03f,
                centerX, rect.Y + rect.Height * 0.50f);
        }

        /// <summary>Dibuja el icono ChevronDown dentro de los límites especificados.</summary>
        public static void DrawChevronDown(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(GraphicsPath path = new GraphicsPath())
            using(Pen pen = new Pen(color, 2.2f)) {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                float leftX = rect.X;
                float centerX = rect.X + rect.Width / 2f;
                float rightX = rect.Right;

                float topY = rect.Y + rect.Height * 0.25f;
                float bottomY = rect.Bottom - rect.Height * 0.25f;

                // Curva izquierda → centro → derecha
                path.AddBezier(
                    leftX, topY,
                    leftX + rect.Width * 0.15f, topY,
                    centerX - rect.Width * 0.15f, bottomY,
                    centerX, bottomY
                );

                path.AddBezier(
                    centerX, bottomY,
                    centerX + rect.Width * 0.15f, bottomY,
                    rightX - rect.Width * 0.15f, topY,
                    rightX, topY
                );

                g.DrawPath(pen, path);
            }
        }

        /// <summary>Dibuja el icono Sync dentro de los límites especificados.</summary>
        public static void DrawSync(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            using(GraphicsPath upper = new GraphicsPath()) {
                upper.AddBezier(
                    rect.X + rect.Width * 0.14f, rect.Y + rect.Height * 0.48f,
                    rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.15f,
                    rect.X + rect.Width * 0.68f, rect.Y + rect.Height * 0.10f,
                    rect.X + rect.Width * 0.86f, rect.Y + rect.Height * 0.35f);
                g.DrawPath(pen, upper);
            }
            PointF upperTip = new PointF(
                rect.X + rect.Width * 0.88f,
                rect.Y + rect.Height * 0.36f);
            g.DrawLine(pen, upperTip,
                new PointF(rect.X + rect.Width * 0.68f, rect.Y + rect.Height * 0.34f));
            g.DrawLine(pen, upperTip,
                new PointF(rect.X + rect.Width * 0.84f, rect.Y + rect.Height * 0.16f));
            using(GraphicsPath lower = new GraphicsPath()) {
                lower.AddBezier(
                    rect.X + rect.Width * 0.86f, rect.Y + rect.Height * 0.52f,
                    rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.85f,
                    rect.X + rect.Width * 0.32f, rect.Y + rect.Height * 0.90f,
                    rect.X + rect.Width * 0.14f, rect.Y + rect.Height * 0.65f);
                g.DrawPath(pen, lower);
            }
            PointF lowerTip = new PointF(
                rect.X + rect.Width * 0.12f,
                rect.Y + rect.Height * 0.64f);
            g.DrawLine(pen, lowerTip,
                new PointF(rect.X + rect.Width * 0.32f, rect.Y + rect.Height * 0.66f));
            g.DrawLine(pen, lowerTip,
                new PointF(rect.X + rect.Width * 0.16f, rect.Y + rect.Height * 0.84f));
        }

        /// <summary>Dibuja el icono Pin dentro de los límites especificados.</summary>
        public static void DrawPin(Graphics g, Rectangle rect, Color color) {
            DrawMapPin(g, rect, color);
        }

        /// <summary>Dibuja una caja de archivo con su tirador frontal.</summary>
        public static void DrawArchive(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            RectangleF body = new RectangleF(
                rect.X + rect.Width * 0.12f,
                rect.Y + rect.Height * 0.28f,
                rect.Width * 0.76f,
                rect.Height * 0.62f);
            g.DrawRectangle(pen, body.X, body.Y, body.Width, body.Height);
            g.DrawLine(pen, rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.28f,
                rect.Right - rect.Width * 0.06f, rect.Y + rect.Height * 0.28f);
            g.DrawLine(pen, rect.X + rect.Width * 0.12f, rect.Y + rect.Height * 0.10f,
                rect.Right - rect.Width * 0.12f, rect.Y + rect.Height * 0.10f);
            g.DrawLine(pen, rect.X + rect.Width * 0.40f, rect.Y + rect.Height * 0.52f,
                rect.X + rect.Width * 0.60f, rect.Y + rect.Height * 0.52f);
        }

        /// <summary>Dibuja un clip para representar un archivo adjunto.</summary>
        public static void DrawAttachment(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            using GraphicsPath path = new GraphicsPath();
            path.AddBezier(
                rect.X + rect.Width * 0.72f, rect.Y + rect.Height * 0.22f,
                rect.X + rect.Width * 0.98f, rect.Y + rect.Height * 0.48f,
                rect.X + rect.Width * 0.47f, rect.Bottom + rect.Height * 0.05f,
                rect.X + rect.Width * 0.19f, rect.Y + rect.Height * 0.73f);
            path.AddBezier(
                rect.X + rect.Width * 0.19f, rect.Y + rect.Height * 0.73f,
                rect.X - rect.Width * 0.03f, rect.Y + rect.Height * 0.50f,
                rect.X + rect.Width * 0.42f, rect.Y + rect.Height * 0.07f,
                rect.X + rect.Width * 0.61f, rect.Y + rect.Height * 0.27f);
            path.AddBezier(
                rect.X + rect.Width * 0.61f, rect.Y + rect.Height * 0.27f,
                rect.X + rect.Width * 0.73f, rect.Y + rect.Height * 0.39f,
                rect.X + rect.Width * 0.42f, rect.Y + rect.Height * 0.68f,
                rect.X + rect.Width * 0.32f, rect.Y + rect.Height * 0.57f);
            g.DrawPath(pen, path);
        }

        /// <summary>Dibuja una persona acompañada de una pluma de autor.</summary>
        public static void DrawAuthor(Graphics g, Rectangle rect, Color color) {
            Rectangle person = new Rectangle(rect.X, rect.Y, (int)(rect.Width * 0.62f), rect.Height);
            DrawUser(g, person, color);
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawLine(pen, rect.X + rect.Width * 0.55f, rect.Bottom - rect.Height * 0.12f,
                rect.Right - rect.Width * 0.05f, rect.Y + rect.Height * 0.28f);
            g.DrawLine(pen, rect.Right - rect.Width * 0.05f, rect.Y + rect.Height * 0.28f,
                rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.25f);
        }

        /// <summary>Dibuja un código de barras con barras de distinto grosor.</summary>
        public static void DrawBarcode(Graphics g, Rectangle rect, Color color) {
            using SolidBrush brush = new SolidBrush(color);
            int[] units = { 1, 2, 1, 1, 3, 1, 2, 1, 2, 1, 1, 2 };
            float unitWidth = rect.Width / 22f;
            float x = rect.X;
            for(int index = 0; index < units.Length; index++) {
                float width = Math.Max(1f, units[index] * unitWidth);
                if(index % 2 == 0) {
                    g.FillRectangle(brush, x, rect.Y + rect.Height * 0.08f, width, rect.Height * 0.84f);
                }
                x += width;
            }
        }

        /// <summary>Dibuja un libro abierto.</summary>
        public static void DrawBook(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            float middle = rect.X + rect.Width / 2f;
            PointF[] leftPage = {
                new PointF(middle, rect.Y + rect.Height * 0.18f),
                new PointF(rect.X + rect.Width * 0.08f, rect.Y + rect.Height * 0.08f),
                new PointF(rect.X + rect.Width * 0.08f, rect.Bottom - rect.Height * 0.12f),
                new PointF(middle, rect.Bottom - rect.Height * 0.02f)
            };
            PointF[] rightPage = {
                new PointF(middle, rect.Y + rect.Height * 0.18f),
                new PointF(rect.Right - rect.Width * 0.08f, rect.Y + rect.Height * 0.08f),
                new PointF(rect.Right - rect.Width * 0.08f, rect.Bottom - rect.Height * 0.12f),
                new PointF(middle, rect.Bottom - rect.Height * 0.02f)
            };
            g.DrawPolygon(pen, leftPage);
            g.DrawPolygon(pen, rightPage);
            g.DrawLine(pen, middle, rect.Y + rect.Height * 0.18f, middle, rect.Bottom - rect.Height * 0.02f);
        }

        /// <summary>Dibuja una cámara fotográfica.</summary>
        public static void DrawCamera(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            RectangleF body = new RectangleF(rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.27f,
                rect.Width * 0.90f, rect.Height * 0.62f);
            g.DrawRectangle(pen, body.X, body.Y, body.Width, body.Height);
            g.DrawEllipse(pen, rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.39f,
                rect.Width * 0.32f, rect.Height * 0.38f);
            g.DrawLines(pen, new[] {
                new PointF(rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.27f),
                new PointF(rect.X + rect.Width * 0.30f, rect.Y + rect.Height * 0.10f),
                new PointF(rect.X + rect.Width * 0.48f, rect.Y + rect.Height * 0.10f),
                new PointF(rect.X + rect.Width * 0.56f, rect.Y + rect.Height * 0.27f)
            });
        }

        /// <summary>Dibuja un carrito de compras.</summary>
        public static void DrawCart(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            g.DrawLine(pen, rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.15f,
                rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.15f);
            g.DrawLine(pen, rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.15f,
                rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.66f);
            g.DrawLine(pen, rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.66f,
                rect.Right - rect.Width * 0.12f, rect.Y + rect.Height * 0.66f);
            g.DrawLine(pen, rect.X + rect.Width * 0.28f, rect.Y + rect.Height * 0.30f,
                rect.Right - rect.Width * 0.06f, rect.Y + rect.Height * 0.30f);
            g.DrawLine(pen, rect.Right - rect.Width * 0.06f, rect.Y + rect.Height * 0.30f,
                rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.58f);
            g.DrawEllipse(pen, rect.X + rect.Width * 0.32f, rect.Bottom - rect.Height * 0.15f,
                rect.Width * 0.08f, rect.Height * 0.08f);
            g.DrawEllipse(pen, rect.Right - rect.Width * 0.22f, rect.Bottom - rect.Height * 0.15f,
                rect.Width * 0.08f, rect.Height * 0.08f);
        }

        /// <summary>Dibuja una ficha con numeral para representar un código identificador.</summary>
        public static void DrawCode(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.14f,
                rect.Width * 0.90f, rect.Height * 0.72f);
            float left = rect.X + rect.Width * 0.34f;
            float right = rect.X + rect.Width * 0.64f;
            float top = rect.Y + rect.Height * 0f;
            float bottom = rect.Bottom - rect.Height * 0.05f;
            g.DrawLine(pen, left, top, left - rect.Width * 0.10f, bottom);
            g.DrawLine(pen, right, top, right - rect.Width * 0.10f, bottom);
            g.DrawLine(pen, rect.X + rect.Width * 0.16f, rect.Y + rect.Height * 0.40f,
                rect.Right - rect.Width * 0.14f, rect.Y + rect.Height * 0.40f);
            g.DrawLine(pen, rect.X + rect.Width * 0.12f, rect.Y + rect.Height * 0.64f,
                rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.64f);
        }

        /// <summary>Dibuja los delimitadores angulares de código fuente.</summary>
        public static void DrawSourceCode(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            float centerY = rect.Y + rect.Height / 2f;
            g.DrawLines(pen, new[] {
                new PointF(rect.X + rect.Width * 0.30f, rect.Y + rect.Height * 0.20f),
                new PointF(rect.X + rect.Width * 0.06f, centerY),
                new PointF(rect.X + rect.Width * 0.30f, rect.Bottom - rect.Height * 0.20f)
            });
            g.DrawLine(pen, rect.X + rect.Width * 0.58f, rect.Y + rect.Height * 0.08f,
                rect.X + rect.Width * 0.42f, rect.Bottom - rect.Height * 0.08f);
            g.DrawLines(pen, new[] {
                new PointF(rect.X + rect.Width * 0.70f, rect.Y + rect.Height * 0.20f),
                new PointF(rect.Right - rect.Width * 0.06f, centerY),
                new PointF(rect.X + rect.Width * 0.70f, rect.Bottom - rect.Height * 0.20f)
            });
        }

        /// <summary>Dibuja un documento acompañado de un signo de suma.</summary>
        public static void DrawDocumentAdd(Graphics g, Rectangle rect, Color color) {
            Rectangle document = new Rectangle(rect.X, rect.Y, (int)(rect.Width * 0.70f), rect.Height);
            DrawFile(g, document, color);
            Rectangle add = new Rectangle(
                rect.X + (int)(rect.Width * 0.52f),
                rect.Y + (int)(rect.Height * 0.52f),
                (int)(rect.Width * 0.46f),
                (int)(rect.Height * 0.46f));
            DrawAddCircle(g, add, color);
        }

        /// <summary>Dibuja un documento con una flecha saliente.</summary>
        public static void DrawExport(Graphics g, Rectangle rect, Color color) {
            Rectangle document = new Rectangle(rect.X, rect.Y, (int)(rect.Width * 0.62f), rect.Height);
            DrawFile(g, document, color);
            using Pen pen = CreatePen(color, rect);
            float y = rect.Y + rect.Height * 0.48f;
            g.DrawLine(pen, rect.X + rect.Width * 0.38f, y, rect.Right - rect.Width * 0.05f, y);
            g.DrawLine(pen, rect.Right - rect.Width * 0.05f, y,
                rect.Right - rect.Width * 0.26f, rect.Y + rect.Height * 0.28f);
            g.DrawLine(pen, rect.Right - rect.Width * 0.05f, y,
                rect.Right - rect.Width * 0.26f, rect.Y + rect.Height * 0.68f);
        }

        /// <summary>Dibuja una carpeta abierta.</summary>
        public static void DrawFolderOpen(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            g.DrawLines(pen, new[] {
                new PointF(rect.X + rect.Width * 0.05f, rect.Bottom - rect.Height * 0.10f),
                new PointF(rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.22f),
                new PointF(rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.22f),
                new PointF(rect.X + rect.Width * 0.48f, rect.Y + rect.Height * 0.36f),
                new PointF(rect.Right - rect.Width * 0.05f, rect.Y + rect.Height * 0.36f)
            });
            g.DrawPolygon(pen, new[] {
                new PointF(rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.44f),
                new PointF(rect.Right - rect.Width * 0.02f, rect.Y + rect.Height * 0.44f),
                new PointF(rect.Right - rect.Width * 0.18f, rect.Bottom - rect.Height * 0.08f),
                new PointF(rect.X + rect.Width * 0.05f, rect.Bottom - rect.Height * 0.08f)
            });
        }

        /// <summary>Dibuja varias etiquetas para representar géneros o categorías.</summary>
        public static void DrawGenre(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.6f);
            for(int index = 0; index < 3; index++) {
                float y = rect.Y + rect.Height * (0.08f + index * 0.29f);
                PointF[] tag = {
                    new PointF(rect.X + rect.Width * 0.08f, y),
                    new PointF(rect.Right - rect.Width * 0.18f, y),
                    new PointF(rect.Right - rect.Width * 0.03f, y + rect.Height * 0.12f),
                    new PointF(rect.Right - rect.Width * 0.18f, y + rect.Height * 0.23f),
                    new PointF(rect.X + rect.Width * 0.08f, y + rect.Height * 0.23f)
                };
                g.DrawPolygon(pen, tag);
                g.DrawEllipse(pen, rect.X + rect.Width * 0.16f, y + rect.Height * 0.08f,
                    Math.Max(1f, rect.Width * 0.05f), Math.Max(1f, rect.Height * 0.05f));
            }
        }

        /// <summary>Dibuja un reloj con una flecha de historial.</summary>
        public static void DrawHistory(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.8f);
            RectangleF clock = new RectangleF(
                rect.X + rect.Width * 0.16f,
                rect.Y + rect.Height * 0.12f,
                rect.Width * 0.74f,
                rect.Height * 0.74f);
            g.DrawArc(pen, clock, -135, 300);
            float centerX = clock.X + clock.Width / 2f;
            float centerY = clock.Y + clock.Height / 2f;
            g.DrawLine(pen, centerX, centerY, centerX, clock.Y + clock.Height * 0.24f);
            g.DrawLine(pen, centerX, centerY, clock.X + clock.Width * 0.72f, centerY);
            PointF arrowTip = new PointF(
                rect.X + rect.Width * 0.15f,
                rect.Y + rect.Height * 0.26f);
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.16f, rect.Y + rect.Height * 0.50f));
            g.DrawLine(pen, arrowTip,
                new PointF(rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.30f));
        }

        /// <summary>Dibuja un documento con una flecha entrante.</summary>
        public static void DrawImport(Graphics g, Rectangle rect, Color color) {
            Rectangle document = new Rectangle(rect.X + (int)(rect.Width * 0.38f), rect.Y,
                (int)(rect.Width * 0.62f), rect.Height);
            DrawFile(g, document, color);
            using Pen pen = CreatePen(color, rect);
            float y = rect.Y + rect.Height * 0.48f;
            g.DrawLine(pen, rect.X + rect.Width * 0.05f, y, rect.X + rect.Width * 0.68f, y);
            g.DrawLine(pen, rect.X + rect.Width * 0.68f, y,
                rect.X + rect.Width * 0.47f, rect.Y + rect.Height * 0.28f);
            g.DrawLine(pen, rect.X + rect.Width * 0.68f, y,
                rect.X + rect.Width * 0.47f, rect.Y + rect.Height * 0.68f);
        }

        /// <summary>Dibuja una estantería con existencias.</summary>
        public static void DrawInventory(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.08f,
                rect.Width * 0.90f, rect.Height * 0.84f);
            g.DrawLine(pen, rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.53f,
                rect.Right - rect.Width * 0.05f, rect.Y + rect.Height * 0.53f);
            for(int row = 0; row < 2; row++) {
                for(int column = 0; column < 3; column++) {
                    float x = rect.X + rect.Width * (0.13f + column * 0.28f);
                    float y = rect.Y + rect.Height * (0.17f + row * 0.44f);
                    g.DrawRectangle(pen, x, y, rect.Width * 0.18f, rect.Height * 0.24f);
                }
            }
        }

        /// <summary>Dibuja un paquete recibiendo una flecha de carga.</summary>
        public static void DrawInventoryLoad(Graphics g, Rectangle rect, Color color) {
            Rectangle package = new Rectangle(rect.X, rect.Y + (int)(rect.Height * 0.34f),
                rect.Width, (int)(rect.Height * 0.66f));
            DrawPackage(g, package, color);
            using Pen pen = CreatePen(color, rect);
            float centerX = rect.X + rect.Width / 2f;
            g.DrawLine(pen, centerX, rect.Y, centerX, rect.Y + rect.Height * 0.42f);
            g.DrawLine(pen, centerX, rect.Y + rect.Height * 0.42f,
                centerX - rect.Width * 0.16f, rect.Y + rect.Height * 0.26f);
            g.DrawLine(pen, centerX, rect.Y + rect.Height * 0.42f,
                centerX + rect.Width * 0.16f, rect.Y + rect.Height * 0.26f);
        }

        /// <summary>Dibuja una lista con viñetas.</summary>
        public static void DrawList(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            using SolidBrush brush = new SolidBrush(color);
            for(int index = 0; index < 3; index++) {
                float y = rect.Y + rect.Height * (0.20f + index * 0.30f);
                float dot = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.10f);
                g.FillEllipse(brush, rect.X + rect.Width * 0.06f, y - dot / 2f, dot, dot);
                g.DrawLine(pen, rect.X + rect.Width * 0.28f, y,
                    rect.Right - rect.Width * 0.05f, y);
            }
        }

        /// <summary>Dibuja un monitor de escritorio.</summary>
        public static void DrawMonitor(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.05f, rect.Y + rect.Height * 0.08f,
                rect.Width * 0.90f, rect.Height * 0.62f);
            g.DrawLine(pen, rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.70f,
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.90f);
            g.DrawLine(pen, rect.X + rect.Width * 0.28f, rect.Y + rect.Height * 0.90f,
                rect.X + rect.Width * 0.72f, rect.Y + rect.Height * 0.90f);
        }

        /// <summary>Dibuja una caja cerrada para representar un producto o paquete.</summary>
        public static void DrawPackage(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            PointF top = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height * 0.05f);
            PointF left = new PointF(rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.28f);
            PointF right = new PointF(rect.Right - rect.Width * 0.06f, rect.Y + rect.Height * 0.28f);
            PointF bottomLeft = new PointF(left.X, rect.Bottom - rect.Height * 0.08f);
            PointF bottom = new PointF(top.X, rect.Bottom - rect.Height * 0.02f);
            PointF bottomRight = new PointF(right.X, bottomLeft.Y);
            g.DrawPolygon(pen, new[] { top, right, bottomRight, bottom, bottomLeft, left });
            g.DrawLine(pen, left, new PointF(top.X, rect.Y + rect.Height * 0.48f));
            g.DrawLine(pen, right, new PointF(top.X, rect.Y + rect.Height * 0.48f));
            g.DrawLine(pen, top.X, rect.Y + rect.Height * 0.48f, top.X, bottom.Y);
        }

        /// <summary>Dibuja una impresora con una hoja de papel.</summary>
        public static void DrawPrint(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.04f,
                rect.Width * 0.56f, rect.Height * 0.32f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.34f,
                rect.Width * 0.88f, rect.Height * 0.42f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.22f, rect.Y + rect.Height * 0.60f,
                rect.Width * 0.56f, rect.Height * 0.34f);
            using SolidBrush brush = new SolidBrush(color);
            g.FillEllipse(brush, rect.Right - rect.Width * 0.22f, rect.Y + rect.Height * 0.43f,
                Math.Max(2f, rect.Width * 0.07f), Math.Max(2f, rect.Height * 0.07f));
        }

        /// <summary>Dibuja un recibo con líneas de detalle.</summary>
        public static void DrawReceipt(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            PointF[] outline = {
                new PointF(rect.X + rect.Width * 0.15f, rect.Y + rect.Height * 0.05f),
                new PointF(rect.Right - rect.Width * 0.15f, rect.Y + rect.Height * 0.05f),
                new PointF(rect.Right - rect.Width * 0.15f, rect.Bottom - rect.Height * 0.06f),
                new PointF(rect.X + rect.Width * 0.72f, rect.Bottom - rect.Height * 0.16f),
                new PointF(rect.X + rect.Width * 0.55f, rect.Bottom - rect.Height * 0.06f),
                new PointF(rect.X + rect.Width * 0.38f, rect.Bottom - rect.Height * 0.16f),
                new PointF(rect.X + rect.Width * 0.15f, rect.Bottom - rect.Height * 0.06f)
            };
            g.DrawPolygon(pen, outline);
            for(int index = 0; index < 3; index++) {
                float y = rect.Y + rect.Height * (0.28f + index * 0.18f);
                g.DrawLine(pen, rect.X + rect.Width * 0.30f, y,
                    rect.Right - rect.Width * 0.30f, y);
            }
        }

        /// <summary>Dibuja una flecha curva hacia la derecha para rehacer.</summary>
        public static void DrawRedo(Graphics g, Rectangle rect, Color color) {
            DrawUndoRedo(g, rect, color, false);
        }

        /// <summary>Dibuja un disquete para representar la acción de guardar.</summary>
        public static void DrawSave(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.08f, rect.Y + rect.Height * 0.05f,
                rect.Width * 0.84f, rect.Height * 0.90f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.05f,
                rect.Width * 0.45f, rect.Height * 0.30f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.24f, rect.Y + rect.Height * 0.58f,
                rect.Width * 0.52f, rect.Height * 0.37f);
        }

        /// <summary>Dibuja un servidor con dos unidades.</summary>
        public static void DrawServer(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            using SolidBrush brush = new SolidBrush(color);
            for(int index = 0; index < 2; index++) {
                float y = rect.Y + rect.Height * (0.08f + index * 0.46f);
                g.DrawRectangle(pen, rect.X + rect.Width * 0.06f, y,
                    rect.Width * 0.88f, rect.Height * 0.34f);
                g.FillEllipse(brush, rect.X + rect.Width * 0.16f, y + rect.Height * 0.12f,
                    Math.Max(2f, rect.Width * 0.07f), Math.Max(2f, rect.Height * 0.07f));
                g.DrawLine(pen, rect.X + rect.Width * 0.40f, y + rect.Height * 0.17f,
                    rect.Right - rect.Width * 0.12f, y + rect.Height * 0.17f);
            }
        }

        /// <summary>Dibuja una tabla con filas y columnas.</summary>
        public static void DrawTable(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.08f,
                rect.Width * 0.92f, rect.Height * 0.84f);
            g.DrawLine(pen, rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.36f,
                rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.36f);
            g.DrawLine(pen, rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.64f,
                rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.64f);
            g.DrawLine(pen, rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.08f,
                rect.X + rect.Width * 0.38f, rect.Bottom - rect.Height * 0.08f);
            g.DrawLine(pen, rect.X + rect.Width * 0.70f, rect.Y + rect.Height * 0.08f,
                rect.X + rect.Width * 0.70f, rect.Bottom - rect.Height * 0.08f);
        }

        /// <summary>Dibuja una letra T con líneas de texto para representar un título.</summary>
        public static void DrawTitle(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.9f);
            g.DrawLine(pen, rect.X + rect.Width * 0.06f, rect.Y + rect.Height * 0.13f,
                rect.X + rect.Width * 0.55f, rect.Y + rect.Height * 0.13f);
            g.DrawLine(pen, rect.X + rect.Width * 0.305f, rect.Y + rect.Height * 0.13f,
                rect.X + rect.Width * 0.305f, rect.Bottom - rect.Height * 0.10f);
            g.DrawLine(pen, rect.X + rect.Width * 0.12f, rect.Bottom - rect.Height * 0.10f,
                rect.X + rect.Width * 0.49f, rect.Bottom - rect.Height * 0.10f);
            g.DrawLine(pen, rect.X + rect.Width * 0.66f, rect.Y + rect.Height * 0.38f,
                rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.38f);
            g.DrawLine(pen, rect.X + rect.Width * 0.66f, rect.Y + rect.Height * 0.64f,
                rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.64f);
        }

        /// <summary>Dibuja una flecha curva hacia la izquierda para deshacer.</summary>
        public static void DrawUndo(Graphics g, Rectangle rect, Color color) {
            DrawUndoRedo(g, rect, color, true);
        }

        /// <summary>Dibuja una ficha de registro con un indicador circular de actualización.</summary>
        public static void DrawUpdate(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.6f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.08f,
                rect.Width * 0.58f, rect.Height * 0.82f);
            g.DrawLine(pen, rect.X + rect.Width * 0.15f, rect.Y + rect.Height * 0.30f,
                rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.30f);
            g.DrawLine(pen, rect.X + rect.Width * 0.15f, rect.Y + rect.Height * 0.49f,
                rect.X + rect.Width * 0.43f, rect.Y + rect.Height * 0.49f);
            RectangleF badge = new RectangleF(rect.X + rect.Width * 0.48f, rect.Y + rect.Height * 0.42f,
                rect.Width * 0.48f, rect.Height * 0.48f);
            g.DrawArc(pen, badge, 90, 270);
            PointF arrowTip = new PointF(badge.Right, badge.Y + badge.Height * 0.50f);
            g.DrawLine(pen, arrowTip,
                new PointF(badge.X + badge.Width * 0.69f, badge.Y + badge.Height * 0.25f));
            g.DrawLine(pen, arrowTip,
                new PointF(badge.Right, badge.Y + badge.Height * 0.12f));
        }

        /// <summary>Dibuja la fachada de un almacén con cajas.</summary>
        public static void DrawWarehouse(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawLines(pen, new[] {
                new PointF(rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.32f),
                new PointF(rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.05f),
                new PointF(rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.32f)
            });
            g.DrawRectangle(pen, rect.X + rect.Width * 0.10f, rect.Y + rect.Height * 0.32f,
                rect.Width * 0.80f, rect.Height * 0.60f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.54f,
                rect.Width * 0.46f, rect.Height * 0.38f);
            g.DrawLine(pen, rect.X + rect.Width * 0.27f, rect.Y + rect.Height * 0.72f,
                rect.X + rect.Width * 0.73f, rect.Y + rect.Height * 0.72f);
            g.DrawLine(pen, rect.X + rect.Width * 0.50f, rect.Y + rect.Height * 0.54f,
                rect.X + rect.Width * 0.50f, rect.Bottom - rect.Height * 0.08f);
        }

        /// <summary>Dibuja una ventana de aplicación con barra de título.</summary>
        public static void DrawWindow(Graphics g, Rectangle rect, Color color) {
            using Pen pen = CreatePen(color, rect, 1.7f);
            g.DrawRectangle(pen, rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.08f,
                rect.Width * 0.92f, rect.Height * 0.84f);
            g.DrawLine(pen, rect.X + rect.Width * 0.04f, rect.Y + rect.Height * 0.32f,
                rect.Right - rect.Width * 0.04f, rect.Y + rect.Height * 0.32f);
            using SolidBrush brush = new SolidBrush(color);
            for(int index = 0; index < 3; index++) {
                float dot = Math.Max(2f, Math.Min(rect.Width, rect.Height) * 0.07f);
                g.FillEllipse(brush, rect.X + rect.Width * (0.14f + index * 0.14f),
                    rect.Y + rect.Height * 0.18f - dot / 2f, dot, dot);
            }
        }

        private static void DrawUndoRedo(Graphics g, Rectangle rect, Color color, bool undo) {
            using Pen pen = CreatePen(color, rect, 1.9f);
            using GraphicsPath curve = new GraphicsPath();
            if(undo) {
                PointF arrowTip = new PointF(
                    rect.X + rect.Width * 0.12f,
                    rect.Y + rect.Height * 0.42f);
                curve.AddBezier(
                    rect.X + rect.Width * 0.18f, rect.Y + rect.Height * 0.42f,
                    rect.X + rect.Width * 0.34f, rect.Y + rect.Height * 0.17f,
                    rect.X + rect.Width * 0.75f, rect.Y + rect.Height * 0.18f,
                    rect.X + rect.Width * 0.85f, rect.Y + rect.Height * 0.48f);
                curve.AddBezier(
                    rect.X + rect.Width * 0.85f, rect.Y + rect.Height * 0.48f,
                    rect.X + rect.Width * 0.91f, rect.Y + rect.Height * 0.66f,
                    rect.X + rect.Width * 0.80f, rect.Y + rect.Height * 0.83f,
                    rect.X + rect.Width * 0.65f, rect.Y + rect.Height * 0.85f);
                g.DrawPath(pen, curve);
                g.DrawLine(pen, arrowTip,
                    new PointF(rect.X + rect.Width * 0.18f, rect.Y + rect.Height * 0.42f));
                g.DrawLine(pen, arrowTip,
                    new PointF(rect.X + rect.Width * 0.33f, rect.Y + rect.Height * 0.21f));
                g.DrawLine(pen, arrowTip,
                    new PointF(rect.X + rect.Width * 0.33f, rect.Y + rect.Height * 0.63f));
            } else {
                PointF arrowTip = new PointF(
                    rect.Right - rect.Width * 0.12f,
                    rect.Y + rect.Height * 0.42f);
                curve.AddBezier(
                    rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.42f,
                    rect.Right - rect.Width * 0.34f, rect.Y + rect.Height * 0.17f,
                    rect.Right - rect.Width * 0.75f, rect.Y + rect.Height * 0.18f,
                    rect.Right - rect.Width * 0.85f, rect.Y + rect.Height * 0.48f);
                curve.AddBezier(
                    rect.Right - rect.Width * 0.85f, rect.Y + rect.Height * 0.48f,
                    rect.Right - rect.Width * 0.91f, rect.Y + rect.Height * 0.66f,
                    rect.Right - rect.Width * 0.80f, rect.Y + rect.Height * 0.83f,
                    rect.Right - rect.Width * 0.65f, rect.Y + rect.Height * 0.85f);
                g.DrawPath(pen, curve);
                g.DrawLine(pen, arrowTip,
                    new PointF(rect.Right - rect.Width * 0.18f, rect.Y + rect.Height * 0.42f));
                g.DrawLine(pen, arrowTip,
                    new PointF(rect.Right - rect.Width * 0.33f, rect.Y + rect.Height * 0.21f));
                g.DrawLine(pen, arrowTip,
                    new PointF(rect.Right - rect.Width * 0.33f, rect.Y + rect.Height * 0.63f));
            }
        }



    }
}
