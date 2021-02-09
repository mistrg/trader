using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Trader.Migrations
{
    public partial class InitialCreate : Migration
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
                    BuyOrderId = table.Column<long>(type: "bigint", nullable: true),
                    BuyWhenCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BuyExchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Pair = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BuyUnitPrice = table.Column<double>(type: "double precision", nullable: true),
                    BuyOrginalAmount = table.Column<double>(type: "double precision", nullable: true),
                    BuyRemainingAmount = table.Column<double>(type: "double precision", nullable: true),
                    BuyStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BuyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SellExchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SellOrigQty = table.Column<double>(type: "double precision", nullable: true),
                    SellOrderListId = table.Column<long>(type: "bigint", nullable: true),
                    SellPrice = table.Column<double>(type: "double precision", nullable: true),
                    SellStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SellTimeInForce = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SellTransactionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SellType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SellClientOrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SellExecutedQty = table.Column<double>(type: "double precision", nullable: true),
                    SellOrderId = table.Column<long>(type: "bigint", nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false)
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
