using Microsoft.EntityFrameworkCore.Migrations;

namespace Trader.Migrations.Observer
{
    public partial class VersionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "BotRuns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "BotRuns");
        }
    }
}
