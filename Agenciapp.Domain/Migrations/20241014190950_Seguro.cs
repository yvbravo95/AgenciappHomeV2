using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Seguro : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceValue",
                table: "OrderCubiqs",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CargaAMSeguros",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargaAMSeguros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargaAMSeguros_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargaAMSeguroItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CargaAMSeguroId = table.Column<Guid>(nullable: false),
                    CargaId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargaAMSeguroItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargaAMSeguroItems_CargaAMSeguros_CargaAMSeguroId",
                        column: x => x.CargaAMSeguroId,
                        principalTable: "CargaAMSeguros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargaAMSeguroItems_OrderCubiqs_CargaId",
                        column: x => x.CargaId,
                        principalTable: "OrderCubiqs",
                        principalColumn: "OrderCubiqId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargaAMSeguroItems_CargaAMSeguroId",
                table: "CargaAMSeguroItems",
                column: "CargaAMSeguroId");

            migrationBuilder.CreateIndex(
                name: "IX_CargaAMSeguroItems_CargaId",
                table: "CargaAMSeguroItems",
                column: "CargaId");

            migrationBuilder.CreateIndex(
                name: "IX_CargaAMSeguros_AgencyId",
                table: "CargaAMSeguros",
                column: "AgencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargaAMSeguroItems");

            migrationBuilder.DropTable(
                name: "CargaAMSeguros");

            migrationBuilder.DropColumn(
                name: "InsuranceValue",
                table: "OrderCubiqs");
        }
    }
}
