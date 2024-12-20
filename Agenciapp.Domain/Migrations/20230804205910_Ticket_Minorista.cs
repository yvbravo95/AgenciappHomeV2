using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Ticket_Minorista : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MinoristaId",
                table: "Ticket",
                nullable: true);

            /*migrationBuilder.CreateTable(
                name: "ServicesByPayNotBilled",
                columns: table => new
                {
                    ServiciosxPagarId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    Tipo = table.Column<string>(nullable: true),
                    IsPaymentProductShipping = table.Column<bool>(nullable: false),
                    ImporteAPagar = table.Column<decimal>(nullable: false),
                    WholesalerName = table.Column<string>(nullable: true),
                    IdWholesaler = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesByPayNotBilled", x => x.ServiciosxPagarId);
                });*/

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_MinoristaId",
                table: "Ticket",
                column: "MinoristaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Minoristas_MinoristaId",
                table: "Ticket",
                column: "MinoristaId",
                principalTable: "Minoristas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Minoristas_MinoristaId",
                table: "Ticket");

           /* migrationBuilder.DropTable(
                name: "ServicesByPayNotBilled");*/

            migrationBuilder.DropIndex(
                name: "IX_Ticket_MinoristaId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MinoristaId",
                table: "Ticket");
        }
    }
}
