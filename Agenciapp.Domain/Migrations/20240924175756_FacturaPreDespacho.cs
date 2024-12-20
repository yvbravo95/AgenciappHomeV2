using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class FacturaPreDespacho : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreDespachoId",
                table: "Facturas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Retail",
                table: "Facturas",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PreDespachoId",
                table: "Facturas",
                column: "PreDespachoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_PreDespachoCubiqs_PreDespachoId",
                table: "Facturas",
                column: "PreDespachoId",
                principalTable: "PreDespachoCubiqs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_PreDespachoCubiqs_PreDespachoId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_PreDespachoId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "PreDespachoId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Retail",
                table: "Facturas");
        }
    }
}
