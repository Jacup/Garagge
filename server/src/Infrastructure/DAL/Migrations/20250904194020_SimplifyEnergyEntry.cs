using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyEnergyEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price_per_unit",
                table: "fuel_entries");

            migrationBuilder.DropColumn(
                name: "price_per_unit",
                table: "charging_entries");

            migrationBuilder.AddColumn<decimal>(
                name: "price_per_unit",
                table: "energy_entry",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price_per_unit",
                table: "energy_entry");

            migrationBuilder.AddColumn<decimal>(
                name: "price_per_unit",
                table: "fuel_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "price_per_unit",
                table: "charging_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
