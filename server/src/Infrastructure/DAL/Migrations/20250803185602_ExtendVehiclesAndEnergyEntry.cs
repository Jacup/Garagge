using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ExtendVehiclesAndEnergyEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "power_type",
                table: "vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vin",
                table: "vehicles",
                type: "character varying(17)",
                maxLength: 17,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "energy_entry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    mileage = table.Column<int>(type: "integer", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_energy_entry", x => x.id);
                    table.ForeignKey(
                        name: "fk_energy_entry_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "charging_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    energy_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    price_per_unit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    charging_duration_minutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_charging_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_charging_entries_energy_entry_id",
                        column: x => x.id,
                        principalTable: "energy_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fuel_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    volume = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit = table.Column<int>(type: "integer", nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuel_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_fuel_entries_energy_entry_id",
                        column: x => x.id,
                        principalTable: "energy_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_energy_entry_vehicle_id",
                table: "energy_entry",
                column: "vehicle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "charging_entries");

            migrationBuilder.DropTable(
                name: "fuel_entries");

            migrationBuilder.DropTable(
                name: "energy_entry");

            migrationBuilder.DropColumn(
                name: "power_type",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "type",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "vin",
                table: "vehicles");
        }
    }
}
