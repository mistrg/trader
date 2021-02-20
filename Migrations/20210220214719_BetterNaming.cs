using Microsoft.EntityFrameworkCore.Migrations;

namespace Trader.Migrations
{
    public partial class BetterNaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BeforeCoinmateEuroFreeAmount",
                table: "Arbitrages",
                newName: "BeforeSellExchangeAvailableAmount");

            migrationBuilder.RenameColumn(
                name: "BeforeBinanceBtcFreeAmount",
                table: "Arbitrages",
                newName: "BeforeBuyExchangeAvailableAmount");

            migrationBuilder.RenameColumn(
                name: "AfterCoinmateEuroFreeAmount",
                table: "Arbitrages",
                newName: "AfterSellExchangeAvailableAmount");

            migrationBuilder.RenameColumn(
                name: "AfterBinanceBtcFreeAmount",
                table: "Arbitrages",
                newName: "AfterBuyExchangeAvailableAmount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BeforeSellExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "BeforeCoinmateEuroFreeAmount");

            migrationBuilder.RenameColumn(
                name: "BeforeBuyExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "BeforeBinanceBtcFreeAmount");

            migrationBuilder.RenameColumn(
                name: "AfterSellExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "AfterCoinmateEuroFreeAmount");

            migrationBuilder.RenameColumn(
                name: "AfterBuyExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "AfterBinanceBtcFreeAmount");
        }
    }
}
