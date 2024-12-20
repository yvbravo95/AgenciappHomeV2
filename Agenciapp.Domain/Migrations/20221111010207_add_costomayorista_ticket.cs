using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class add_costomayorista_ticket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoMayorista",
                table: "Ticket",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoMayorista",
                table: "Ticket");
        }
    }
}
