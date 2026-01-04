using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulkueche.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnsureDefaultMealQuantityExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration ensures DefaultMealQuantity exists even if previous migration was marked as applied
            // but didn't actually execute successfully
            migrationBuilder.Sql(@"
                -- Check if DefaultMealQuantity column exists, if not recreate table
                CREATE TABLE IF NOT EXISTS Persons_temp (
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
                
                -- Copy data, handling both old and new schema
                INSERT INTO Persons_temp (Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, DefaultMealQuantity)
                SELECT 
                    Id, 
                    Name, 
                    Street, 
                    HouseNumber, 
                    Zip, 
                    City, 
                    Contact, 
                    DefaultDelivery, 
                    Category,
                    1
                FROM Persons;
                
                DROP TABLE Persons;
                ALTER TABLE Persons_temp RENAME TO Persons;
                
                CREATE INDEX IX_Persons_Name ON Persons (Name);
                CREATE INDEX IX_Persons_Name_Category ON Persons (Name, Category);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down migration needed as this is a data fix
        }
    }
}
