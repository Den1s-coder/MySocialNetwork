using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "User");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 23, 13, 44, 59, 948, DateTimeKind.Utc).AddTicks(9348), new DateTime(2026, 2, 23, 13, 44, 59, 948, DateTimeKind.Utc).AddTicks(9352) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(226), new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(228) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(237), new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(238) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(240), new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(241) });

            migrationBuilder.UpdateData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(243), new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(243) });
        }
    }
}
