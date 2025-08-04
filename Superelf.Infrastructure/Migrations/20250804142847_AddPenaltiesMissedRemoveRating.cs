using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Superelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPenaltiesMissedRemoveRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PlayerPerformances");

            migrationBuilder.AddColumn<int>(
                name: "PenaltiesMissed",
                table: "PlayerPerformances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltiesMissed",
                table: "PlayerPerformances");

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "PlayerPerformances",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "550e8400-e29b-41d4-a716-446655440002",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4a23a313-f114-43d4-b10a-9c21bb0ebf4a", "AQAAAAIAAYagAAAAEErMXXi/YiWHCA1cZJmmOBPP5IaAJG7xrL+iAVu4SU0+DmI7noxXBSyG9IErxwlp5g==", "c8a3484b-842f-4477-b92b-b5945fb67352" });

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9962));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9980));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9672));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 936, DateTimeKind.Utc).AddTicks(87));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9718));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 936, DateTimeKind.Utc).AddTicks(110));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9736));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9755));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9772));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9803));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9821));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9838));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9857));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9877));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9895));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9912));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9929));

            migrationBuilder.UpdateData(
                table: "Clubs",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9946));

            migrationBuilder.UpdateData(
                table: "Leagues",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 8, 3, 19, 38, 42, 935, DateTimeKind.Utc).AddTicks(9587));
        }
    }
}
