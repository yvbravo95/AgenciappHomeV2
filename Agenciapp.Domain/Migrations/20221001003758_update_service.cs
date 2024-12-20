using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class update_service : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Package",
                table: "TipoServicios",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Servicios",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Package",
                table: "TipoServicios");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Servicios");
        }
    }
}
