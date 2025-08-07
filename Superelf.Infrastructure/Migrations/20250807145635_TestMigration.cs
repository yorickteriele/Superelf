using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Superelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MigrationTestColumn",
                table: "Clubs",
                type: "text",
                nullable: true);

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
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1563), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1581), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1226), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1598), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1240), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1706), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1251), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1262), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1273), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1292), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1303), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1313), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1324), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1335), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1345), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1356), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1508), null });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "MigrationTestColumn" },
                values: new object[] { new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1523), null });

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 14, 56, 34, 410, DateTimeKind.Utc).AddTicks(1183));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MigrationTestColumn",
                table: "Clubs");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6a122ae2-66c8-47f4-849c-dc642a261a49", "AQAAAAIAAYagAAAAEInyG19GQ96TbPZJ+IJfBzDP84Zgv24wevs/OB1rXOFBBfFc+KmffSat+mHYTj1/gw==", "9fe145f1-ef8a-4301-9683-448dff3e0aa4" });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6537));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6547));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6386));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6558));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6399));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6569));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6411));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6421));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6430));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6447));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6458));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6469));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6478));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6490));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6499));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6508));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6517));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6527));

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 4, 14, 28, 46, 227, DateTimeKind.Utc).AddTicks(6332));
        }
    }
}
