using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Pallet_Revert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_SubPalletCubiq_SubPalletId",
                table: "Paquete");

            migrationBuilder.DropTable(
                name: "SubPalletCubiq");

            migrationBuilder.RenameColumn(
                name: "SubPalletId",
                table: "Paquete",
                newName: "PalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Paquete_SubPalletId",
                table: "Paquete",
                newName: "IX_Paquete_PalletId");

            migrationBuilder.AddColumn<int>(
                name: "QtyPallets",
                table: "PalletCubiq",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_PalletCubiq_PalletId",
                table: "Paquete",
                column: "PalletId",
                principalTable: "PalletCubiq",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paquete_PalletCubiq_PalletId",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "QtyPallets",
                table: "PalletCubiq");

            migrationBuilder.RenameColumn(
                name: "PalletId",
                table: "Paquete",
                newName: "SubPalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Paquete_PalletId",
                table: "Paquete",
                newName: "IX_Paquete_SubPalletId");

            migrationBuilder.CreateTable(
                name: "SubPalletCubiq",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(nullable: false),
                    PalletId = table.Column<Guid>(nullable: false)
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
                name: "IX_SubPalletCubiq_AgencyId",
                table: "SubPalletCubiq",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubPalletCubiq_PalletId",
                table: "SubPalletCubiq",
                column: "PalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Paquete_SubPalletCubiq_SubPalletId",
                table: "Paquete",
                column: "SubPalletId",
                principalTable: "SubPalletCubiq",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
