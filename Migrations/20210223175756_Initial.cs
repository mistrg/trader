using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Trader.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arbitrages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstProfitGross = table.Column<double>(type: "double precision", nullable: false),
                    EstProfitNet = table.Column<double>(type: "double precision", nullable: false),
                    EstProfitNetRate = table.Column<double>(type: "double precision", nullable: false),
                    EstBuyFee = table.Column<double>(type: "double precision", nullable: false),
                    EstSellFee = table.Column<double>(type: "double precision", nullable: false),
                    BotRunId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BotVersion = table.Column<int>(type: "integer", nullable: false),
                    Ocid = table.Column<long>(type: "bigint", nullable: false),
                    Pair = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BuyOrderId = table.Column<long>(type: "bigint", nullable: true),
                    BuyWhenCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BuyExchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BuyUnitPrice = table.Column<double>(type: "double precision", nullable: true),
                    BuyOrginalAmount = table.Column<double>(type: "double precision", nullable: true),
                    BuyRemainingAmount = table.Column<double>(type: "double precision", nullable: true),
                    BuyStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BuyComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BuyCummulativeQuoteQty = table.Column<double>(type: "double precision", nullable: true),
                    BuyCummulativeFee = table.Column<double>(type: "double precision", nullable: true),
                    BuyNetPrice = table.Column<double>(type: "double precision", nullable: true),
                    SellExchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SellWhenCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SellComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SellOrginalAmount = table.Column<double>(type: "double precision", nullable: true),
                    SellRemainingAmount = table.Column<double>(type: "double precision", nullable: true),
                    SellCummulativeFee = table.Column<double>(type: "double precision", nullable: true),
                    SellCummulativeQuoteQty = table.Column<double>(type: "double precision", nullable: true),
                    SellStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SellOrderId = table.Column<long>(type: "bigint", nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    SellNetPrice = table.Column<double>(type: "double precision", nullable: true),
                    RealProfitNet = table.Column<double>(type: "double precision", nullable: true),
                    BeforeSellExchangeAvailableAmount = table.Column<double>(type: "double precision", nullable: true),
                    BeforeBuyExchangeAvailableAmount = table.Column<double>(type: "double precision", nullable: true),
                    AfterSellExchangeAvailableAmount = table.Column<double>(type: "double precision", nullable: true),
                    AfterBuyExchangeAvailableAmount = table.Column<double>(type: "double precision", nullable: true),
                    RealProfitNetRate = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arbitrages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arbitrages");
        }
    }
}
