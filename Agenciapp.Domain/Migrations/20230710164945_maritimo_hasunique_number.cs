using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class maritimo_hasunique_number : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "EnvioMaritimo",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnvioMaritimo_Number",
                table: "EnvioMaritimo",
                column: "Number",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EnvioMaritimo_Number",
                table: "EnvioMaritimo");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "EnvioMaritimo",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
