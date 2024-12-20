using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Guia_DatosContenedor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CAT",
                table: "GuiaAerea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRecogida",
                table: "GuiaAerea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaViaje",
                table: "GuiaAerea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SEAL",
                table: "GuiaAerea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SMLU",
                table: "GuiaAerea",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CAT",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "FechaRecogida",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "FechaViaje",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "SEAL",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "SMLU",
                table: "GuiaAerea");
        }
    }
}
