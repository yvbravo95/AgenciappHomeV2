using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class add_wholesaler_cred : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassApiWholesaler",
                table: "Agency",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserApiWholesaler",
                table: "Agency",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassApiWholesaler",
                table: "Agency");

            migrationBuilder.DropColumn(
                name: "UserApiWholesaler",
                table: "Agency");
        }
    }
}
