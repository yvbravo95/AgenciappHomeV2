using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderCubiq_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuiaAerea_GuideType",
                table: "OrderCubiqs");

            migrationBuilder.RenameColumn(
                name: "GuiaAerea_Type",
                table: "OrderCubiqs",
                newName: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OrderCubiqs",
                newName: "GuiaAerea_Type");

            migrationBuilder.AddColumn<string>(
                name: "GuiaAerea_GuideType",
                table: "OrderCubiqs",
                nullable: true);
        }
    }
}
