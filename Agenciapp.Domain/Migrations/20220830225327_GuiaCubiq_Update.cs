using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class GuiaCubiq_Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostAv3",
                table: "GuiaAerea",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostAv4",
                table: "GuiaAerea",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAv3",
                table: "GuiaAerea",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAv4",
                table: "GuiaAerea",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostAv3",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "CostAv4",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "PriceAv3",
                table: "GuiaAerea");

            migrationBuilder.DropColumn(
                name: "PriceAv4",
                table: "GuiaAerea");
        }
    }
}
