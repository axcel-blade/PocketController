namespace PocketConsoleServer
{
    internal static class Program
    {
        /// <summary>Application entry point.</summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
