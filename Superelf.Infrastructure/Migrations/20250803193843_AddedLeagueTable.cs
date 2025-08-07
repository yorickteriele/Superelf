using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Superelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedLeagueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarEventId",
                table: "Matches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeagueId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeagueId",
                table: "Clubs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    GoogleCalendarId = table.Column<string>(type: "text", nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4a23a313-f114-43d4-b10a-9c21bb0ebf4a", "AQAAAAIAAYagAAAAEErMXXi/YiWHCA1cZJmmOBPP5IaAJG7xrL+iAVu4SU0+DmI7noxXBSyG9IErxwlp5g==", "c8a3484b-842f-4477-b92b-b5945fb67352" });

            migrationBuilder.InsertData(
                table: "Leagues",
                columns: new[] { "Id", "Country", "CreatedAt", "GoogleCalendarId", "IsActive", "LastSyncAt", "LogoUrl", "Name", "ShortName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9587), null, true, null, null, "Eredivisie", "ERE" });

            migrationBuilder.InsertData(
                table: "Clubs",
                columns: new[] { "Id", "Country", "CreatedAt", "LeagueId", "LogoUrl", "Name", "ShortName" },
                values: new object[,]
                {
                    { new Guid("10101010-1010-1010-1010-101010101010"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9962), new Guid("11111111-1111-1111-1111-111111111111"), null, "PEC Zwolle", "PEC" },
                    { new Guid("20202020-2020-2020-2020-202020202020"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9980), new Guid("11111111-1111-1111-1111-111111111111"), null, "RKC Waalwijk", "RKC" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9672), new Guid("11111111-1111-1111-1111-111111111111"), null, "Ajax", "AJX" },
                    { new Guid("30303030-3030-3030-3030-303030303030"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 936, DateTimeKind.Utc).AddTicks(87), new Guid("11111111-1111-1111-1111-111111111111"), null, "Sparta Rotterdam", "SPA" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9718), new Guid("11111111-1111-1111-1111-111111111111"), null, "PSV", "PSV" },
                    { new Guid("40404040-4040-4040-4040-404040404040"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 936, DateTimeKind.Utc).AddTicks(110), new Guid("11111111-1111-1111-1111-111111111111"), null, "Almere City", "ALM" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9736), new Guid("11111111-1111-1111-1111-111111111111"), null, "Feyenoord", "FEY" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9755), new Guid("11111111-1111-1111-1111-111111111111"), null, "AZ", "AZ" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9772), new Guid("11111111-1111-1111-1111-111111111111"), null, "FC Utrecht", "UTR" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9803), new Guid("11111111-1111-1111-1111-111111111111"), null, "FC Twente", "TWE" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9821), new Guid("11111111-1111-1111-1111-111111111111"), null, "Vitesse", "VIT" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9838), new Guid("11111111-1111-1111-1111-111111111111"), null, "SC Heerenveen", "HEE" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9857), new Guid("11111111-1111-1111-1111-111111111111"), null, "FC Groningen", "GRO" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9877), new Guid("11111111-1111-1111-1111-111111111111"), null, "Willem II", "WIL" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9895), new Guid("11111111-1111-1111-1111-111111111111"), null, "NEC", "NEC" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9912), new Guid("11111111-1111-1111-1111-111111111111"), null, "Fortuna Sittard", "FOR" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9929), new Guid("11111111-1111-1111-1111-111111111111"), null, "Go Ahead Eagles", "GAE" },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Netherlands", new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9946), new Guid("11111111-1111-1111-1111-111111111111"), null, "Heracles Almelo", "HER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LeagueId",
                table: "Matches",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_LeagueId",
                table: "Clubs",
                column: "LeagueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_Leagues_LeagueId",
                table: "Clubs",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Leagues_LeagueId",
                table: "Matches",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_Leagues_LeagueId",
                table: "Clubs");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Leagues_LeagueId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropIndex(
                name: "IX_Matches_LeagueId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Clubs_LeagueId",
                table: "Clubs");

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));

            migrationBuilder.DropColumn(
                name: "GoogleCalendarEventId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LeagueId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LeagueId",
                table: "Clubs");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9821a247-2d45-4962-8904-9bc1fb990483", "AQAAAAIAAYagAAAAELdlUY6lkmIZ7vcaPgFRa3DeAwbWGI4GAxoHNDYQZFl6eU0aCeAYtvrZpdEdDjdYFw==", "5899b46b-6051-40c5-a9fe-60aaf19ebf47" });
        }
    }
}
