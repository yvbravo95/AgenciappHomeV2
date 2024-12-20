using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Pallet_GuiaId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GuiaId",
                table: "PalletCubiq",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PalletCubiq_GuiaId",
                table: "PalletCubiq",
                column: "GuiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PalletCubiq_GuiaAerea_GuiaId",
                table: "PalletCubiq",
                column: "GuiaId",
                principalTable: "GuiaAerea",
                principalColumn: "GuiaAereaId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PalletCubiq_GuiaAerea_GuiaId",
                table: "PalletCubiq");

            migrationBuilder.DropIndex(
                name: "IX_PalletCubiq_GuiaId",
                table: "PalletCubiq");

            migrationBuilder.DropColumn(
                name: "GuiaId",
                table: "PalletCubiq");
        }
    }
}
