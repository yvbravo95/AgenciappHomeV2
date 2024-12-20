using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class add_agency_agency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "Aduana",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aduana_AgencyId",
                table: "Aduana",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aduana_Agency_AgencyId",
                table: "Aduana",
                column: "AgencyId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aduana_Agency_AgencyId",
                table: "Aduana");

            migrationBuilder.DropIndex(
                name: "IX_Aduana_AgencyId",
                table: "Aduana");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Aduana");
        }
    }
}
