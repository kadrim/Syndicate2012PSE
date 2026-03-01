using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Avalonia;

namespace ME3Server_WV
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            string[] commandlineargs = System.Environment.GetCommandLineArgs();
            ME3Server.isMITM = commandlineargs.Contains("-mitm", StringComparer.InvariantCultureIgnoreCase);
            ME3Server.silentStart = commandlineargs.Contains("-silentstart", StringComparer.InvariantCultureIgnoreCase);
            ME3Server.silentExit = commandlineargs.Contains("-silentexit", StringComparer.InvariantCultureIgnoreCase);

            if (commandlineargs.Contains("-deactivateonly", StringComparer.InvariantCultureIgnoreCase))
            {
                Frontend.DeactivateRedirection();
            }
            else
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
