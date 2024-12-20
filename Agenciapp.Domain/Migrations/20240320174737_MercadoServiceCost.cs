using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class MercadoServiceCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<decimal>(
                name: "Mercado_Amount",
                table: "ReporteLiquidacion",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Mercado_Date",
                table: "ReporteLiquidacion",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mercado_Number",
                table: "ReporteLiquidacion",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Mercado_Pagado",
                table: "ReporteLiquidacion",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mercado_Status",
                table: "ReporteLiquidacion",
                nullable: true);*/

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCost",
                table: "Mercado",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.DropColumn(
                name: "Mercado_Amount",
                table: "ReporteLiquidacion");

            migrationBuilder.DropColumn(
                name: "Mercado_Date",
                table: "ReporteLiquidacion");

            migrationBuilder.DropColumn(
                name: "Mercado_Number",
                table: "ReporteLiquidacion");

            migrationBuilder.DropColumn(
                name: "Mercado_Pagado",
                table: "ReporteLiquidacion");

            migrationBuilder.DropColumn(
                name: "Mercado_Status",
                table: "ReporteLiquidacion");
           */
            migrationBuilder.DropColumn(
                name: "ServiceCost",
                table: "Mercado");
        }
    }
}
