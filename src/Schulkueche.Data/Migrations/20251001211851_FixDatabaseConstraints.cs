using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulkueche.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDatabaseConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Persons_Name",
                table: "Persons",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_Name",
                table: "Persons");
        }
    }
}
