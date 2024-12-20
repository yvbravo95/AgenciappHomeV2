using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class ProductCargaAm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductsCargaAm",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderCubiqId = table.Column<Guid>(nullable: false),
                    ProductoBodegaId = table.Column<Guid>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsCargaAm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsCargaAm_OrderCubiqs_OrderCubiqId",
                        column: x => x.OrderCubiqId,
                        principalTable: "OrderCubiqs",
                        principalColumn: "OrderCubiqId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductsCargaAm_ProductosBodegas_ProductoBodegaId",
                        column: x => x.ProductoBodegaId,
                        principalTable: "ProductosBodegas",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductsCargaAm_OrderCubiqId",
                table: "ProductsCargaAm",
                column: "OrderCubiqId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsCargaAm_ProductoBodegaId",
                table: "ProductsCargaAm",
                column: "ProductoBodegaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductsCargaAm");
        }
    }
}
