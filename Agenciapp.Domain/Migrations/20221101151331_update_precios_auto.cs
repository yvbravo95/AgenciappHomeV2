using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class update_precios_auto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Minorista11",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista110",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista12",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista13",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista14",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista15",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista16",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista17",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista18",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista19",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista21",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista210",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista22",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista23",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista24",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista25",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista26",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista27",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista28",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista29",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "RentadoraId",
                table: "PreciosAutos",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreciosAutos_RentadoraId",
                table: "PreciosAutos",
                column: "RentadoraId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreciosAutos_Rentadora_RentadoraId",
                table: "PreciosAutos",
                column: "RentadoraId",
                principalTable: "Rentadora",
                principalColumn: "RentadoraId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreciosAutos_Rentadora_RentadoraId",
                table: "PreciosAutos");

            migrationBuilder.DropIndex(
                name: "IX_PreciosAutos_RentadoraId",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista11",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista110",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista12",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista13",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista14",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista15",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista16",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista17",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista18",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista19",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista21",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista210",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista22",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista23",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista24",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista25",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista26",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista27",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista28",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista29",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "RentadoraId",
                table: "PreciosAutos");
        }
    }
}
