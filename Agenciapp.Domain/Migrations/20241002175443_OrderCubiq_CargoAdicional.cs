using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderCubiq_CargoAdicional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CargoAdicional",
                table: "OrderCubiqs",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CargoAdicionalDescription",
                table: "OrderCubiqs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoAdicional",
                table: "OrderCubiqs");

            migrationBuilder.DropColumn(
                name: "CargoAdicionalDescription",
                table: "OrderCubiqs");
        }
    }
}
