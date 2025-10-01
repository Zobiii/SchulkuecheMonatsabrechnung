using Microsoft.EntityFrameworkCore;
using Schulkueche.Core;

namespace Schulkueche.Data;

/// <summary>
/// Options for pricing to allow future configuration.
/// </summary>
public class PricingOptions
{
    public decimal PensionerMealPrice { get; set; } = PricingDefaults.PensionerMealPrice;
    public decimal DeliverySurcharge { get; set; } = PricingDefaults.DeliverySurcharge;
    public decimal ChildMealPrice { get; set; } = PricingDefaults.ChildMealPrice;
}

/// <summary>
/// EF Core DbContext for the application.
/// </summary>
public class KitchenDbContext : DbContext
{
    public KitchenDbContext(DbContextOptions<KitchenDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<MealOrder> MealOrders => Set<MealOrder>();
    public DbSet<AdditionalCharge> AdditionalCharges => Set<AdditionalCharge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(b =>
        {
            b.Property(p => p.Name).IsRequired();
            b.HasIndex(p => new { p.Name, p.Category }).IsUnique(false);
        });

        modelBuilder.Entity<MealOrder>(b =>
        {
            b.HasIndex(x => new { x.Date, x.PersonId }).IsUnique();
            b.Property(x => x.Quantity).HasDefaultValue(0);
            b.HasOne(x => x.Person)!.WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Date).HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));
        });

        modelBuilder.Entity<AdditionalCharge>(b =>
        {
            b.HasOne(x => x.Person)!.WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Month).HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));
        });
    }
}
