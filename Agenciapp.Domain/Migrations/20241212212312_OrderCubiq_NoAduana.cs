using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderCubiq_NoAduana : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NoAduana",
                table: "OrderCubiqs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecogidaAlmacen",
                table: "OrderCubiqs",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoAduana",
                table: "OrderCubiqs");

            migrationBuilder.DropColumn(
                name: "RecogidaAlmacen",
                table: "OrderCubiqs");
        }
    }
}
