using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class BagEM_wharehouse_location : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "WharehouseLocation",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseLocation",
                table: "BagEMs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WharehouseLocation",
                table: "User");

            migrationBuilder.DropColumn(
                name: "WarehouseLocation",
                table: "BagEMs");

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
    }
}
