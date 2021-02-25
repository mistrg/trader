using Microsoft.EntityFrameworkCore.Migrations;

namespace Trader.Migrations
{
    public partial class walletdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BeforeSellExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "WalletQuoteAmountSurplus");

            migrationBuilder.RenameColumn(
                name: "BeforeBuyExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "WalletBaseAmountSurplus");

            migrationBuilder.RenameColumn(
                name: "AfterSellExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "SellCummulativeFeeQuote");

            migrationBuilder.RenameColumn(
                name: "AfterBuyExchangeAvailableAmount",
                table: "Arbitrages",
                newName: "BuyCummulativeFeeQuote");

            migrationBuilder.AddColumn<double>(
                name: "AfterBuyExchangeAvailableBaseAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AfterBuyExchangeAvailableQuoteAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AfterSellExchangeAvailableBaseAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AfterSellExchangeAvailableQuoteAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BeforeBuyExchangeAvailableBaseAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BeforeBuyExchangeAvailableQuoteAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BeforeSellExchangeAvailableBaseAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BeforeSellExchangeAvailableQuoteAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterBuyExchangeAvailableBaseAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "AfterBuyExchangeAvailableQuoteAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "AfterSellExchangeAvailableBaseAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "AfterSellExchangeAvailableQuoteAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BeforeBuyExchangeAvailableBaseAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BeforeBuyExchangeAvailableQuoteAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BeforeSellExchangeAvailableBaseAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BeforeSellExchangeAvailableQuoteAmount",
                table: "Arbitrages");

            migrationBuilder.RenameColumn(
                name: "WalletQuoteAmountSurplus",
                table: "Arbitrages",
                newName: "BeforeSellExchangeAvailableAmount");

            migrationBuilder.RenameColumn(
                name: "WalletBaseAmountSurplus",
                table: "Arbitrages",
                newName: "BeforeBuyExchangeAvailableAmount");

            migrationBuilder.RenameColumn(
                name: "SellCummulativeFeeQuote",
                table: "Arbitrages",
                newName: "AfterSellExchangeAvailableAmount");

            migrationBuilder.RenameColumn(
                name: "BuyCummulativeFeeQuote",
                table: "Arbitrages",
                newName: "AfterBuyExchangeAvailableAmount");
        }
    }
}
