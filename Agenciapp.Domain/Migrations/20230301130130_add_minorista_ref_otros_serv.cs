using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class add_minorista_ref_otros_serv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MinoristaId",
                table: "Servicios",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_MinoristaId",
                table: "Servicios",
                column: "MinoristaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servicios_MinoristaOtrosServs_MinoristaId",
                table: "Servicios",
                column: "MinoristaId",
                principalTable: "MinoristaOtrosServs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servicios_MinoristaOtrosServs_MinoristaId",
                table: "Servicios");

            migrationBuilder.DropIndex(
                name: "IX_Servicios_MinoristaId",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "MinoristaId",
                table: "Servicios");
        }
    }
}
