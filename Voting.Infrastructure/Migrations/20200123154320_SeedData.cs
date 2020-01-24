using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Model.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Blocks",
                columns: new[] { "Id", "Difficulty", "Hash", "Nonce", "PreviousHash", "Timestamp" },
                values: new object[] { 1, 2, new byte[] { 140, 242, 80, 5, 231, 184, 72, 157, 135, 76, 89, 124, 93, 52, 81, 74, 1, 16, 163, 249, 228, 189, 149, 236, 238, 143, 218, 199, 179, 18, 161, 98 }, 0, null, 0L });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Blocks",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
