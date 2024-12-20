using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class ticket_agencyTransferida : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgencyTransferidaId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_AgencyTransferidaId",
                table: "Ticket",
                column: "AgencyTransferidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Agency_AgencyTransferidaId",
                table: "Ticket",
                column: "AgencyTransferidaId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Agency_AgencyTransferidaId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_AgencyTransferidaId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "AgencyTransferidaId",
                table: "Ticket");
        }
    }
}
