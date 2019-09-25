using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Logiz.Radar.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ProjectName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TestCase",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    TestCaseName = table.Column<string>(nullable: false),
                    TestVariantID = table.Column<string>(nullable: false),
                    TestCaseSteps = table.Column<string>(nullable: false),
                    ExpectedResult = table.Column<string>(nullable: false),
                    ActualResult = table.Column<string>(nullable: true),
                    TesterName = table.Column<string>(nullable: true),
                    PlannedDate = table.Column<DateTime>(nullable: false),
                    TestStatus = table.Column<string>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCase", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TestCaseAttachment",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    TestCaseID = table.Column<string>(nullable: true),
                    FullFileName = table.Column<string>(nullable: true),
                    OriginalFileName = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCaseAttachment", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TestScenario",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ScenarioName = table.Column<string>(nullable: false),
                    ProjectID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestScenario", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TestVariant",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    VariantName = table.Column<string>(nullable: false),
                    ScenarioID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestVariant", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserMappingProject",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Username = table.Column<string>(nullable: false),
                    ProjectID = table.Column<string>(nullable: false),
                    CanWrite = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMappingProject", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "TestCase");

            migrationBuilder.DropTable(
                name: "TestCaseAttachment");

            migrationBuilder.DropTable(
                name: "TestScenario");

            migrationBuilder.DropTable(
                name: "TestVariant");

            migrationBuilder.DropTable(
                name: "UserMappingProject");
        }
    }
}
