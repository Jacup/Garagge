using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefreshToken_UserAgentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "device_name",
                table: "refresh_tokens",
                newName: "location");

            migrationBuilder.AddColumn<string>(
                name: "device_browser",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_os",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_type",
                table: "refresh_tokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_browser",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "device_os",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "device_type",
                table: "refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "location",
                table: "refresh_tokens",
                newName: "device_name");
        }
    }
}
