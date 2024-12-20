using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class add_category_SUV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo13",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista113",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Minorista213",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios13",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro13",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeFijo13",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista113",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Minorista213",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios13",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro13",
                table: "PreciosAutos");
        }
    }
}
