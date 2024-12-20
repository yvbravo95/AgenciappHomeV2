using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class MercadoUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mercado_User_UserId",
                table: "Mercado");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Mercado",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CanceledDate",
                table: "Mercado",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MercadoId",
                table: "Logs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_MercadoId",
                table: "Logs",
                column: "MercadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Mercado_MercadoId",
                table: "Logs",
                column: "MercadoId",
                principalTable: "Mercado",
                principalColumn: "MercadoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mercado_User_UserId",
                table: "Mercado",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Mercado_MercadoId",
                table: "Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Mercado_User_UserId",
                table: "Mercado");

            migrationBuilder.DropIndex(
                name: "IX_Logs_MercadoId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "CanceledDate",
                table: "Mercado");

            migrationBuilder.DropColumn(
                name: "MercadoId",
                table: "Logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Mercado",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Mercado_User_UserId",
                table: "Mercado",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
