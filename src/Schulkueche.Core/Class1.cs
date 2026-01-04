using System.ComponentModel.DataAnnotations;

namespace Schulkueche.Core;

/// <summary>
/// Types of people/groups supported by the system.
/// </summary>
public enum PersonCategory
{
    Pensioner = 0,
    ChildGroup = 1,
    FreeMeal = 2
}

/// <summary>
/// Person or group receiving meals.
/// </summary>
public class Person
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(120)]
    public string? Street { get; set; }

    [MaxLength(20)]
    public string? HouseNumber { get; set; }

    [MaxLength(10)]
    public string? Zip { get; set; }

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(120)]
    public string? Contact { get; set; }

    /// <summary>
    /// Default delivery preference shown in the daily capture.
    /// </summary>
    public bool DefaultDelivery { get; set; }

    public PersonCategory Category { get; set; }

    /// <summary>
    /// Default meal quantity for automatic pre-filling (only for Pensioners). Default is 1.
    /// </summary>
    public int DefaultMealQuantity { get; set; } = 1;
}

/// <summary>
/// A meal order entry for a specific date.
/// </summary>
public class MealOrder
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public int Quantity { get; set; }
    public bool Delivery { get; set; }
}

/// <summary>
/// Additional charge item, e.g., floor carriers for a newly added pensioner in a month.
/// </summary>
public class AdditionalCharge
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public DateOnly Month { get; set; } // Use first day of month as key
    [MaxLength(200)]
    public required string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Represents a user who can log into the system
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public required string Username { get; set; }
    
    [MaxLength(255)]
    public required string PasswordHash { get; set; }
    
    [MaxLength(255)]
    public required string Email { get; set; }
    
    public bool IsAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Static constants for business defaults.
/// Values can be overridden via configuration later.
/// </summary>
public static class PricingDefaults
{
    public const decimal PensionerMealPrice = 4.50m;
    public const decimal DeliverySurcharge = 3.50m;
    public const decimal ChildMealPrice = 2.90m;
}
