using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Paquete_GuiaAerea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParleCubiq_GuiaAerea_GuiaAereaId",
                table: "ParleCubiq");

            migrationBuilder.DropIndex(
                name: "IX_ParleCubiq_GuiaAereaId",
                table: "ParleCubiq");

            migrationBuilder.DropColumn(
                name: "GuiaAereaId",
                table: "ParleCubiq");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GuiaAereaId",
                table: "ParleCubiq",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParleCubiq_GuiaAereaId",
                table: "ParleCubiq",
                column: "GuiaAereaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParleCubiq_GuiaAerea_GuiaAereaId",
                table: "ParleCubiq",
                column: "GuiaAereaId",
                principalTable: "GuiaAerea",
                principalColumn: "GuiaAereaId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
