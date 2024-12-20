using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class minorAuth_DocumentAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentCity",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentCounty",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentState",
                table: "MinorAuthorizationOrders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentCity",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "DocumentCounty",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "DocumentState",
                table: "MinorAuthorizationOrders");
        }
    }
}
