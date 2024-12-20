using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class EnbioMaritimoBag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BagEM",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EnvioMaritimoId = table.Column<Guid>(nullable: false),
                    CreateAt = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Weight = table.Column<decimal>(nullable: false),
                    Price = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BagEM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BagEM_EnvioMaritimo_EnvioMaritimoId",
                        column: x => x.EnvioMaritimoId,
                        principalTable: "EnvioMaritimo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BagEM_EnvioMaritimoId",
                table: "BagEM",
                column: "EnvioMaritimoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BagEM");
        }
    }
}
