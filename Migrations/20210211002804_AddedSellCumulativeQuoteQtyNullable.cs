using Microsoft.EntityFrameworkCore.Migrations;

namespace Trader.Migrations
{
    public partial class AddedSellCumulativeQuoteQtyNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SellCummulativeQuoteQty",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellCummulativeQuoteQty",
                table: "Arbitrages");
        }
    }
}
