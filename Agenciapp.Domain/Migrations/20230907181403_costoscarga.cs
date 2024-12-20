using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class costoscarga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostosCubiq");

            migrationBuilder.CreateTable(
                name: "CostosCarga",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: true),
                    Value = table.Column<decimal>(nullable: false),
                    Value2 = table.Column<decimal>(nullable: false),
                    ZonaId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostosCarga", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostosCarga_Zona_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "Zona",
                        principalColumn: "ZonaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostosCarga_ZonaId",
                table: "CostosCarga",
                column: "ZonaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostosCarga");

            migrationBuilder.CreateTable(
                name: "CostosCubiq",
                columns: table => new
                {
                    CostoCubiqId = table.Column<Guid>(nullable: false),
                    Value = table.Column<decimal>(nullable: false),
                    Value2 = table.Column<decimal>(nullable: false),
                    ZonaId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostosCubiq", x => x.CostoCubiqId);
                    table.ForeignKey(
                        name: "FK_CostosCubiq_Zona_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "Zona",
                        principalColumn: "ZonaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostosCubiq_ZonaId",
                table: "CostosCubiq",
                column: "ZonaId");
        }
    }
}
