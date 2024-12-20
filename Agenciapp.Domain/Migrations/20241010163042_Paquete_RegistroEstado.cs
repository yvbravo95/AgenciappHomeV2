using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Paquete_RegistroEstado : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaqueteId",
                table: "RegistroEstado",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseLocation",
                table: "Paquete",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroEstado_PaqueteId",
                table: "RegistroEstado",
                column: "PaqueteId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroEstado_Paquete_PaqueteId",
                table: "RegistroEstado",
                column: "PaqueteId",
                principalTable: "Paquete",
                principalColumn: "PaqueteId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistroEstado_Paquete_PaqueteId",
                table: "RegistroEstado");

            migrationBuilder.DropIndex(
                name: "IX_RegistroEstado_PaqueteId",
                table: "RegistroEstado");

            migrationBuilder.DropColumn(
                name: "PaqueteId",
                table: "RegistroEstado");

            migrationBuilder.DropColumn(
                name: "WarehouseLocation",
                table: "Paquete");
        }
    }
}
