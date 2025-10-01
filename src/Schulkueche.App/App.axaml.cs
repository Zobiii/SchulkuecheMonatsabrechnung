using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Schulkueche.App.ViewModels;
using Schulkueche.App.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Schulkueche.Data;
using Schulkueche.App.Infrastructure;

namespace Schulkueche.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App(IHost host)
    {
        _host = host;
    }

    // Parameterless constructor required by Avalonia XAML runtime loader (fixes AVLN3001 warning)
    // Not used in normal program flow where Program supplies the IHost.
    // Note: This creates a temporary host that will be disposed when the app shuts down.
    public App() : this(Bootstrapper.BuildHost())
    {
    }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            // Ensure database is created/updated
            using (var scope = _host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<KitchenDbContext>();
                DbInitializer.EnsureDatabaseUpdatedAsync(db).GetAwaiter().GetResult();
            }

            var vm = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_host.Services);
            desktop.MainWindow = new MainWindow { DataContext = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}