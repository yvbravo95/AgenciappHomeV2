using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderCubiq_Number_Unique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "OrderCubiqs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderCubiqs_Number",
                table: "OrderCubiqs",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderCubiqs_Number",
                table: "OrderCubiqs");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "OrderCubiqs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
