using System;
using Avalonia;

namespace PacketViewer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure(() => new App())
                .UsePlatformDetect()
                .LogToTrace();
    }
}
