using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Schulkueche.Data;

public static class DbInitializer
{
    public static async Task EnsureDatabaseUpdatedAsync(KitchenDbContext db)
    {
        // First check and fix the schema if needed
        await EnsureSchemaIsCorrectAsync(db);
        
        // Then apply any pending migrations
        await db.Database.MigrateAsync();
    }
    
    private static async Task EnsureSchemaIsCorrectAsync(KitchenDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        
        try
        {
            // First check if Persons table exists at all
            using var tableCheckCmd = connection.CreateCommand();
            tableCheckCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Persons';";
            var tableExists = Convert.ToInt32(await tableCheckCmd.ExecuteScalarAsync()) > 0;
            
            if (!tableExists)
            {
                // Table doesn't exist yet - let EF migrations create it
                return;
            }
            
            // Check if DefaultMealQuantity column exists
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Persons') WHERE name='DefaultMealQuantity';";
            var hasColumn = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
            
            if (!hasColumn)
            {
                // Column missing - recreate table with correct schema
                using var transaction = connection.BeginTransaction();
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                        CREATE TABLE Persons_new (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Street TEXT,
                            HouseNumber TEXT,
                            Zip TEXT,
                            City TEXT,
                            Contact TEXT,
                            DefaultDelivery INTEGER NOT NULL,
                            Category INTEGER NOT NULL,
                            DefaultMealQuantity INTEGER NOT NULL DEFAULT 1
                        );
                        
                        INSERT INTO Persons_new (Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, DefaultMealQuantity)
                        SELECT Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, 1
                        FROM Persons;
                        
                        DROP TABLE Persons;
                        ALTER TABLE Persons_new RENAME TO Persons;
                        
                        CREATE INDEX IX_Persons_Name ON Persons (Name);
                        CREATE INDEX IX_Persons_Name_Category ON Persons (Name, Category);
                    ";
                    await cmd.ExecuteNonQueryAsync();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }
}
