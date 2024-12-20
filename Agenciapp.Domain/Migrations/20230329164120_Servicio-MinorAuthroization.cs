using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class ServicioMinorAuthroization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MinorAuthorizationOrders_Agency_AgencyId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_MinorAuthorizationOrders_Client_ClientId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_MinorAuthorizationOrders_User_UserId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropIndex(
                name: "IX_MinorAuthorizationOrders_AgencyId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropIndex(
                name: "IX_MinorAuthorizationOrders_ClientId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropIndex(
                name: "IX_MinorAuthorizationOrders_OrderNumber",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropIndex(
                name: "IX_MinorAuthorizationOrders_UserId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "Child",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MinorAuthorizationOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "MinorAuthorizationId",
                table: "Servicios",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_MinorAuthorizationId",
                table: "Servicios",
                column: "MinorAuthorizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servicios_MinorAuthorizationOrders_MinorAuthorizationId",
                table: "Servicios",
                column: "MinorAuthorizationId",
                principalTable: "MinorAuthorizationOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servicios_MinorAuthorizationOrders_MinorAuthorizationId",
                table: "Servicios");

            migrationBuilder.DropIndex(
                name: "IX_Servicios_MinorAuthorizationId",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "MinorAuthorizationId",
                table: "Servicios");

            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Child",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_AgencyId",
                table: "MinorAuthorizationOrders",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_ClientId",
                table: "MinorAuthorizationOrders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_OrderNumber",
                table: "MinorAuthorizationOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_UserId",
                table: "MinorAuthorizationOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MinorAuthorizationOrders_Agency_AgencyId",
                table: "MinorAuthorizationOrders",
                column: "AgencyId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MinorAuthorizationOrders_Client_ClientId",
                table: "MinorAuthorizationOrders",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MinorAuthorizationOrders_User_UserId",
                table: "MinorAuthorizationOrders",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
