using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderCubiq_GuiaAereaType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuiaAerea_GuideType",
                table: "OrderCubiqs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuiaAerea_Type",
                table: "OrderCubiqs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuiaAerea_GuideType",
                table: "OrderCubiqs");

            migrationBuilder.DropColumn(
                name: "GuiaAerea_Type",
                table: "OrderCubiqs");
        }
    }
}
