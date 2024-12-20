using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class pricebyprovinceminorista : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "RetailAgencyId",
                table: "HMpaquetesPriceByProvinces",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "RetailId",
                table: "HMpaquetesPriceByProvinces",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HMpaquetesPriceByProvinces_RetailId",
                table: "HMpaquetesPriceByProvinces",
                column: "RetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_HMpaquetesPriceByProvinces_Minoristas_RetailId",
                table: "HMpaquetesPriceByProvinces",
                column: "RetailId",
                principalTable: "Minoristas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HMpaquetesPriceByProvinces_Minoristas_RetailId",
                table: "HMpaquetesPriceByProvinces");

            migrationBuilder.DropIndex(
                name: "IX_HMpaquetesPriceByProvinces_RetailId",
                table: "HMpaquetesPriceByProvinces");

            migrationBuilder.DropColumn(
                name: "RetailId",
                table: "HMpaquetesPriceByProvinces");

            migrationBuilder.AlterColumn<Guid>(
                name: "RetailAgencyId",
                table: "HMpaquetesPriceByProvinces",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
