using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;

namespace Schulkueche.Data;

public static class DataSetup
{
    /// <summary>
    /// Adds the KitchenDbContext and repositories. The database file is created at provided path.
    /// </summary>
    public static IServiceCollection AddKitchenData(
        this IServiceCollection services,
        string sqliteDbPath,
        Action<PricingOptions>? pricing = null)
    {
        services.Configure(pricing ?? (_ => { }));

        services.AddDbContext<KitchenDbContext>(opt =>
        {
            opt.UseSqlite($"Data Source={sqliteDbPath}");
        });

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IBillingService, BillingService>();

        // QuestPDF license (community)
        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }
}

/// <summary>
/// Design-time factory for EF Core tools.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KitchenDbContext>
{
    public KitchenDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KitchenDbContext>();
        optionsBuilder.UseSqlite("Data Source=design-time.db");
        return new KitchenDbContext(optionsBuilder.Options);
    }
}
