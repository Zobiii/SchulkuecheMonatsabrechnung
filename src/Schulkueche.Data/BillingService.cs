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
    decimal AdditionalCharges,
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
            
        // Get additional charges for the month
        var additionalCharges = await db.AdditionalCharges.Include(c => c.Person)
            .Where(c => c.Month == first)
            .ToListAsync(ct);
        var chargesByPerson = additionalCharges.GroupBy(c => c.PersonId)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.UnitPrice * c.Quantity));

        var rows = orders
            .GroupBy(o => o.PersonId)
            .Select(g =>
            {
                var firstOrder = g.First();
                var p = firstOrder.Person;
                if (p is null)
                {
                    throw new InvalidOperationException($"Person data missing for PersonId {firstOrder.PersonId}. This indicates a data integrity issue.");
                }
                
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
                var additionalChargesSum = chargesByPerson.GetValueOrDefault(p.Id, 0m);
                var total = unit * qty + deliverySum + additionalChargesSum;

                string address = string.Join("\n", new[]
                {
                    string.Join(' ', new[]{ p.Street, p.HouseNumber }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    string.Join(' ', new[]{ p.Zip, p.City }.Where(s => !string.IsNullOrWhiteSpace(s)))
                }.Where(s => !string.IsNullOrWhiteSpace(s)));

                return new BillingRow(p.Name, address, p.Category, unit, qty, deliveries, PricingDefaults.DeliverySurcharge, additionalChargesSum, total);
            })
            .OrderBy(r => r.Name)
            .ToList();

        return rows;
    }

    public async Task<string> ExportMonthlyPdfAsync(int year, int month, string outputPath, CancellationToken ct = default)
    {
        var rows = await CalculateMonthlyAsync(year, month, ct);
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

        // Totals removed per request

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(header =>
                {
                    header.Item().Text($"Gemeinde-Küche").SemiBold().FontSize(14);
                    header.Item().Text($"Sammelabrechnung {monthName}");
                });

                page.Content().Column(col =>
                {
                    void Section(string title, IEnumerable<BillingRow> items)
                    {
                        var list = items.ToList();
                        if (!list.Any()) return;

                        col.Item().PaddingTop(8).Text(title).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3); // Name
                                cols.RelativeColumn(4); // Adresse
                                cols.RelativeColumn(2); // Einheitspreis
                                cols.RelativeColumn(1); // Menge
                                cols.RelativeColumn(2); // Essen Summe
                                cols.RelativeColumn(1); // Lieferungen
                                cols.RelativeColumn(2); // Lieferung Summe
                                cols.RelativeColumn(2); // Zusatzkosten
                                cols.RelativeColumn(2); // Gesamt
                            });

                            // Header
                            table.Header(h =>
                            {
                                h.Cell().Text("Name").SemiBold();
                                h.Cell().Text("Anschrift").SemiBold();
                                h.Cell().AlignRight().Text("Einzelpreis").SemiBold();
                                h.Cell().AlignRight().Text("Menge").SemiBold();
                                h.Cell().AlignRight().Text("Essen").SemiBold();
                                h.Cell().AlignRight().Text("Liefer.").SemiBold();
                                h.Cell().AlignRight().Text("Lieferung").SemiBold();
                                h.Cell().AlignRight().Text("Zusatzk.").SemiBold();
                                h.Cell().AlignRight().Text("Summe").SemiBold();
                            });

                            var rowIndex = 0;
                            foreach (var r in list)
                            {
                                var bg = (rowIndex++ % 2 == 0) ? Colors.Grey.Lighten4 : Colors.White;
                                table.Cell().Background(bg).Text(r.Name);
                                table.Cell().Background(bg).Text(r.Address ?? string.Empty);
                                table.Cell().Background(bg).AlignRight().Text(r.UnitPrice.ToString("C"));
                                table.Cell().Background(bg).AlignRight().Text(r.Quantity.ToString());
                                var mealSum = r.UnitPrice * r.Quantity;
                                var deliverySum = r.DeliveryCount * r.DeliverySurcharge;
                                table.Cell().Background(bg).AlignRight().Text(mealSum.ToString("C"));
                                table.Cell().Background(bg).AlignRight().Text(r.DeliveryCount > 0 ? r.DeliveryCount.ToString() : "");
                                table.Cell().Background(bg).AlignRight().Text(deliverySum > 0 ? deliverySum.ToString("C") : "");
                                table.Cell().Background(bg).AlignRight().Text(r.AdditionalCharges > 0 ? r.AdditionalCharges.ToString("C") : "");
                                table.Cell().Background(bg).AlignRight().Text(r.Total.ToString("C"));
                            }

                            // No section totals per request
                        });
                    }

                    Section("Pensionisten", rows.Where(r => r.Category == PersonCategory.Pensioner));
                    Section("Kindergruppe", rows.Where(r => r.Category == PersonCategory.ChildGroup));
                    Section("Gratis", rows.Where(r => r.Category == PersonCategory.FreeMeal));

                    // No overall totals per request
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().AlignLeft().Text(t =>
                    {
                        t.Span("Auto-created by Schulkueche Monatsabrechnung").FontSize(9);
                    });

                    r.RelativeItem().AlignRight().Text(txt =>
                    {
                        txt.Span("Seite ").FontSize(9);
                        txt.CurrentPageNumber();
                        txt.Span(" / ").FontSize(9);
                        txt.TotalPages();
                    });
                });
            });
        });

        doc.GeneratePdf(outputPath);
        return outputPath;
    }
}
