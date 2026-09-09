using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using static System.ComponentModel.TypeConverter;

namespace Sara_UI_Design.SaraControls {
    public static class SaraUI_IconLibrary {

        private static Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();
        public enum SaraIconStyle {
            Outline,
            Filled,
            Circle,
            Square,
            Rounded
        }

        public static void DrawIcon(string iconName, Graphics g, Rectangle rect, Color color, SaraIconStyle style) {
            switch(style) {
                case SaraIconStyle.Filled:
                DrawFilled(iconName, g, rect, color);
                break;

                case SaraIconStyle.Circle:
                DrawCircle(iconName, g, rect, color);
                break;

                case SaraIconStyle.Square:
                DrawSquare(iconName, g, rect, color);
                break;

                case SaraIconStyle.Rounded:
                DrawRounded(iconName, g, rect, color);
                break;

                default:
                DrawIcon(iconName, g, rect, color);
                break;
            }
        }

        public static void DrawIcon(string iconName, Graphics g, Rectangle rect, Color color) {
            if(string.IsNullOrEmpty(iconName) || iconName == "None")
                return;

            try {
                string methodName = "Draw" + iconName;
                if(!_methodCache.ContainsKey(methodName)) {
                    var method = typeof(SaraUI_IconLibrary).GetMethod(methodName,
                        BindingFlags.Public | BindingFlags.Static);
                    _methodCache[methodName] = method;
                }

                if(_methodCache[methodName] != null) {
                    _methodCache[methodName].Invoke(null, new object[] { g, rect, color });
                }
            } catch {
                // Si un icono falla, dibujamos un pequeño cuadro de error o nada, 
                // pero evitamos que el diseñador de Visual Studio se bloquee.
                g.DrawRectangle(Pens.Red, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            }
        }

        public static List<string> GetAvailableIcons() {
            return typeof(SaraUI_IconLibrary)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Draw"))
                .Select(m => m.Name.Replace("Draw", ""))
                .ToList();
        }

        private static void DrawFilled(string iconName, Graphics g, Rectangle rect, Color color) {
            using(SolidBrush brush = new SolidBrush(color)) {
                // ejemplo base: User
                if(iconName == "User") {
                    DrawUserFilled(g, rect, color);
                    return;
                }

                // fallback
                DrawIcon(iconName, g, rect, color);
            }
        }

        private static void DrawCircle(string iconName, Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
            }

            Rectangle inner = Rectangle.Inflate(rect, -4, -4);
            DrawIcon(iconName, g, inner, color);
        }

        private static void DrawSquare(string iconName, Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
            }

            Rectangle inner = Rectangle.Inflate(rect, -4, -4);
            DrawIcon(iconName, g, inner, color);
        }

