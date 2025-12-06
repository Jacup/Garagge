using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Extend_Refresh_Token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "device_name",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_revoked",
                table: "refresh_tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "replaced_by_token",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "refresh_tokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_name",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "is_revoked",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "replaced_by_token",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "refresh_tokens");
        }
    }
}
