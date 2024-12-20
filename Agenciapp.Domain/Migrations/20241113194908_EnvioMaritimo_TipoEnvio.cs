using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class EnvioMaritimo_TipoEnvio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoEnvio",
                table: "EnvioMaritimo",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transitaria",
                table: "EnvioMaritimo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoEnvio",
                table: "EnvioMaritimo");

            migrationBuilder.DropColumn(
                name: "Transitaria",
                table: "EnvioMaritimo");
        }
    }
}
