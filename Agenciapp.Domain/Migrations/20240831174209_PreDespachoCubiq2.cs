using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class PreDespachoCubiq2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "PreDespachoCubiqs",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoCubiqs_Number",
                table: "PreDespachoCubiqs",
                column: "Number",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PreDespachoCubiqs_Number",
                table: "PreDespachoCubiqs");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "PreDespachoCubiqs",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
