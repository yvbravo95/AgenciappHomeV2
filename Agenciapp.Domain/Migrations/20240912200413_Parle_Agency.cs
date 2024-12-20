using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Parle_Agency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "ParleCubiq",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParleCubiq_AgencyId",
                table: "ParleCubiq",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParleCubiq_Agency_AgencyId",
                table: "ParleCubiq",
                column: "AgencyId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParleCubiq_Agency_AgencyId",
                table: "ParleCubiq");

            migrationBuilder.DropIndex(
                name: "IX_ParleCubiq_AgencyId",
                table: "ParleCubiq");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "ParleCubiq");
        }
    }
}
