using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderCarga_Aduana : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ValorAduanalId",
                table: "ValorAduanalItem",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "ValorAduanalItem",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "AduanaId",
                table: "ValorAduanalItem",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValorAduanalItem_AduanaId",
                table: "ValorAduanalItem",
                column: "AduanaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ValorAduanalItem_Aduana_AduanaId",
                table: "ValorAduanalItem",
                column: "AduanaId",
                principalTable: "Aduana",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ValorAduanalItem_Aduana_AduanaId",
                table: "ValorAduanalItem");

            migrationBuilder.DropIndex(
                name: "IX_ValorAduanalItem_AduanaId",
                table: "ValorAduanalItem");

            migrationBuilder.DropColumn(
                name: "AduanaId",
                table: "ValorAduanalItem");

            migrationBuilder.AlterColumn<Guid>(
                name: "ValorAduanalId",
                table: "ValorAduanalItem",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "ValorAduanalItem",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
