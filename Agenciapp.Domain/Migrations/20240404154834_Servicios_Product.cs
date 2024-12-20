using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Servicios_Product : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServicioId",
                table: "ProductosVendidos",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosVendidos_ServicioId",
                table: "ProductosVendidos",
                column: "ServicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosVendidos_Servicios_ServicioId",
                table: "ProductosVendidos",
                column: "ServicioId",
                principalTable: "Servicios",
                principalColumn: "ServicioId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductosVendidos_Servicios_ServicioId",
                table: "ProductosVendidos");

            migrationBuilder.DropIndex(
                name: "IX_ProductosVendidos_ServicioId",
                table: "ProductosVendidos");

            migrationBuilder.DropColumn(
                name: "ServicioId",
                table: "ProductosVendidos");
        }
    }
}
