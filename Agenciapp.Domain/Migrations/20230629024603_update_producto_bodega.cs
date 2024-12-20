using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class update_producto_bodega : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ProductosBodegas",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosBodegas_UserId",
                table: "ProductosBodegas",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosBodegas_User_UserId",
                table: "ProductosBodegas",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductosBodegas_User_UserId",
                table: "ProductosBodegas");


            migrationBuilder.DropIndex(
                name: "IX_ProductosBodegas_UserId",
                table: "ProductosBodegas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ProductosBodegas");
        }
    }
}
