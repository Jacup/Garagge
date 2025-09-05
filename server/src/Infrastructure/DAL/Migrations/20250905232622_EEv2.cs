using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EEv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_energy_entry_vehicles_vehicle_id",
                table: "energy_entry");

            migrationBuilder.DropTable(
                name: "charging_entries");

            migrationBuilder.DropTable(
                name: "fuel_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_energy_entry",
                table: "energy_entry");

            migrationBuilder.RenameTable(
                name: "energy_entry",
                newName: "energy_entries");

            migrationBuilder.RenameIndex(
                name: "ix_energy_entry_vehicle_id",
                table: "energy_entries",
                newName: "ix_energy_entries_vehicle_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "price_per_unit",
                table: "energy_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "cost",
                table: "energy_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "energy_unit",
                table: "energy_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "energy_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "volume",
                table: "energy_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "pk_energy_entries",
                table: "energy_entries",
                column: "id");

            migrationBuilder.CreateTable(
                name: "vehicle_energy_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    energy_type = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_energy_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_energy_types_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_energy_types_vehicle_id_energy_type",
                table: "vehicle_energy_types",
                columns: new[] { "vehicle_id", "energy_type" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_energy_entries_vehicles_vehicle_id",
                table: "energy_entries",
                column: "vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_energy_entries_vehicles_vehicle_id",
                table: "energy_entries");

            migrationBuilder.DropTable(
                name: "vehicle_energy_types");

            migrationBuilder.DropPrimaryKey(
                name: "pk_energy_entries",
                table: "energy_entries");

            migrationBuilder.DropColumn(
                name: "energy_unit",
                table: "energy_entries");

            migrationBuilder.DropColumn(
                name: "type",
                table: "energy_entries");

            migrationBuilder.DropColumn(
                name: "volume",
                table: "energy_entries");

            migrationBuilder.RenameTable(
                name: "energy_entries",
                newName: "energy_entry");

            migrationBuilder.RenameIndex(
                name: "ix_energy_entries_vehicle_id",
                table: "energy_entry",
                newName: "ix_energy_entry_vehicle_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "price_per_unit",
                table: "energy_entry",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "cost",
                table: "energy_entry",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_energy_entry",
                table: "energy_entry",
                column: "id");

            migrationBuilder.CreateTable(
                name: "charging_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    charging_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    energy_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit = table.Column<int>(type: "integer", nullable: false)
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
                    unit = table.Column<int>(type: "integer", nullable: false),
                    volume = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "fk_energy_entry_vehicles_vehicle_id",
                table: "energy_entry",
                column: "vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
