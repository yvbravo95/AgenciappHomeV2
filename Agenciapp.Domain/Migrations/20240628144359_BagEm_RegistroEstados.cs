using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class BagEm_RegistroEstados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BagEMId",
                table: "RegistroEstado",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroEstado_BagEMId",
                table: "RegistroEstado",
                column: "BagEMId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistroEstado_BagEMs_BagEMId",
                table: "RegistroEstado",
                column: "BagEMId",
                principalTable: "BagEMs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistroEstado_BagEMs_BagEMId",
                table: "RegistroEstado");

            migrationBuilder.DropIndex(
                name: "IX_RegistroEstado_BagEMId",
                table: "RegistroEstado");

            migrationBuilder.DropColumn(
                name: "BagEMId",
                table: "RegistroEstado");
        }
    }
}
