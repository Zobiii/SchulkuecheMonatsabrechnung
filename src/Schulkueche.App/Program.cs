using Avalonia;
using System;
using Microsoft.Extensions.Hosting;
using Schulkueche.App.Infrastructure;

namespace Schulkueche.App;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using var host = Bootstrapper.BuildHost();
        BuildAvaloniaApp(host)
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IHost host)
        => AppBuilder.Configure(() => new App(host))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
