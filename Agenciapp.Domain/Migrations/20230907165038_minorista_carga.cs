using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class minorista_carga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.CreateTable(
                name: "HMIncompleteOrdersReceived",
                columns: table => new
                {
                    Number = table.Column<string>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    CantidadEnviada = table.Column<int>(nullable: false),
                    CantidadRecibida = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HMIncompleteOrdersReceived", x => x.Number);
                });*/

            migrationBuilder.CreateTable(
                name: "MinoristaCargas",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MinoristaId = table.Column<Guid>(nullable: false),
                    MayoristaId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinoristaCargas", x => x.Id);
                });

            /*migrationBuilder.CreateTable(
                name: "OrdersDispatchedBaggageReceived",
                columns: table => new
                {
                    OrderNumber = table.Column<string>(nullable: false),
                    ShippingNumber = table.Column<string>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    Qty = table.Column<int>(nullable: false),
                    QtyReceived = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersDispatchedBaggageReceived", x => x.OrderNumber);
                });*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropTable(
                name: "HMIncompleteOrdersReceived");*/

            migrationBuilder.DropTable(
                name: "MinoristaCargas");

            /*migrationBuilder.DropTable(
                name: "OrdersDispatchedBaggageReceived");*/
        }
    }
}
