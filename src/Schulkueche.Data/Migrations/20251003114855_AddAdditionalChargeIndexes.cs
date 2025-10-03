using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schulkueche.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalChargeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdditionalCharges_PersonId",
                table: "AdditionalCharges");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalCharges_Month",
                table: "AdditionalCharges",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalCharges_PersonId_Month",
                table: "AdditionalCharges",
                columns: new[] { "PersonId", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdditionalCharges_Month",
                table: "AdditionalCharges");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalCharges_PersonId_Month",
                table: "AdditionalCharges");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalCharges_PersonId",
                table: "AdditionalCharges",
                column: "PersonId");
        }
    }
}
