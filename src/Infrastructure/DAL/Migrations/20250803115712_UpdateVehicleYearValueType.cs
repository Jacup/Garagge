using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleYearValueType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE vehicles 
                ALTER COLUMN manufactured_year 
                TYPE integer 
                USING EXTRACT(YEAR FROM manufactured_year);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE vehicles 
                ALTER COLUMN manufactured_year 
                DROP NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE vehicles 
                ALTER COLUMN manufactured_year 
                TYPE date 
                USING TO_DATE(manufactured_year || '-01-01', 'YYYY-MM-DD');
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE vehicles 
                ALTER COLUMN manufactured_year 
                SET NOT NULL;
            ");        }
    }
}
