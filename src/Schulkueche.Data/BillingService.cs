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
    int EtagentraegerMenge,
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
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity)); // Nur Menge, kein Preis

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
                var etagentraegerMenge = chargesByPerson.GetValueOrDefault(p.Id, 0);
                var total = unit * qty + deliverySum; // Etagenträger werden NICHT verrechnet

                string address = string.Join("\n", new[]
                {
                    string.Join(' ', new[]{ p.Street, p.HouseNumber }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    string.Join(' ', new[]{ p.Zip, p.City }.Where(s => !string.IsNullOrWhiteSpace(s)))
                }.Where(s => !string.IsNullOrWhiteSpace(s)));

                return new BillingRow(p.Name, address, p.Category, unit, qty, deliveries, PricingDefaults.DeliverySurcharge, etagentraegerMenge, total);
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
                page.Margin(25);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().PaddingBottom(20).Column(header =>
                {
                    header.Item().AlignCenter().Text($"Gemeinde-Küche Munderfing").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    header.Item().PaddingTop(5).AlignCenter().Text($"Sammelabrechnung {monthName}").FontSize(14).FontColor(Colors.Grey.Darken1);
                    header.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    void Section(string title, IEnumerable<BillingRow> items)
                    {
                        var list = items.ToList();
                        if (!list.Any()) return;

                        col.Item().PaddingTop(20).PaddingBottom(8).Text(title).SemiBold().FontSize(12).FontColor(Colors.Blue.Darken1);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(4); // Name
                                cols.RelativeColumn(4.5f); // Anschrift (kleiner)
                                cols.RelativeColumn(1.5f); // Anzahl (größer)
                                cols.RelativeColumn(1.8f); // Essen Summe
                                cols.RelativeColumn(1.2f); // Lieferungen
                                cols.RelativeColumn(1.5f); // Lieferung Summe (kleiner)
                                cols.RelativeColumn(1.2f); // Etagenträger Menge
                                cols.RelativeColumn(1.8f); // Total
                            });

                            // Header
                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Name").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Anschrift").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).AlignCenter().Text("Anzahl").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).AlignCenter().Text("Essen €").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).AlignCenter().Text("Lief.").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).AlignCenter().Text("Lief. €").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).AlignCenter().Text("Etag.").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Blue.Lighten4).Padding(6).AlignCenter().Text("Total").SemiBold().FontSize(9);
                            });

                            var rowIndex = 0;
                            foreach (var r in list)
                            {
                                var bg = (rowIndex % 2 == 0) ? Colors.White : Colors.Grey.Lighten5;
                                var borderColor = Colors.Grey.Lighten2;
                                
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).Text(r.Name).FontSize(8);
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).Text(r.Address ?? string.Empty).FontSize(8);
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).AlignCenter().Text(r.Quantity > 0 ? r.Quantity.ToString() : "-").FontSize(8);
                                
                                var mealSum = r.UnitPrice * r.Quantity;
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).AlignCenter().Text(mealSum > 0 ? mealSum.ToString("C") : "-").FontSize(8);
                                
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).AlignCenter().Text(r.DeliveryCount > 0 ? r.DeliveryCount.ToString() : "-").FontSize(8);
                                
                                var deliverySum = r.DeliveryCount * r.DeliverySurcharge;
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).AlignCenter().Text(deliverySum > 0 ? deliverySum.ToString("C") : "-").FontSize(8);
                                
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).AlignCenter().Text(r.EtagentraegerMenge > 0 ? r.EtagentraegerMenge.ToString() : "-").FontSize(8);
                                
                                table.Cell().Background(bg).Padding(4).BorderBottom(1).BorderColor(borderColor).AlignCenter().Text(r.Total.ToString("C")).SemiBold().FontSize(8);
                                
                                rowIndex++;
                            }

                            // No section totals per request
                        });
                    }

                    Section("Pensionisten", rows.Where(r => r.Category == PersonCategory.Pensioner));
                    Section("Kindergruppe", rows.Where(r => r.Category == PersonCategory.ChildGroup));
                    Section("Gratis", rows.Where(r => r.Category == PersonCategory.FreeMeal));

                    // No overall totals per request
                });

                page.Footer().PaddingTop(20).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(8).AlignLeft().Text(t =>
                        {
                            t.Span("Erstellt mit Schulkueche Monatsabrechnung v1.3.0.0").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    r.RelativeItem().Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(8).AlignRight().Text(txt =>
                        {
                            txt.Span("Seite ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            txt.CurrentPageNumber().FontColor(Colors.Grey.Darken1);
                            txt.Span(" von ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            txt.TotalPages().FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        });

        doc.GeneratePdf(outputPath);
        return outputPath;
    }
}
