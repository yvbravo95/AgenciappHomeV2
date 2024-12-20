using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class BagItem_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecived",
                table: "BagItem");

            migrationBuilder.AddColumn<int>(
                name: "QtyReceived",
                table: "BagItem",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QtyReceived",
                table: "BagItem");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecived",
                table: "BagItem",
                nullable: false,
                defaultValue: false);
        }
    }
}