        private static void DrawRounded(string iconName, Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2))
            using(GraphicsPath path = new GraphicsPath()) {
                int r = 6;

                path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);

                path.CloseFigure();

                g.DrawPath(pen, path);
            }

            Rectangle inner = Rectangle.Inflate(rect, -4, -4);
            DrawIcon(iconName, g, inner, color);
        }

        public class IconNameConverter:StringConverter {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => false;

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // Usamos tu nuevo método dinámico
                var icons = SaraUI_IconLibrary.GetAvailableIcons();
                icons.Remove("Icon"); // Limpiamos nombres que no sean iconos puros
                icons.Sort();
                icons.Insert(0, "None");
                return new StandardValuesCollection(icons);
            }
        }


        // ==========================
        // ICONOS A
        // ==========================

        public static void DrawAdd(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;

                g.DrawLine(pen, cx, rect.Y + rect.Height * 0.2f, cx, rect.Bottom - rect.Height * 0.2f);
                g.DrawLine(pen, rect.X + rect.Width * 0.2f, cy, rect.Right - rect.Width * 0.2f, cy);
            }
        }

        public static void DrawAddCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
                DrawAdd(g, rect, color);
            }
        }

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

        public static void DrawArrowLeft(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float y = rect.Y + rect.Height / 2f;

                g.DrawLine(pen, rect.Right, y, rect.X + rect.Width * 0.3f, y);
                g.DrawLine(pen, rect.X + rect.Width * 0.3f, y, rect.X + rect.Width * 0.5f, rect.Y);
                g.DrawLine(pen, rect.X + rect.Width * 0.3f, y, rect.X + rect.Width * 0.5f, rect.Bottom);
            }
        }

        public static void DrawArrowRight2(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float y = rect.Y + rect.Height / 2f;

                g.DrawLine(pen, rect.X, y, rect.Right - rect.Width * 0.3f, y);
                g.DrawLine(pen, rect.Right - rect.Width * 0.3f, y, rect.Right - rect.Width * 0.5f, rect.Y);
                g.DrawLine(pen, rect.Right - rect.Width * 0.3f, y, rect.Right - rect.Width * 0.5f, rect.Bottom);
            }
        }

        public static void DrawArrowRight(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(pen, rect.X, rect.Y + (rect.Height / 2), rect.Right, rect.Y + (rect.Height / 2));
                g.DrawLine(pen, rect.Right - 5, rect.Y, rect.Right, rect.Y + (rect.Height / 2));
                g.DrawLine(pen, rect.Right - 5, rect.Bottom, rect.Right, rect.Y + (rect.Height / 2));
            }
        }

        public static void DrawArrowUp(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                float x = rect.X + rect.Width / 2f;

                g.DrawLine(pen, x, rect.Bottom, x, rect.Y + rect.Height * 0.3f);
                g.DrawLine(pen, x, rect.Y + rect.Height * 0.3f, rect.X, rect.Y + rect.Height * 0.5f);
                g.DrawLine(pen, x, rect.Y + rect.Height * 0.3f, rect.Right, rect.Y + rect.Height * 0.5f);
            }
        }

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

        public static void DrawAlert(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                Point p1 = new Point(rect.X + rect.Width / 2, rect.Y);
                Point p2 = new Point(rect.Right, rect.Bottom);
                Point p3 = new Point(rect.X, rect.Bottom);

                g.DrawPolygon(pen, new[] { p1, p2, p3 });

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height * 0.3f,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height * 0.6f);

                g.DrawEllipse(pen,
                    rect.X + rect.Width / 2 - 1,
                    rect.Y + rect.Height * 0.75f,
                    2, 2);
            }
        }

        // ==========================
        // ICONOS B
        // ==========================

        public static void DrawBell(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                Rectangle bell = new Rectangle(
                    rect.X + rect.Width / 4,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height / 2
                );

                g.DrawArc(pen, bell, 180, 180);

                g.DrawLine(pen,
                    rect.X + rect.Width / 4,
                    rect.Y + rect.Height / 2,
                    rect.X + rect.Width * 3 / 4,
                    rect.Y + rect.Height / 2);

                g.DrawEllipse(pen,
                    rect.X + rect.Width / 2 - 2,
                    rect.Bottom - 4,
                    4, 4);
            }
        }

        public static void DrawBookmark(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                Point topLeft = new Point(rect.X, rect.Y);
                Point topRight = new Point(rect.Right, rect.Y);
                Point bottom = new Point(rect.X + rect.Width / 2, rect.Bottom);

                g.DrawLines(pen, new[] {
            topLeft,
            topRight,
            bottom,
            topLeft
        });
            }
        }

        public static void DrawBug(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect.X + rect.Width / 4, rect.Y, rect.Width / 2, rect.Height / 2);
                g.DrawLine(pen, rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.X + rect.Width / 2, rect.Bottom);

                g.DrawLine(pen, rect.X, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2);
            }
        }

        // ==========================
        // ICONOS C
        // ==========================

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

        public static void DrawCheckCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
                DrawCheck(g, rect, color);
            }
        }

        public static void DrawCloseCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
            }
        }

        public static void DrawCloud(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect.X, rect.Y + rect.Height / 3, rect.Width / 2, rect.Height / 2);
                g.DrawEllipse(pen, rect.X + rect.Width / 3, rect.Y, rect.Width / 2, rect.Height / 2);

                g.DrawLine(pen,
                    rect.X,
                    rect.Bottom - rect.Height / 3,
                    rect.Right,
                    rect.Bottom - rect.Height / 3);
            }
        }

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

        public static void DrawDelete(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
            }
        }

        public static void DrawDeleteCircle(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawEllipse(pen, rect);
                DrawDelete(g, rect, color);
            }
        }

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

        public static void DrawEdit(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen,
                    rect.X + rect.Width * 0.2f,
                    rect.Bottom - rect.Height * 0.2f,
                    rect.Right,
                    rect.Y);

                g.DrawLine(pen,
                    rect.Right - 4,
                    rect.Y,
                    rect.Right,
                    rect.Y + 4);
            }
        }

        public static void DrawEditSquare(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
                DrawEdit(g, rect, color);
            }
        }

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

        public static void DrawEyeOff(Graphics g, Rectangle rect, Color color) {
            DrawEye(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Bottom, rect.Right, rect.Y);
            }
        }

        // ==========================
        // ICONOS F
        // ==========================

        public static void DrawFile(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
            }
        }

        public static void DrawFileText(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);

                g.DrawLine(pen, rect.X + 4, rect.Y + 6, rect.Right - 4, rect.Y + 6);
                g.DrawLine(pen, rect.X + 4, rect.Y + 10, rect.Right - 4, rect.Y + 10);
            }
        }

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

        public static void DrawGear(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;

                float r1 = rect.Width * 0.2f;
                float r2 = rect.Width * 0.45f;

                g.DrawEllipse(pen, cx - r1, cy - r1, r1 * 2, r1 * 2);

                for(int i = 0; i < 360; i += 45) {
                    double a = i * Math.PI / 180;

                    float x1 = (float)(cx + Math.Cos(a) * r1);
                    float y1 = (float)(cy + Math.Sin(a) * r1);

                    float x2 = (float)(cx + Math.Cos(a) * r2);
                    float y2 = (float)(cy + Math.Sin(a) * r2);

                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

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

        public static void DrawHeart(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                GraphicsPath path = new GraphicsPath();

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

        public static void DrawHelp(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                // ?
                g.DrawArc(pen, rect.X + rect.Width * 0.2f, rect.Y, rect.Width * 0.6f, rect.Height * 0.5f, 0, 180);

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height * 0.5f,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height * 0.7f);

                g.DrawEllipse(pen,
                    rect.X + rect.Width / 2 - 1,
                    rect.Bottom - rect.Height * 0.2f,
                    2, 2);
            }
        }

        // ==========================
        // ICONOS I
        // ==========================

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

        public static void DrawJustifyLeft(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                for(int i = 0; i < 4; i++) {
                    float y = rect.Y + (i + 1) * rect.Height / 5f;
                    g.DrawLine(pen, rect.X, y, rect.Right - rect.Width * (i % 2 == 0 ? 0 : 0.3f), y);
                }
            }
        }

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

        public static void DrawKey(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int r = rect.Width / 4;

                g.DrawEllipse(pen, rect.X, rect.Y + rect.Height / 4, r, r);

                g.DrawLine(pen,
                    rect.X + r,
                    rect.Y + rect.Height / 2,
                    rect.Right,
                    rect.Y + rect.Height / 2);

                g.DrawLine(pen,
                    rect.Right - rect.Width * 0.2f,
                    rect.Y + rect.Height / 2,
                    rect.Right - rect.Width * 0.2f,
                    rect.Bottom);
            }
        }

        // ==========================
        // ICONOS L
        // ==========================

        public static void DrawLock(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                // cuerpo
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y + rect.Height / 3,
                    rect.Width,
                    rect.Height * 2 / 3);

                // arco
                g.DrawArc(pen,
                    rect.X + rect.Width / 4,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height / 2,
                    0, 180);
            }
        }

        public static void DrawUnlock(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                // cuerpo
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y + rect.Height / 3,
                    rect.Width,
                    rect.Height * 2 / 3);

                // arco abierto
                g.DrawArc(pen,
                    rect.X + rect.Width / 4,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height / 2,
                    45, 270);
            }
        }

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

        public static void DrawMenu(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                for(int i = 0; i < 3; i++) {
                    float y = rect.Y + (i + 1) * rect.Height / 4f;
                    g.DrawLine(pen, rect.X, y, rect.Right, y);
                }
            }
        }

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

        public static void DrawMap(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int w = rect.Width / 3;

                g.DrawLine(pen, rect.X + w, rect.Y, rect.X + w, rect.Bottom);
                g.DrawLine(pen, rect.X + w * 2, rect.Y, rect.X + w * 2, rect.Bottom);
            }
        }

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

        public static void DrawPlay(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Point p1 = new Point(rect.X, rect.Y);
                Point p2 = new Point(rect.Right, rect.Y + rect.Height / 2);
                Point p3 = new Point(rect.X, rect.Bottom);

                g.DrawPolygon(pen, new[] { p1, p2, p3 });
            }
        }

        public static void DrawPause(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int w = rect.Width / 3;

                g.DrawRectangle(pen, rect.X, rect.Y, w, rect.Height);
                g.DrawRectangle(pen, rect.Right - w, rect.Y, w, rect.Height);
            }
        }

        public static void DrawStop(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
            }
        }

        // ==========================
        // ICONOS PHONE
        // ==========================

        public static void DrawPhone(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawArc(pen, rect, 45, 90);
                g.DrawArc(pen, rect, 225, 90);
            }
        }

        // ==========================
        // ICONOS PLUS
        // ==========================

        public static void DrawPlus(Graphics g, Rectangle rect, Color color) {
            DrawAdd(g, rect, color);
        }

        public static void DrawPlusCircle(Graphics g, Rectangle rect, Color color) {
            DrawAddCircle(g, rect, color);
        }

        public static void DrawPlusSquare(Graphics g, Rectangle rect, Color color) {
            DrawAddSquare(g, rect, color);
        }


        // ==========================
        // ICONOS Q
        // ==========================

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

        public static void DrawRefresh(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawArc(pen, rect, 30, 300);

                g.DrawLine(pen,
                    rect.Right - rect.Width * 0.2f,
                    rect.Y + rect.Height * 0.2f,
                    rect.Right,
                    rect.Y);

                g.DrawLine(pen,
                    rect.Right - rect.Width * 0.2f,
                    rect.Y + rect.Height * 0.2f,
                    rect.Right,
                    rect.Y + rect.Height * 0.3f);
            }
        }

        public static void DrawReload(Graphics g, Rectangle rect, Color color) {
            DrawRefresh(g, rect, color);
        }

        // ==========================
        // ICONOS S
        // ==========================

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

        public static void DrawSearchMinus(Graphics g, Rectangle rect, Color color) {
            DrawSearch(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                int size = rect.Width / 5;

                int x = rect.Right - size;
                int y = rect.Y + size;

                g.DrawLine(pen, x, y, x + size, y);
            }
        }

        public static void DrawShield(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                Point top = new Point(rect.X + rect.Width / 2, rect.Y);
                Point left = new Point(rect.X, rect.Y + rect.Height / 3);
                Point right = new Point(rect.Right, rect.Y + rect.Height / 3);
                Point bottom = new Point(rect.X + rect.Width / 2, rect.Bottom);

                g.DrawPolygon(pen, new[] { top, right, bottom, left });
            }
        }

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

        public static void DrawTrashAlt(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen,
                    rect.X + rect.Width * 0.2f,
                    rect.Y + rect.Height * 0.2f,
                    rect.Width * 0.6f,
                    rect.Height * 0.7f);

                g.DrawLine(pen,
                    rect.X,
                    rect.Y + rect.Height * 0.2f,
                    rect.Right,
                    rect.Y + rect.Height * 0.2f);
            }
        }

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

        public static void DrawToggleOn(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);

                g.DrawEllipse(pen,
                    rect.Right - rect.Width / 2,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height);
            }
        }

        public static void DrawToggleOff(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);

                g.DrawEllipse(pen,
                    rect.X,
                    rect.Y,
                    rect.Width / 2,
                    rect.Height);
            }
        }

        // ==========================
        // ICONOS U
        // ==========================

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

        public static void DrawVideo(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y,
                    rect.Width * 0.7f,
                    rect.Height);

                Point p1 = new Point((int)(rect.Right - rect.Width * 0.3f), rect.Y);
                Point p2 = new Point(rect.Right, rect.Y + rect.Height / 2);
                Point p3 = new Point((int)(rect.Right - rect.Width * 0.3f), rect.Bottom);

                g.DrawPolygon(pen, new[] { p1, p2, p3 });
            }
        }

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

        public static void DrawVolumeOff(Graphics g, Rectangle rect, Color color) {
            DrawVolume(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
            }
        }

        // ==========================
        // ICONOS W
        // ==========================

        public static void DrawWarning(Graphics g, Rectangle rect, Color color) {
            DrawAlert(g, rect, color);
        }

        public static void DrawWifi(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using(Pen pen = new Pen(color, 2)) {
                // Usamos porcentajes para que nunca sea negativo, sin importar el tamaño
                for(int i = 0; i < 3; i++) {
                    float gap = rect.Width * 0.2f * i;
                    RectangleF arcRect = new RectangleF(
                        rect.X + gap,
                        rect.Y + gap,
                        Math.Max(1, rect.Width - (gap * 2)),
                        Math.Max(1, rect.Height - (gap * 2))
                    );
                    g.DrawArc(pen, arcRect, 225, 90);
                }

                // El punto base
                float dotSize = rect.Width * 0.2f;
                g.FillEllipse(new SolidBrush(color),
                    rect.X + (rect.Width - dotSize) / 2,
                    rect.Bottom - dotSize,
                    dotSize, dotSize);
            }
        }

        // ==========================
        // ICONOS X
        // ==========================

        public static void DrawX(Graphics g, Rectangle rect, Color color) {
            DrawDelete(g, rect, color);
        }

        // ==========================
        // ICONOS Y
        // ==========================

        public static void DrawYen(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                g.DrawLine(pen,
                    rect.X + rect.Width / 4,
                    rect.Y + rect.Height * 0.6f,
                    rect.Right - rect.Width / 4,
                    rect.Y + rect.Height * 0.6f);
            }
        }

        // ==========================
        // ICONOS Z
        // ==========================

        public static void DrawZoomIn(Graphics g, Rectangle rect, Color color) {
            DrawSearch(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                float cx = rect.X + rect.Width / 2;
                float cy = rect.Y + rect.Height / 2;

                g.DrawLine(pen, cx - 3, cy, cx + 3, cy);
                g.DrawLine(pen, cx, cy - 3, cx, cy + 3);
            }
        }

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

        public static void DrawClose(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2.5f)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
            }
        }

       

        public static void DrawBulb(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = rect.Width;
            float h = rect.Height;

            using(Pen pen = new Pen(color, 2))
            using(GraphicsPath path = new GraphicsPath()) {
                // --- Parte superior (bombilla redondeada) ---
                RectangleF bulbRect = new RectangleF(
                    rect.X + w * 0.2f,
                    rect.Y,
                    w * 0.6f,
                    h * 0.6f
                );

                path.AddArc(bulbRect, 200, 140);
                path.AddArc(bulbRect, 20, 140);

                // --- Cuello ---
                float neckWidth = w * 0.25f;
                float neckX = rect.X + (w - neckWidth) / 2;
                float neckTop = rect.Y + h * 0.55f;

                path.AddLine(
                    rect.X + w * 0.35f, neckTop,
                    neckX, neckTop
                );

                path.AddLine(
                    neckX + neckWidth, neckTop,
                    rect.X + w * 0.65f, neckTop
                );

                g.DrawPath(pen, path);

                // --- Base (rosca) ---
                float baseTop = rect.Y + h * 0.7f;

                g.DrawLine(pen,
                    rect.X + w * 0.35f, baseTop,
                    rect.X + w * 0.65f, baseTop);

                g.DrawLine(pen,
                    rect.X + w * 0.37f, baseTop + 3,
                    rect.X + w * 0.63f, baseTop + 3);

                g.DrawLine(pen,
                    rect.X + w * 0.4f, baseTop + 6,
                    rect.X + w * 0.6f, baseTop + 6);
            }
        }

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

        public static void DrawSettings(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawEllipse(pen, rect.X + rect.Width / 4, rect.Y + rect.Height / 4, rect.Width / 2, rect.Height / 2);
                // Dientes del engranaje (simplificado para que se vea limpio)
                for(int i = 0; i < 360; i += 45) {
                    double angle = i * Math.PI / 180;
                    float x1 = (float)(rect.X + rect.Width / 2 + Math.Cos(angle) * (rect.Width / 4));
                    float y1 = (float)(rect.Y + rect.Height / 2 + Math.Sin(angle) * (rect.Height / 4));
                    float x2 = (float)(rect.X + rect.Width / 2 + Math.Cos(angle) * (rect.Width / 2));
                    float y2 = (float)(rect.Y + rect.Height / 2 + Math.Sin(angle) * (rect.Height / 2));
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        public static void DrawCalendar(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(pen, rect.X, rect.Y + 3, rect.Width, rect.Height - 3);
                g.DrawLine(pen, rect.X + (rect.Width / 4), rect.Y, rect.X + (rect.Width / 4), rect.Y + 5);
                g.DrawLine(pen, rect.X + (rect.Width * 3 / 4), rect.Y, rect.X + (rect.Width * 3 / 4), rect.Y + 5);
                g.DrawLine(pen, rect.X, rect.Y + 8, rect.X + rect.Width, rect.Y + 8);
            }
        }

        public static void DrawUser(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int headSize = rect.Width / 2;
                g.DrawEllipse(pen, rect.X + (rect.Width - headSize) / 2, rect.Y, headSize, headSize);
                g.DrawArc(pen, rect.X, rect.Y + (rect.Height / 2), rect.Width, rect.Height, 180, 180);
            }
        }

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

        public static void DrawUsers(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                int head = rect.Width / 4;

                // Usuario frontal
                g.DrawEllipse(pen,
                    rect.X + head,
                    rect.Y,
                    head,
                    head);

                g.DrawArc(pen,
                    rect.X + head / 2,
                    rect.Y + head,
                    rect.Width - head,
                    rect.Height,
                    180, 180);

                // Usuario atrás
                g.DrawEllipse(pen,
                    rect.X,
                    rect.Y + head / 2,
                    head,
                    head);
            }
        }

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

        public static void DrawSearch(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int circleSize = (int)(rect.Width * 0.7);
                g.DrawEllipse(pen, rect.X, rect.Y, circleSize, circleSize);
                g.DrawLine(pen, rect.X + (int)(circleSize * 0.8), rect.Y + (int)(circleSize * 0.8), rect.Right, rect.Bottom);
            }
        }

        public static void DrawTrash(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(pen, rect.X + 2, rect.Y + 4, rect.Width - 4, rect.Height - 4);
                g.DrawLine(pen, rect.X, rect.Y + 2, rect.Right, rect.Y + 2);
                g.DrawLine(pen, rect.X + (rect.Width / 2) - 2, rect.Y, rect.X + (rect.Width / 2) + 2, rect.Y);
            }
        }

        public static void DrawClear(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
            }
        }

        public static void DrawStats(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = rect.Width / 4;
                g.DrawLine(pen, rect.X + w, rect.Bottom, rect.X + w, rect.Y + (rect.Height / 2));
                g.DrawLine(pen, rect.X + w * 2, rect.Bottom, rect.X + w * 2, rect.Y);
                g.DrawLine(pen, rect.X + w * 3, rect.Bottom, rect.X + w * 3, rect.Y + (rect.Height / 3));
            }
        }

        

        public static void DrawLogout(Graphics g, Rectangle rect, Color color) {
            using(Pen pen = new Pen(color, 2)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // El "rectángulo" abierto (puerta)
                g.DrawLines(pen, new Point[] {
            new Point(rect.X + rect.Width/3, rect.Y),
            new Point(rect.X, rect.Y),
            new Point(rect.X, rect.Bottom),
            new Point(rect.X + rect.Width/3, rect.Bottom)
        });
                // La flecha
                int arrowY = rect.Y + rect.Height / 2;
                g.DrawLine(pen, rect.X + 4, arrowY, rect.Right, arrowY);
                g.DrawLine(pen, rect.Right - 5, arrowY - 5, rect.Right, arrowY);
                g.DrawLine(pen, rect.Right - 5, arrowY + 5, rect.Right, arrowY);
            }
        }

        public static void DrawLogin(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                // puerta
                g.DrawRectangle(pen,
                    rect.X,
                    rect.Y,
                    rect.Width * 0.6f,
                    rect.Height);

                // flecha entrando
                float y = rect.Y + rect.Height / 2;

                g.DrawLine(pen,
                    rect.Right,
                    y,
                    rect.X + rect.Width * 0.4f,
                    y);

                g.DrawLine(pen,
                    rect.Right - 5,
                    y - 5,
                    rect.Right,
                    y);

                g.DrawLine(pen,
                    rect.Right - 5,
                    y + 5,
                    rect.Right,
                    y);
            }
        }

        public static void DrawLockKey(Graphics g, Rectangle rect, Color color) {
            DrawLock(g, rect, color);

            using(Pen pen = new Pen(color, 2)) {
                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2,
                    rect.X + rect.Width * 0.8f,
                    rect.Y + rect.Height / 2);
            }
        }

        public static void DrawNotification(Graphics g, Rectangle rect, Color color) {
            DrawBell(g, rect, color);

            using(SolidBrush brush = new SolidBrush(color)) {
                g.FillEllipse(brush,
                    rect.Right - 6,
                    rect.Y,
                    6, 6);
            }
        }

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

        public static void DrawCheckSquare(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
                DrawCheck(g, rect, color);
            }
        }

        public static void DrawCloseSquare(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawRectangle(pen, rect);
                DrawDelete(g, rect, color);
            }
        }

        public static void DrawPower(Graphics g, Rectangle rect, Color color) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(color, 2)) {
                g.DrawArc(pen, rect, 45, 270);

                g.DrawLine(pen,
                    rect.X + rect.Width / 2,
                    rect.Y,
                    rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2);
            }
        }

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

        public static void DrawSync(Graphics g, Rectangle rect, Color color) {
            DrawRefresh(g, rect, color);
        }

        public static void DrawPin(Graphics g, Rectangle rect, Color color) {
            DrawMapPin(g, rect, color);
        }



    }
}