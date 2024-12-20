using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class PreDespachoCubiq_AgencyTransferida : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AgencyId",
                table: "PreDespachoCubiqs",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AgencyTransferidaId",
                table: "PreDespachoCubiqs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoCubiqs_AgencyTransferidaId",
                table: "PreDespachoCubiqs",
                column: "AgencyTransferidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreDespachoCubiqs_Agency_AgencyTransferidaId",
                table: "PreDespachoCubiqs",
                column: "AgencyTransferidaId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreDespachoCubiqs_Agency_AgencyTransferidaId",
                table: "PreDespachoCubiqs");

            migrationBuilder.DropIndex(
                name: "IX_PreDespachoCubiqs_AgencyTransferidaId",
                table: "PreDespachoCubiqs");

            migrationBuilder.DropColumn(
                name: "AgencyTransferidaId",
                table: "PreDespachoCubiqs");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgencyId",
                table: "PreDespachoCubiqs",
                nullable: true,
                oldClrType: typeof(Guid));
        }
    }
}
