using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class BagEM_Distributor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DistributorId",
                table: "BagEMs",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BagEMs_DistributorId",
                table: "BagEMs",
                column: "DistributorId");

            migrationBuilder.AddForeignKey(
                name: "FK_BagEMs_User_DistributorId",
                table: "BagEMs",
                column: "DistributorId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BagEMs_User_DistributorId",
                table: "BagEMs");

            migrationBuilder.DropIndex(
                name: "IX_BagEMs_DistributorId",
                table: "BagEMs");

            migrationBuilder.DropColumn(
                name: "DistributorId",
                table: "BagEMs");
        }
    }
}
