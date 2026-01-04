using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulkueche.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCustomMealPriceWithDefaultMealQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if DefaultMealQuantity column already exists, if not add it
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS Persons_backup AS SELECT * FROM Persons;
                
                DROP TABLE IF EXISTS Persons;
                
                CREATE TABLE Persons (
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
                
                INSERT INTO Persons (Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, DefaultMealQuantity)
                SELECT Id, Name, Street, HouseNumber, Zip, City, Contact, DefaultDelivery, Category, 
                    COALESCE(
                        (SELECT DefaultMealQuantity FROM Persons_backup WHERE Persons_backup.Id = Persons_backup.Id LIMIT 1),
                        1
                    ) as DefaultMealQuantity
                FROM Persons_backup;
                
                DROP TABLE Persons_backup;
                
                CREATE INDEX IX_Persons_Name ON Persons (Name);
                CREATE INDEX IX_Persons_Name_Category ON Persons (Name, Category);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultMealQuantity",
                table: "Persons");

            migrationBuilder.AddColumn<decimal>(
                name: "CustomMealPrice",
                table: "Persons",
                type: "TEXT",
                nullable: true);
        }
    }
}
