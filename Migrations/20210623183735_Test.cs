using Microsoft.EntityFrameworkCore.Migrations;

namespace Trader.Migrations
{
    public partial class Test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyNetPrice",
                table: "Arbitrages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BuyNetPrice",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);
        }
    }
}
