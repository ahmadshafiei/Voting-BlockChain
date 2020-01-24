using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Model.Migrations
{
    public partial class DataJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Blocks",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Blocks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Data", "Hash" },
                values: new object[] { "[]", new byte[] { 76, 138, 47, 41, 137, 101, 32, 5, 136, 170, 19, 208, 175, 228, 48, 222, 168, 0, 52, 184, 228, 69, 124, 35, 140, 25, 214, 14, 199, 169, 202, 93 } });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Blocks");

            migrationBuilder.UpdateData(
                table: "Blocks",
                keyColumn: "Id",
                keyValue: 1,
                column: "Hash",
                value: new byte[] { 140, 242, 80, 5, 231, 184, 72, 157, 135, 76, 89, 124, 93, 52, 81, 74, 1, 16, 163, 249, 228, 189, 149, 236, 238, 143, 218, 199, 179, 18, 161, 98 });
        }
    }
}
