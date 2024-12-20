using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderContainer_RegistroEstados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderContainerId",
                table: "RegistroEstado",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroEstado_OrderContainerId",
                table: "RegistroEstado",
                column: "OrderContainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroEstado_OrderContainers_OrderContainerId",
                table: "RegistroEstado",
                column: "OrderContainerId",
                principalTable: "OrderContainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistroEstado_OrderContainers_OrderContainerId",
                table: "RegistroEstado");

            migrationBuilder.DropIndex(
                name: "IX_RegistroEstado_OrderContainerId",
                table: "RegistroEstado");

            migrationBuilder.DropColumn(
                name: "OrderContainerId",
                table: "RegistroEstado");
        }
    }
}
