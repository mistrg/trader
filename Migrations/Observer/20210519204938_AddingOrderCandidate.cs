using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Trader.Migrations.Observer
{
    public partial class AddingOrderCandidate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.CreateTable(
                name: "OrderCandidates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    BotRunId = table.Column<string>(type: "text", nullable: true),
                    BotVersion = table.Column<int>(type: "integer", nullable: false),
                    BuyExchange = table.Column<string>(type: "text", nullable: true),
                    EstBuyFee = table.Column<double>(type: "double precision", nullable: false),
                    EstProfitGross = table.Column<double>(type: "double precision", nullable: false),
                    EstProfitNet = table.Column<double>(type: "double precision", nullable: false),
                    EstProfitNetRate = table.Column<double>(type: "double precision", nullable: false),
                    EstSellFee = table.Column<double>(type: "double precision", nullable: false),
                    Pair = table.Column<string>(type: "text", nullable: true),
                    SellExchange = table.Column<string>(type: "text", nullable: true),
                    TotalAskPrice = table.Column<double>(type: "double precision", nullable: false),
                    TotalBidPrice = table.Column<double>(type: "double precision", nullable: false),
                    UnitAskPrice = table.Column<double>(type: "double precision", nullable: false),
                    UnitBidPrice = table.Column<double>(type: "double precision", nullable: false),
                    WhenBuySpoted = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WhenCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WhenSellSpoted = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCandidates", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
