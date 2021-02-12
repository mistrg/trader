using Microsoft.EntityFrameworkCore.Migrations;

namespace Trader.Migrations
{
    public partial class ArbitrageExtended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AfterBinanceBtcFreeAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AfterCoinmateEuroFreeAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BeforeBinanceBtcFreeAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BeforeCoinmateEuroFreeAmount",
                table: "Arbitrages",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BuyNetPriceCm",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RealProfitNet",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SellNetPriceBi",
                table: "Arbitrages",
                type: "double precision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterBinanceBtcFreeAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "AfterCoinmateEuroFreeAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BeforeBinanceBtcFreeAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BeforeCoinmateEuroFreeAmount",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "BuyNetPriceCm",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "RealProfitNet",
                table: "Arbitrages");

            migrationBuilder.DropColumn(
                name: "SellNetPriceBi",
                table: "Arbitrages");
        }
    }
}
