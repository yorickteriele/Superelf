using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Superelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJerseyNumberAddPhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JerseyNumber",
                table: "FootballPlayers");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "FootballPlayers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9821a247-2d45-4962-8904-9bc1fb990483", "AQAAAAIAAYagAAAAELdlUY6lkmIZ7vcaPgFRa3DeAwbWGI4GAxoHNDYQZFl6eU0aCeAYtvrZpdEdDjdYFw==", "5899b46b-6051-40c5-a9fe-60aaf19ebf47" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "FootballPlayers");

            migrationBuilder.AddColumn<int>(
                name: "JerseyNumber",
                table: "FootballPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0d5b0a2d-dad7-4d23-bd08-88a0d90a8a8b", "AQAAAAIAAYagAAAAEDjDS5rT2pXZLe5SDiKfp8+rovjrIX8q4XSuVCYPtckanb4pdnsFcTTPfcrjmjzAWQ==", "cb547aa4-eed5-447e-acc4-bc0704b2130a" });
        }
    }
}
