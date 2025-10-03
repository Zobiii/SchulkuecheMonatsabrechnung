using Microsoft.EntityFrameworkCore;
using Schulkueche.Core;

namespace Schulkueche.Data;

/// <summary>
/// Abstraction to manage persons.
/// </summary>
public interface IPersonRepository
{
    Task<Person> AddAsync(Person person, CancellationToken ct = default);
    Task UpdateAsync(Person person, CancellationToken ct = default);
    Task<Person?> GetAsync(int id, CancellationToken ct = default);
    Task<List<Person>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>
/// Abstraction to manage meal orders for a date.
/// </summary>
public interface IOrderRepository
{
    Task<List<MealOrder>> GetForDateAsync(DateOnly date, CancellationToken ct = default);
    Task UpsertRangeAsync(IEnumerable<MealOrder> orders, CancellationToken ct = default);
}

/// <summary>
/// Abstraction to manage additional charges (e.g., floor carriers for new pensioners).
/// </summary>
public interface IAdditionalChargeRepository
{
    Task<AdditionalCharge> AddAsync(AdditionalCharge charge, CancellationToken ct = default);
    Task UpdateAsync(AdditionalCharge charge, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<List<AdditionalCharge>> GetForPersonAsync(int personId, CancellationToken ct = default);
    Task<List<AdditionalCharge>> GetForMonthAsync(int year, int month, CancellationToken ct = default);
    Task<AdditionalCharge?> GetAsync(int id, CancellationToken ct = default);
}

internal sealed class PersonRepository(KitchenDbContext db) : IPersonRepository
{
    public async Task<Person> AddAsync(Person person, CancellationToken ct = default)
    {
        db.Persons.Add(person);
        await db.SaveChangesAsync(ct);
        return person;
    }

    public async Task UpdateAsync(Person person, CancellationToken ct = default)
    {
        db.Persons.Update(person);
        await db.SaveChangesAsync(ct);
    }

    public Task<Person?> GetAsync(int id, CancellationToken ct = default)
        => db.Persons.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Person>> GetAllAsync(CancellationToken ct = default)
        => db.Persons.OrderBy(p => p.Name).ToListAsync(ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Persons.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null) return;
        db.Persons.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

internal sealed class AdditionalChargeRepository(KitchenDbContext db) : IAdditionalChargeRepository
{
    public async Task<AdditionalCharge> AddAsync(AdditionalCharge charge, CancellationToken ct = default)
    {
        db.AdditionalCharges.Add(charge);
        await db.SaveChangesAsync(ct);
        return charge;
    }

    public async Task UpdateAsync(AdditionalCharge charge, CancellationToken ct = default)
    {
        db.AdditionalCharges.Update(charge);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.AdditionalCharges.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return;
        db.AdditionalCharges.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<List<AdditionalCharge>> GetForPersonAsync(int personId, CancellationToken ct = default)
        => db.AdditionalCharges.Include(c => c.Person)
            .Where(c => c.PersonId == personId)
            .OrderByDescending(c => c.Month)
            .ToListAsync(ct);

    public Task<List<AdditionalCharge>> GetForMonthAsync(int year, int month, CancellationToken ct = default)
    {
        var targetMonth = new DateOnly(year, month, 1);
        return db.AdditionalCharges.Include(c => c.Person)
            .Where(c => c.Month == targetMonth)
            .Where(c => c.Person != null) // Ensure Person is not null
            .OrderBy(c => c.Person!.Name)
            .ToListAsync(ct);
    }

    public Task<AdditionalCharge?> GetAsync(int id, CancellationToken ct = default)
        => db.AdditionalCharges.Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}

internal sealed class OrderRepository(KitchenDbContext db) : IOrderRepository
{
    public Task<List<MealOrder>> GetForDateAsync(DateOnly date, CancellationToken ct = default)
        => db.MealOrders.Include(o => o.Person)
            .Where(o => o.Date == date)
            .OrderBy(o => o.Person!.Name)
            .ToListAsync(ct);

    public async Task UpsertRangeAsync(IEnumerable<MealOrder> orders, CancellationToken ct = default)
    {
        var ordersList = orders.ToList();
        if (!ordersList.Any()) return;
        
        // Get all existing orders for the date and person IDs in a single query
        var date = ordersList.First().Date;
        var personIds = ordersList.Select(o => o.PersonId).ToHashSet();
        
        var existingOrders = await db.MealOrders
            .Where(o => o.Date == date && personIds.Contains(o.PersonId))
            .ToDictionaryAsync(o => o.PersonId, ct);
        
        foreach (var order in ordersList)
        {
            if (existingOrders.TryGetValue(order.PersonId, out var existing))
            {
                existing.Quantity = order.Quantity;
                existing.Delivery = order.Delivery;
            }
            else
            {
                db.MealOrders.Add(order);
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
