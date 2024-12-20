using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class RefundLog_RemittanceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefundLogs_Order_RemesaId",
                table: "RefundLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundLogs_Remittance_RemesaId",
                table: "RefundLogs",
                column: "RemesaId",
                principalTable: "Remittance",
                principalColumn: "RemittanceId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefundLogs_Remittance_RemesaId",
                table: "RefundLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundLogs_Order_RemesaId",
                table: "RefundLogs",
                column: "RemesaId",
                principalTable: "Order",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
