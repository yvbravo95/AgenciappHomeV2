using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Minorista_AddPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<decimal>(
                name: "Order_CantLb",
                table: "ReporteLiquidacion",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Order_CantLbMedicina",
                table: "ReporteLiquidacion",
                nullable: true);*/

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Minoristas",
                nullable: true);

            /*migrationBuilder.CreateTable(
                name: "OrdersByProvince",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(nullable: false),
                    PrincipalDistributorId = table.Column<Guid>(nullable: false),
                    Estado = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    City = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersByProvince", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "OrdersReceivedByAgency",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(nullable: false),
                    PrincipalDistributorId = table.Column<Guid>(nullable: false),
                    Estado = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    AgencyName = table.Column<string>(nullable: true),
                    AgencyTransferredName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersReceivedByAgency", x => x.OrderId);
                });*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropTable(
                name: "OrdersByProvince");

            migrationBuilder.DropTable(
                name: "OrdersReceivedByAgency");

            migrationBuilder.DropColumn(
                name: "Order_CantLb",
                table: "ReporteLiquidacion");

            migrationBuilder.DropColumn(
                name: "Order_CantLbMedicina",
                table: "ReporteLiquidacion");*/

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Minoristas");
        }
    }
}
