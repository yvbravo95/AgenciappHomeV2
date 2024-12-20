using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class add_rentadora_ticket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RentadoraId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_RentadoraId",
                table: "Ticket",
                column: "RentadoraId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Rentadora_RentadoraId",
                table: "Ticket",
                column: "RentadoraId",
                principalTable: "Rentadora",
                principalColumn: "RentadoraId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Rentadora_RentadoraId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_RentadoraId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "RentadoraId",
                table: "Ticket");
        }
    }
}
