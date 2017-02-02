using System;

namespace YolkaBot.Client.Desktop
{
    /// <summary>
    ///     The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            //var defaultHost = "your.host:your_port";
            var defaultHost = "127.0.0.1:42038";
            var arg = args.Length != 0 ? args[0] : defaultHost;
            var parts = arg.Trim().Split(':');
            var host = parts[0];
            var port = int.Parse(parts[1]);
            using (var game = new ClientGame(host, port))
            {
                game.Run();
            }
        }
    }
}