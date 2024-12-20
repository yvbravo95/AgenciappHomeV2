using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Paquete_DropPalletId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParleId",
                table: "Paquete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParleId",
                table: "Paquete",
                nullable: true);
        }
    }
}
