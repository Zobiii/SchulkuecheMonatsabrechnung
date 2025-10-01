using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schulkueche.Data;
using Schulkueche.App.ViewModels;
using System;
using System.IO;

namespace Schulkueche.App.Infrastructure;

/// <summary>
/// Configures the application's dependency injection and hosting.
/// </summary>
public static class Bootstrapper
{
    public static IHost BuildHost()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDir = Path.Combine(appData, "Schulkueche");
        Directory.CreateDirectory(appDir);
        var dbPath = Path.Combine(appDir, "kitchen.db");

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(b =>
            {
                b.AddDebug();
                b.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices(s =>
            {
                s.AddKitchenData(dbPath);
                s.AddUi();
            })
            .Build();

        return host;
    }
}
