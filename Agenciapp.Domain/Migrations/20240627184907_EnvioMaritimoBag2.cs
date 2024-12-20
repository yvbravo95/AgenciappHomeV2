using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class EnvioMaritimoBag2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BagEM_EnvioMaritimo_EnvioMaritimoId",
                table: "BagEM");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BagEM",
                table: "BagEM");

            migrationBuilder.RenameTable(
                name: "BagEM",
                newName: "BagEMs");

            migrationBuilder.RenameIndex(
                name: "IX_BagEM_EnvioMaritimoId",
                table: "BagEMs",
                newName: "IX_BagEMs_EnvioMaritimoId");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "BagEMs",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BagEMs",
                table: "BagEMs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BagEMs_Number",
                table: "BagEMs",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BagEMs_EnvioMaritimo_EnvioMaritimoId",
                table: "BagEMs",
                column: "EnvioMaritimoId",
                principalTable: "EnvioMaritimo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BagEMs_EnvioMaritimo_EnvioMaritimoId",
                table: "BagEMs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BagEMs",
                table: "BagEMs");

            migrationBuilder.DropIndex(
                name: "IX_BagEMs_Number",
                table: "BagEMs");

            migrationBuilder.RenameTable(
                name: "BagEMs",
                newName: "BagEM");

            migrationBuilder.RenameIndex(
                name: "IX_BagEMs_EnvioMaritimoId",
                table: "BagEM",
                newName: "IX_BagEM_EnvioMaritimoId");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "BagEM",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BagEM",
                table: "BagEM",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BagEM_EnvioMaritimo_EnvioMaritimoId",
                table: "BagEM",
                column: "EnvioMaritimoId",
                principalTable: "EnvioMaritimo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
