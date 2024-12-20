using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Paquete_Aduana : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AduanaDescription",
                table: "Paquete",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AduanaId",
                table: "Paquete",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paquete_AduanaId",
                table: "Paquete",
                column: "AduanaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_Aduana_AduanaId",
                table: "Paquete",
                column: "AduanaId",
                principalTable: "Aduana",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_Aduana_AduanaId",
                table: "Paquete");

            migrationBuilder.DropIndex(
                name: "IX_Paquete_AduanaId",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "AduanaDescription",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "AduanaId",
                table: "Paquete");
        }
    }
}
