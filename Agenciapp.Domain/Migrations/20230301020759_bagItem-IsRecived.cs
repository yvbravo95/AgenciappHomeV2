using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class bagItemIsRecived : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecived",
                table: "Bag");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecived",
                table: "BagItem",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecived",
                table: "BagItem");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecived",
                table: "Bag",
                nullable: false,
                defaultValue: false);
        }
    }
}
