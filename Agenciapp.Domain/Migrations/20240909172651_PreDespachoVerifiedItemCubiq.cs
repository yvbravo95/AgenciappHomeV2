using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class PreDespachoVerifiedItemCubiq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "PreDespachoItemCubiqs",
                newName: "Exist");

            migrationBuilder.AddColumn<Guid>(
                name: "PaqueteId",
                table: "PreDespachoItemCubiqs",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PaqueteNumber",
                table: "PreDespachoItemCubiqs",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PreDespachoId",
                table: "OrderCubiqs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoItemCubiqs_PaqueteId",
                table: "PreDespachoItemCubiqs",
                column: "PaqueteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCubiqs_PreDespachoId",
                table: "OrderCubiqs",
                column: "PreDespachoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderCubiqs_PreDespachoCubiqs_PreDespachoId",
                table: "OrderCubiqs",
                column: "PreDespachoId",
                principalTable: "PreDespachoCubiqs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PreDespachoItemCubiqs_Paquete_PaqueteId",
                table: "PreDespachoItemCubiqs",
                column: "PaqueteId",
                principalTable: "Paquete",
                principalColumn: "PaqueteId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderCubiqs_PreDespachoCubiqs_PreDespachoId",
                table: "OrderCubiqs");

            migrationBuilder.DropForeignKey(
                name: "FK_PreDespachoItemCubiqs_Paquete_PaqueteId",
                table: "PreDespachoItemCubiqs");

            migrationBuilder.DropIndex(
                name: "IX_PreDespachoItemCubiqs_PaqueteId",
                table: "PreDespachoItemCubiqs");

            migrationBuilder.DropIndex(
                name: "IX_OrderCubiqs_PreDespachoId",
                table: "OrderCubiqs");

            migrationBuilder.DropColumn(
                name: "PaqueteId",
                table: "PreDespachoItemCubiqs");

            migrationBuilder.DropColumn(
                name: "PaqueteNumber",
                table: "PreDespachoItemCubiqs");

            migrationBuilder.DropColumn(
                name: "PreDespachoId",
                table: "OrderCubiqs");

            migrationBuilder.RenameColumn(
                name: "Exist",
                table: "PreDespachoItemCubiqs",
                newName: "IsVerified");
        }
    }
}
