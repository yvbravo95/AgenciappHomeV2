using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class OrderContainer_Distributors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AfiliadoId",
                table: "OrderContainers",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistributorId",
                table: "OrderContainers",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RepartidorId",
                table: "OrderContainers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderContainers_AfiliadoId",
                table: "OrderContainers",
                column: "AfiliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderContainers_DistributorId",
                table: "OrderContainers",
                column: "DistributorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderContainers_RepartidorId",
                table: "OrderContainers",
                column: "RepartidorId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderContainers_Minoristas_AfiliadoId",
                table: "OrderContainers",
                column: "AfiliadoId",
                principalTable: "Minoristas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderContainers_User_DistributorId",
                table: "OrderContainers",
                column: "DistributorId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderContainers_User_RepartidorId",
                table: "OrderContainers",
                column: "RepartidorId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderContainers_Minoristas_AfiliadoId",
                table: "OrderContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderContainers_User_DistributorId",
                table: "OrderContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderContainers_User_RepartidorId",
                table: "OrderContainers");

            migrationBuilder.DropIndex(
                name: "IX_OrderContainers_AfiliadoId",
                table: "OrderContainers");

            migrationBuilder.DropIndex(
                name: "IX_OrderContainers_DistributorId",
                table: "OrderContainers");

            migrationBuilder.DropIndex(
                name: "IX_OrderContainers_RepartidorId",
                table: "OrderContainers");

            migrationBuilder.DropColumn(
                name: "AfiliadoId",
                table: "OrderContainers");

            migrationBuilder.DropColumn(
                name: "DistributorId",
                table: "OrderContainers");

            migrationBuilder.DropColumn(
                name: "RepartidorId",
                table: "OrderContainers");
        }
    }
}
