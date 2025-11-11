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
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(b =>
        {
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Street).HasMaxLength(120);
            b.Property(p => p.HouseNumber).HasMaxLength(20);
            b.Property(p => p.Zip).HasMaxLength(10);
            b.Property(p => p.City).HasMaxLength(120);
            b.Property(p => p.Contact).HasMaxLength(120);
            b.HasIndex(p => new { p.Name, p.Category }).IsUnique(false);
            b.HasIndex(p => p.Name); // Performance index for search
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
            b.HasIndex(x => new { x.PersonId, x.Month }); // Performance index for queries
            b.HasIndex(x => x.Month); // Performance index for monthly queries
        });

        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.Username).IsRequired().HasMaxLength(50);
            b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            b.Property(u => u.Email).IsRequired().HasMaxLength(255);
            b.HasIndex(u => u.Username).IsUnique();
            b.HasIndex(u => u.Email).IsUnique();
        });
    }
}
