using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Cubiq_updatepackage_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "OrderCubiqs");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Paquete",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Paquete");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "OrderCubiqs",
                nullable: false,
                defaultValue: 0);
        }
    }
}
