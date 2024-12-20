using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Passport_OrderNumber_Unique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Passport",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Passport_OrderNumber",
                table: "Passport",
                column: "OrderNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Passport_OrderNumber",
                table: "Passport");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Passport",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
