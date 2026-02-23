using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SocialNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ReactionTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "ImageUrl", "SortOrder", "Symbol", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "like", new DateTime(2026, 2, 23, 13, 44, 59, 948, DateTimeKind.Utc).AddTicks(9348), null, 10, "👍", new DateTime(2026, 2, 23, 13, 44, 59, 948, DateTimeKind.Utc).AddTicks(9352) },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "love", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(226), null, 20, "❤️", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(228) },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "laugh", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(237), null, 30, "😂", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(238) },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "sad", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(240), null, 40, "😢", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(241) },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "angry", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(243), null, 50, "😡", new DateTime(2026, 2, 23, 13, 44, 59, 949, DateTimeKind.Utc).AddTicks(243) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "ReactionTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));
        }
    }
}
