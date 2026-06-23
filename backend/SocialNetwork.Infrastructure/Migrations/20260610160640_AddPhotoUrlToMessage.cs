using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoUrlToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Messages",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 16, 6, 39, 956, DateTimeKind.Utc).AddTicks(9397), new DateTime(2026, 6, 10, 16, 6, 39, 956, DateTimeKind.Utc).AddTicks(9400) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(363), new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(364) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(371), new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(372) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(374), new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(374) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(376), new DateTime(2026, 6, 10, 16, 6, 39, 957, DateTimeKind.Utc).AddTicks(376) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Messages");

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 6, 59, 46, 719, DateTimeKind.Utc).AddTicks(9416), new DateTime(2026, 3, 12, 6, 59, 46, 719, DateTimeKind.Utc).AddTicks(9419) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(284), new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(284) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(290), new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(291) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(293), new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(293) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(295), new DateTime(2026, 3, 12, 6, 59, 46, 720, DateTimeKind.Utc).AddTicks(295) });
        }
    }
}
