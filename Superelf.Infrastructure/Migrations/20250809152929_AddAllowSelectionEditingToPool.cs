using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Superelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowSelectionEditingToPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "AllowSelectionEditing",
                table: "Pools",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "56f8b69a-a667-4631-a970-3fe3dea3071d", "AQAAAAIAAYagAAAAEBSZf6PWrcspVNZvj2Cccflhu9q8YOUtTCSw9u8xgd4RyV8E5t3jv3+CySbM5DBOIg==", "6f588a1e-5855-4abe-8235-0279d34f476f" });

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 9, 15, 29, 28, 202, DateTimeKind.Utc).AddTicks(9314));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowSelectionEditing",
                table: "Pools");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b69a9d2a-3221-4764-b281-33d36a70f9af", "AQAAAAIAAYagAAAAEPBAnLgiWswkLSrmSz6GBPMoOJ2wDnhl1A9PM+kvSMNwnzOcRIoFvc3jWxE0cuC96g==", "36d1ff68-e7cb-48d6-bbec-6096a55c70ba" });

            migrationBuilder.InsertData(
                table: "Clubs",
                columns: new[] { "Id", "Country", "CreatedAt", "LeagueId", "LogoUrl", "MigrationTestColumn", "Name", "ShortName" },
                values: new object[,]
                {
                    { new Guid("10101010-1010-1010-1010-101010101010"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6973), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "PEC Zwolle", "PEC" },
                    { new Guid("20202020-2020-2020-2020-202020202020"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6984), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "RKC Waalwijk", "RKC" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6681), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Ajax", "AJX" },
                    { new Guid("30303030-3030-3030-3030-303030303030"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6995), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Sparta Rotterdam", "SPA" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6695), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "PSV", "PSV" },
                    { new Guid("40404040-4040-4040-4040-404040404040"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(7008), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Almere City", "ALM" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6821), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Feyenoord", "FEY" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6834), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "AZ", "AZ" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6846), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "FC Utrecht", "UTR" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6868), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "FC Twente", "TWE" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6880), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Vitesse", "VIT" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6893), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "SC Heerenveen", "HEE" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6904), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "FC Groningen", "GRO" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6917), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Willem II", "WIL" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6929), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "NEC", "NEC" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6940), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Fortuna Sittard", "FOR" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6951), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Go Ahead Eagles", "GAE" },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Netherlands", new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6962), new Guid("11111111-1111-1111-1111-111111111111"), null, null, "Heracles Almelo", "HER" }
                });

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6629));
        }
    }
}
