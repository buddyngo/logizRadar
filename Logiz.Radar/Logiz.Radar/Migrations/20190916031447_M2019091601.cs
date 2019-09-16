using Microsoft.EntityFrameworkCore.Migrations;

namespace Logiz.Radar.Migrations
{
    public partial class M2019091601 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TestCase");

            migrationBuilder.AddColumn<int>(
                name: "TestStatus",
                table: "TestCase",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestStatus",
                table: "TestCase");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TestCase",
                nullable: false,
                defaultValue: 0);
        }
    }
}
