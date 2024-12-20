using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class PreDespacho_Agency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "PreDespachoCubiqs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoCubiqs_AgencyId",
                table: "PreDespachoCubiqs",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreDespachoCubiqs_Agency_AgencyId",
                table: "PreDespachoCubiqs",
                column: "AgencyId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreDespachoCubiqs_Agency_AgencyId",
                table: "PreDespachoCubiqs");

            migrationBuilder.DropIndex(
                name: "IX_PreDespachoCubiqs_AgencyId",
                table: "PreDespachoCubiqs");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "PreDespachoCubiqs");
        }
    }
}
