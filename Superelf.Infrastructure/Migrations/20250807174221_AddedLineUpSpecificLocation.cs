using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Superelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedLineUpSpecificLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecificPosition",
                table: "LineupLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b69a9d2a-3221-4764-b281-33d36a70f9af", "AQAAAAIAAYagAAAAEPBAnLgiWswkLSrmSz6GBPMoOJ2wDnhl1A9PM+kvSMNwnzOcRIoFvc3jWxE0cuC96g==", "36d1ff68-e7cb-48d6-bbec-6096a55c70ba" });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6973));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6984));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6681));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6995));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6695));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(7008));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6821));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6834));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6846));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6868));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6880));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6893));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6904));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6917));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6929));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6940));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6951));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6962));

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 17, 42, 21, 154, DateTimeKind.Utc).AddTicks(6629));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecificPosition",
                table: "LineupLines");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b5e3bef1-f155-4952-892b-a08686b3f08f", "AQAAAAIAAYagAAAAEERcqYt4TfMjVDj5w3yUJ7LmCW2SRQVy5j+ClxkbQq/8xO5mU2XMFnm1vLYdHe336w==", "a15d5ae5-a6d8-4efa-92fb-1576e367aa4c" });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1563));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1581));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1226));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1598));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1240));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1706));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1251));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1262));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1273));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1292));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1303));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1313));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1324));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1335));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1345));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1356));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1508));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1523));

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1183));
        }
    }
}
