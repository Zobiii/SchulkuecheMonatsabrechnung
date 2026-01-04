using Microsoft.EntityFrameworkCore;

namespace Schulkueche.Data;

public static class DbInitializer
{
    public static async Task EnsureDatabaseUpdatedAsync(KitchenDbContext db)
    {
        await db.Database.MigrateAsync();
    }
}
