using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Schulkueche.Data;

public static class DbInitializer
{
    public static async Task EnsureDatabaseUpdatedAsync(KitchenDbContext db)
    {
        // Manual migration for v1.4.1: Replace CustomMealPrice with DefaultMealQuantity
        await ApplyManualMigrationAsync(db);
        
        await db.Database.MigrateAsync();
    }
    
    private static async Task ApplyManualMigrationAsync(KitchenDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            
            // Check if CustomMealPrice column exists
            command.CommandText = "PRAGMA table_info(Persons);";
            using var reader = await command.ExecuteReaderAsync();
            
            bool hasCustomMealPrice = false;
            bool hasDefaultMealQuantity = false;
            
            while (await reader.ReadAsync())
            {
                var columnName = reader.GetString(1);
                if (columnName == "CustomMealPrice") hasCustomMealPrice = true;
                if (columnName == "DefaultMealQuantity") hasDefaultMealQuantity = true;
            }
            reader.Close();
            
            // If we have old structure, migrate it
            if (hasCustomMealPrice && !hasDefaultMealQuantity)
            {
                // Add new column
                using var addColumnCmd = connection.CreateCommand();
                addColumnCmd.CommandText = "ALTER TABLE Persons ADD COLUMN DefaultMealQuantity INTEGER NOT NULL DEFAULT 1;";
                await addColumnCmd.ExecuteNonQueryAsync();
                
                // Remove old column (SQLite doesn't support DROP COLUMN directly in older versions)
                // We'll recreate the table without CustomMealPrice
                await RecreatePersonsTableAsync(connection);
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    private static async Task RecreatePersonsTableAsync(System.Data.Common.DbConnection connection)
    {
        using var transaction = connection.BeginTransaction();
        try
        {
            // Create new table with correct structure
            using var createCmd = connection.CreateCommand();
            createCmd.Transaction = transaction;
            createCmd.CommandText = @"
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
                );";
            await createCmd.ExecuteNonQueryAsync();
            
            // Copy data from old table to new table
            using var copyCmd = connection.CreateCommand();
            copyCmd.Transaction = transaction;
            copyCmd.CommandText = @"
                INSERT INTO Persons_new (Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, DefaultMealQuantity)
                SELECT Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, 1
                FROM Persons;";
            await copyCmd.ExecuteNonQueryAsync();
            
            // Drop old table
            using var dropCmd = connection.CreateCommand();
            dropCmd.Transaction = transaction;
            dropCmd.CommandText = "DROP TABLE Persons;";
            await dropCmd.ExecuteNonQueryAsync();
            
            // Rename new table to original name
            using var renameCmd = connection.CreateCommand();
            renameCmd.Transaction = transaction;
            renameCmd.CommandText = "ALTER TABLE Persons_new RENAME TO Persons;";
            await renameCmd.ExecuteNonQueryAsync();
            
            // Recreate indexes
            using var indexCmd1 = connection.CreateCommand();
            indexCmd1.Transaction = transaction;
            indexCmd1.CommandText = "CREATE INDEX IX_Persons_Name ON Persons (Name);";
            await indexCmd1.ExecuteNonQueryAsync();
            
            using var indexCmd2 = connection.CreateCommand();
            indexCmd2.Transaction = transaction;
            indexCmd2.CommandText = "CREATE INDEX IX_Persons_Name_Category ON Persons (Name, Category);";
            await indexCmd2.ExecuteNonQueryAsync();
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
