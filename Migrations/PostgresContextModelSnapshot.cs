﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Trader.PostgresDb;

namespace Trader.Migrations
{
    [DbContext(typeof(PostgresContext))]
    partial class PostgresContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Trader.PostgresDb.Arbitrage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("BotRunId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("BotVersion")
                        .HasColumnType("integer");

                    b.Property<string>("BuyExchange")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<long?>("BuyOrderId")
                        .HasColumnType("bigint");

                    b.Property<double?>("BuyOrginalAmount")
                        .HasColumnType("double precision");

                    b.Property<double?>("BuyRemainingAmount")
                        .HasColumnType("double precision");

                    b.Property<string>("BuyStatus")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("BuyType")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<double?>("BuyUnitPrice")
                        .HasColumnType("double precision");

                    b.Property<DateTime?>("BuyWhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Comment")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<double>("EstBuyFee")
                        .HasColumnType("double precision");

                    b.Property<double>("EstProfitGross")
                        .HasColumnType("double precision");

                    b.Property<double>("EstProfitNet")
                        .HasColumnType("double precision");

                    b.Property<double>("EstProfitNetRate")
                        .HasColumnType("double precision");

                    b.Property<double>("EstSellFee")
                        .HasColumnType("double precision");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("boolean");

                    b.Property<long>("Ocid")
                        .HasColumnType("bigint");

                    b.Property<string>("Pair")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("SellClientOrderId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("SellExchange")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<double?>("SellExecutedQty")
                        .HasColumnType("double precision");

                    b.Property<long?>("SellOrderId")
                        .HasColumnType("bigint");

                    b.Property<long?>("SellOrderListId")
                        .HasColumnType("bigint");

                    b.Property<double?>("SellOrigQty")
                        .HasColumnType("double precision");

                    b.Property<double?>("SellPrice")
                        .HasColumnType("double precision");

                    b.Property<string>("SellStatus")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("SellTimeInForce")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime?>("SellTransactionTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("SellType")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("Arbitrages");
                });
#pragma warning restore 612, 618
        }
    }
}
