using Microsoft.Extensions.DependencyInjection;
using Schulkueche.App.ViewModels;

namespace Schulkueche.App.Infrastructure;

public static class UiSetup
{
    public static IServiceCollection AddUi(this IServiceCollection services)
    {
        services.AddTransient<PersonenViewModel>();
        services.AddTransient<ErfassungViewModel>();
        services.AddTransient<AbrechnungViewModel>();
        services.AddTransient<EtagentraegerViewModel>();
        services.AddTransient<MainWindowViewModel>();
        return services;
    }
}
