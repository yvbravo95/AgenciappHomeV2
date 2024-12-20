using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class sxp_index_number : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.DropForeignKey(
                name: "FK_AuxReservas_Rentadora_RentadoraId",
                table: "AuxReservas");*/

            migrationBuilder.AlterColumn<string>(
                name: "NoServicio",
                table: "ServiciosxPagar",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosxPagar_NoServicio",
                table: "ServiciosxPagar",
                column: "NoServicio");

           /* migrationBuilder.AddForeignKey(
                name: "FK_AuxReservas_Rentadora_RentadoraId",
                table: "AuxReservas",
                column: "RentadoraId",
                principalTable: "Rentadora",
                principalColumn: "RentadoraId",
                onDelete: ReferentialAction.Restrict);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropForeignKey(
                name: "FK_AuxReservas_Rentadora_RentadoraId",
                table: "AuxReservas");*/

            migrationBuilder.DropIndex(
                name: "IX_ServiciosxPagar_NoServicio",
                table: "ServiciosxPagar");

            migrationBuilder.AlterColumn<string>(
                name: "NoServicio",
                table: "ServiciosxPagar",
                nullable: true,
                oldClrType: typeof(string));

           /* migrationBuilder.AddForeignKey(
                name: "FK_AuxReservas_Rentadora_RentadoraId",
                table: "AuxReservas",
                column: "RentadoraId",
                principalTable: "Rentadora",
                principalColumn: "RentadoraId",
                onDelete: ReferentialAction.Cascade);*/
        }
    }
}
