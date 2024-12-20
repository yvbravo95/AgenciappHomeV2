using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class uppdate_preciosAuto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo1",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo10",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo2",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo3",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo4",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo5",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo6",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo7",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo8",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeFijo9",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios10",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios7",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios8",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Precios9",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro1",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro10",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro2",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro3",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro4",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro5",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro6",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro7",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro8",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Seguro9",
                table: "PreciosAutos",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeFijo1",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo10",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo2",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo3",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo4",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo5",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo6",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo7",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo8",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "FeeFijo9",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios10",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios7",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios8",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Precios9",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro1",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro10",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro2",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro3",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro4",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro5",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro6",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro7",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro8",
                table: "PreciosAutos");

            migrationBuilder.DropColumn(
                name: "Seguro9",
                table: "PreciosAutos");
        }
    }
}
