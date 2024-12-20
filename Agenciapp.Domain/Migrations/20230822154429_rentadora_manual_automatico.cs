using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class rentadora_manual_automatico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo11",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo12",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista111",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista112",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista211",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista212",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios11",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios12",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro11",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro12",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeFijo11",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo12",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista111",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista112",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista211",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista212",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios11",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios12",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro11",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro12",
                table: "PreciosAutos");
        }
    }
}
