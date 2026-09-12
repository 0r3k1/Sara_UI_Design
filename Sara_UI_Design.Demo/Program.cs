namespace Sara_UI_Design.Demo {
    internal static class Program {
        /// <summary>
        /// Punto de entrada de la aplicación de demostración.
        /// </summary>
        [STAThread]
        static void Main() {
#if NETFRAMEWORK
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#else
            ApplicationConfiguration.Initialize();
#endif
            Application.Run(new MainForm());
        }
    }
}
