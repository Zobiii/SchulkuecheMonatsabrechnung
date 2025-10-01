using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Schulkueche.Core;

namespace Schulkueche.Data;

public record BillingRow(
    string Name,
    string Address,
    PersonCategory Category,
    decimal UnitPrice,
    int Quantity,
    int DeliveryCount,
    decimal DeliverySurcharge,
    decimal Total);

public interface IBillingService
{
    Task<IReadOnlyList<BillingRow>> CalculateMonthlyAsync(int year, int month, CancellationToken ct = default);
    Task<string> ExportMonthlyPdfAsync(int year, int month, string outputPath, CancellationToken ct = default);
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

                return new BillingRow(p.Name, address, p.Category, unit, qty, deliveries, PricingDefaults.DeliverySurcharge, total);
            })
            .OrderBy(r => r.Name)
            .ToList();

        return rows;
    }

    public async Task<string> ExportMonthlyPdfAsync(int year, int month, string outputPath, CancellationToken ct = default)
    {
        var rows = await CalculateMonthlyAsync(year, month, ct);
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

        var total = rows.Sum(r => r.Total);
        var totalPensioners = rows.Where(r => r.Category == PersonCategory.Pensioner).Sum(r => r.Total);
        var totalChildren = rows.Where(r => r.Category == PersonCategory.ChildGroup).Sum(r => r.Total);
        var totalFree = rows.Where(r => r.Category == PersonCategory.FreeMeal).Sum(r => r.Total);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text($"Gemeinde-Küche – Sammelabrechnung {monthName}").SemiBold().FontSize(16);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3); // Name
                        cols.RelativeColumn(5); // Adresse
                        cols.RelativeColumn(2); // Essenspreis/Menge
                        cols.RelativeColumn(3); // Lieferung
                        cols.RelativeColumn(2); // Summe
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Name").SemiBold();
                        h.Cell().Text("Anschrift").SemiBold();
                        h.Cell().Text("Essenspreis / Menge").SemiBold();
                        h.Cell().Text("Lieferung").SemiBold();
                        h.Cell().Text("Summe").SemiBold();
                    });

                    foreach (var r in rows)
                    {
                        var cat = r.Category switch
                        {
                            PersonCategory.Pensioner => " (Pensionist)",
                            PersonCategory.ChildGroup => " (Kinder)",
                            PersonCategory.FreeMeal => " (Gratis)",
                            _ => string.Empty
                        };

                        table.Cell().Text(r.Name + cat);
                        table.Cell().Text(r.Address ?? string.Empty);
                        table.Cell().Text($"{r.UnitPrice:C} x {r.Quantity}");
                        table.Cell().Text(r.DeliveryCount > 0 ? $"{r.DeliveryCount} x {r.DeliverySurcharge:C}" : "");
                        table.Cell().Text(r.Total.ToString("C"));
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Spacing(5);
                    col.Item().Text($"Gesamt Pensionisten: {totalPensioners:C}");
                    col.Item().Text($"Gesamt Kinder: {totalChildren:C}");
                    col.Item().Text($"Gesamt Gratis: {totalFree:C}");
                    col.Item().Text($"Gesamtsumme: {total:C}").SemiBold();
                });
            });
        });

        doc.GeneratePdf(outputPath);
        return outputPath;
    }
}
