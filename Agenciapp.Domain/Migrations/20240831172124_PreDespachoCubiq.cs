using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class PreDespachoCubiq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreDespachoCubiqs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreDespachoCubiqs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDespachoCubiqs_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreDespachoItemCubiqs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PreDespachoId = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: false),
                    OrderNumber = table.Column<string>(nullable: false),
                    IsVerified = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreDespachoItemCubiqs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDespachoItemCubiqs_OrderCubiqs_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderCubiqs",
                        principalColumn: "OrderCubiqId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreDespachoItemCubiqs_PreDespachoCubiqs_PreDespachoId",
                        column: x => x.PreDespachoId,
                        principalTable: "PreDespachoCubiqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoCubiqs_UserId",
                table: "PreDespachoCubiqs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoItemCubiqs_OrderId",
                table: "PreDespachoItemCubiqs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDespachoItemCubiqs_PreDespachoId",
                table: "PreDespachoItemCubiqs",
                column: "PreDespachoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreDespachoItemCubiqs");

            migrationBuilder.DropTable(
                name: "PreDespachoCubiqs");
        }
    }
}
