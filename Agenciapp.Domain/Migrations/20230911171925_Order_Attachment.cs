using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Order_Attachment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Attachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPassport_Description",
                table: "Attachments",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_OrderId",
                table: "Attachments",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Order_OrderId",
                table: "Attachments",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Order_OrderId",
                table: "Attachments");

            /*migrationBuilder.DropTable(
                name: "HMIncompleteOrdersReceived");*/

            /*migrationBuilder.DropTable(
                name: "OrdersDispatchedBaggageReceived");*/

            migrationBuilder.DropIndex(
                name: "IX_Attachments_OrderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AttachmentPassport_Description",
                table: "Attachments");
        }
    }
}
