using Microsoft.EntityFrameworkCore.Migrations;

namespace Logiz.Radar.Migrations
{
    public partial class M2019091602 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActualResult",
                table: "TestCase",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActualResult",
                table: "TestCase",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
