using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Trader.Migrations.Observer
{
    public partial class Telemetry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BotRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    WhenCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotRuns", x => x.Id);
                });

           
            migrationBuilder.CreateTable(
                name: "ExchangeRuntimeInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExchangeName = table.Column<string>(type: "text", nullable: true),
                    BotRunId = table.Column<string>(type: "text", nullable: true),
                    OrderBookTotalCount = table.Column<long>(type: "bigint", nullable: false),
                    OrderBookSuccessCount = table.Column<long>(type: "bigint", nullable: false),
                    OrderBookFailCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRuntimeInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRuntimeInfo_BotRuns_BotRunId",
                        column: x => x.BotRunId,
                        principalTable: "BotRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRuntimeInfo_BotRunId",
                table: "ExchangeRuntimeInfo",
                column: "BotRunId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRuntimeInfo");

            migrationBuilder.DropTable(
                name: "OrderCandidates");

            migrationBuilder.DropTable(
                name: "BotRuns");
        }
    }
}
