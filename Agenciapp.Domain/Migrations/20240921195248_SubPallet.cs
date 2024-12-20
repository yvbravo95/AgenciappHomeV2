using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class SubPallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderCubiqs_PreDespachoCubiqs_PreDespachoId",
                table: "OrderCubiqs");

            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_ParleCubiq_ParleId",
                table: "Paquete");

            migrationBuilder.DropTable(
                name: "ParleCubiq");

            migrationBuilder.DropIndex(
                name: "IX_Paquete_ParleId",
                table: "Paquete");

            migrationBuilder.DropIndex(
                name: "IX_OrderCubiqs_PreDespachoId",
                table: "OrderCubiqs");

            migrationBuilder.DropColumn(
                name: "Exist",
                table: "PreDespachoItemCubiqs");

            migrationBuilder.DropColumn(
                name: "PreDespachoId",
                table: "OrderCubiqs");

            migrationBuilder.AddColumn<Guid>(
                name: "PreDespachoId",
                table: "Paquete",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubPalletId",
                table: "Paquete",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PalletCubiq",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdateAt = table.Column<DateTime>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletCubiq", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalletCubiq_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubPalletCubiq",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    PalletId = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPalletCubiq", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubPalletCubiq_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubPalletCubiq_PalletCubiq_PalletId",
                        column: x => x.PalletId,
                        principalTable: "PalletCubiq",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paquete_PreDespachoId",
                table: "Paquete",
                column: "PreDespachoId");

            migrationBuilder.CreateIndex(
                name: "IX_Paquete_SubPalletId",
                table: "Paquete",
                column: "SubPalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletCubiq_AgencyId",
                table: "PalletCubiq",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PalletCubiq_Number",
                table: "PalletCubiq",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubPalletCubiq_AgencyId",
                table: "SubPalletCubiq",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubPalletCubiq_PalletId",
                table: "SubPalletCubiq",
                column: "PalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_PreDespachoCubiqs_PreDespachoId",
                table: "Paquete",
                column: "PreDespachoId",
                principalTable: "PreDespachoCubiqs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_SubPalletCubiq_SubPalletId",
                table: "Paquete",
                column: "SubPalletId",
                principalTable: "SubPalletCubiq",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_PreDespachoCubiqs_PreDespachoId",
                table: "Paquete");

            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_SubPalletCubiq_SubPalletId",
                table: "Paquete");

            migrationBuilder.DropTable(
                name: "SubPalletCubiq");

            migrationBuilder.DropTable(
                name: "PalletCubiq");

            migrationBuilder.DropIndex(
                name: "IX_Paquete_PreDespachoId",
                table: "Paquete");

            migrationBuilder.DropIndex(
                name: "IX_Paquete_SubPalletId",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "PreDespachoId",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "SubPalletId",
                table: "Paquete");

            migrationBuilder.AddColumn<bool>(
                name: "Exist",
                table: "PreDespachoItemCubiqs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PreDespachoId",
                table: "OrderCubiqs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParleCubiq",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    UpdateAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParleCubiq", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParleCubiq_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paquete_ParleId",
                table: "Paquete",
                column: "ParleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCubiqs_PreDespachoId",
                table: "OrderCubiqs",
                column: "PreDespachoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParleCubiq_AgencyId",
                table: "ParleCubiq",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ParleCubiq_Number",
                table: "ParleCubiq",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderCubiqs_PreDespachoCubiqs_PreDespachoId",
                table: "OrderCubiqs",
                column: "PreDespachoId",
                principalTable: "PreDespachoCubiqs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_ParleCubiq_ParleId",
                table: "Paquete",
                column: "ParleId",
                principalTable: "ParleCubiq",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
