using Microsoft.EntityFrameworkCore;
using Schulkueche.Core;

namespace Schulkueche.Data;

public record BillingRow(
    string Name,
    string Address,
    decimal UnitPrice,
    int Quantity,
    int DeliveryCount,
    decimal DeliverySurcharge,
    decimal Total);

public interface IBillingService
{
    Task<IReadOnlyList<BillingRow>> CalculateMonthlyAsync(int year, int month, CancellationToken ct = default);
}

internal sealed class BillingService(KitchenDbContext db) : IBillingService
{
    public async Task<IReadOnlyList<BillingRow>> CalculateMonthlyAsync(int year, int month, CancellationToken ct = default)
    {
        var first = new DateOnly(year, month, 1);
        var next = first.AddMonths(1);

        var orders = await db.MealOrders.Include(o => o.Person)
            .Where(o => o.Date >= first && o.Date < next)
            .ToListAsync(ct);

        var rows = orders
            .GroupBy(o => o.PersonId)
            .Select(g =>
            {
                var p = g.First().Person!;
                var unit = p.CustomMealPrice ?? p.Category switch
                {
                    PersonCategory.Pensioner => PricingDefaults.PensionerMealPrice,
                    PersonCategory.ChildGroup => PricingDefaults.ChildMealPrice,
                    PersonCategory.FreeMeal => 0m,
                    _ => 0m
                };
                var qty = g.Sum(x => x.Quantity);
                var deliveries = g.Count(x => x.Delivery);
                var deliverySum = deliveries * PricingDefaults.DeliverySurcharge;
                var total = unit * qty + deliverySum;

                string address = string.Join("\n", new[]
                {
                    string.Join(' ', new[]{ p.Street, p.HouseNumber }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    string.Join(' ', new[]{ p.Zip, p.City }.Where(s => !string.IsNullOrWhiteSpace(s)))
                }.Where(s => !string.IsNullOrWhiteSpace(s)));

                return new BillingRow(p.Name, address, unit, qty, deliveries, PricingDefaults.DeliverySurcharge, total);
            })
            .OrderBy(r => r.Name)
            .ToList();

        return rows;
    }
}
