using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class CostByProvince_Cost234 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost2",
                table: "CostByProvinces",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost3",
                table: "CostByProvinces",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost4",
                table: "CostByProvinces",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost2",
                table: "CostByProvinces");

            migrationBuilder.DropColumn(
                name: "Cost3",
                table: "CostByProvinces");

            migrationBuilder.DropColumn(
                name: "Cost4",
                table: "CostByProvinces");
        }
    }
}
