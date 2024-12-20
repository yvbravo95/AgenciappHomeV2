using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class shipping_distributor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoDespachoDistributor",
                table: "Shipping",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrincipalDistributorId",
                table: "Shipping",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipping_PrincipalDistributorId",
                table: "Shipping",
                column: "PrincipalDistributorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipping_User_PrincipalDistributorId",
                table: "Shipping",
                column: "PrincipalDistributorId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipping_User_PrincipalDistributorId",
                table: "Shipping");

            migrationBuilder.DropIndex(
                name: "IX_Shipping_PrincipalDistributorId",
                table: "Shipping");

            migrationBuilder.DropColumn(
                name: "NoDespachoDistributor",
                table: "Shipping");

            migrationBuilder.DropColumn(
                name: "PrincipalDistributorId",
                table: "Shipping");
        }
    }
}
