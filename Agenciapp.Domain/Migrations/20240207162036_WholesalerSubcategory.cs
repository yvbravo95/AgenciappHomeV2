using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class WholesalerSubcategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Subcategory",
                table: "Wholesalers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subcategory",
                table: "Wholesalers");
        }
    }
}
