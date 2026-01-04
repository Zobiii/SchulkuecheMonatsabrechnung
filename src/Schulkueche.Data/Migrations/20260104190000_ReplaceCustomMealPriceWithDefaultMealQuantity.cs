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
            migrationBuilder.DropColumn(
                name: "CustomMealPrice",
                table: "Persons");

            migrationBuilder.AddColumn<int>(
                name: "DefaultMealQuantity",
                table: "Persons",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
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
