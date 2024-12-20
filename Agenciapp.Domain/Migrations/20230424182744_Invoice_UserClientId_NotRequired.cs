using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Invoice_UserClientId_NotRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_UserClients_UserClientId",
                table: "Invoices");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserClientId",
                table: "Invoices",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_UserClients_UserClientId",
                table: "Invoices",
                column: "UserClientId",
                principalTable: "UserClients",
                principalColumn: "UserClientId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_UserClients_UserClientId",
                table: "Invoices");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserClientId",
                table: "Invoices",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_UserClients_UserClientId",
                table: "Invoices",
                column: "UserClientId",
                principalTable: "UserClients",
                principalColumn: "UserClientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
