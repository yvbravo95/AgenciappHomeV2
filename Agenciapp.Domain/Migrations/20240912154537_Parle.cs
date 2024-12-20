using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Parle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderCubiqs_ContainerGuiaCubiq_ContainerId",
                table: "OrderCubiqs");

            migrationBuilder.DropTable(
                name: "ContainerGuiaCubiq");

            migrationBuilder.DropIndex(
                name: "IX_OrderCubiqs_ContainerId",
                table: "OrderCubiqs");

            migrationBuilder.DropColumn(
                name: "ContainerId",
                table: "OrderCubiqs");

            migrationBuilder.AddColumn<Guid>(
                name: "ParleId",
                table: "Paquete",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParleCubiq",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdateAt = table.Column<DateTime>(nullable: false),
                    GuiaAereaId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParleCubiq", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParleCubiq_GuiaAerea_GuiaAereaId",
                        column: x => x.GuiaAereaId,
                        principalTable: "GuiaAerea",
                        principalColumn: "GuiaAereaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paquete_ParleId",
                table: "Paquete",
                column: "ParleId");

            migrationBuilder.CreateIndex(
                name: "IX_ParleCubiq_GuiaAereaId",
                table: "ParleCubiq",
                column: "GuiaAereaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParleCubiq_Number",
                table: "ParleCubiq",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_ParleCubiq_ParleId",
                table: "Paquete",
                column: "ParleId",
                principalTable: "ParleCubiq",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_ParleCubiq_ParleId",
                table: "Paquete");

            migrationBuilder.DropTable(
                name: "ParleCubiq");

            migrationBuilder.DropIndex(
                name: "IX_Paquete_ParleId",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "ParleId",
                table: "Paquete");

            migrationBuilder.AddColumn<Guid>(
                name: "ContainerId",
                table: "OrderCubiqs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContainerGuiaCubiq",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    GuiaAereaId = table.Column<Guid>(nullable: true),
                    Number = table.Column<string>(nullable: true),
                    UpdateAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerGuiaCubiq", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerGuiaCubiq_GuiaAerea_GuiaAereaId",
                        column: x => x.GuiaAereaId,
                        principalTable: "GuiaAerea",
                        principalColumn: "GuiaAereaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderCubiqs_ContainerId",
                table: "OrderCubiqs",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerGuiaCubiq_GuiaAereaId",
                table: "ContainerGuiaCubiq",
                column: "GuiaAereaId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderCubiqs_ContainerGuiaCubiq_ContainerId",
                table: "OrderCubiqs",
                column: "ContainerId",
                principalTable: "ContainerGuiaCubiq",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
