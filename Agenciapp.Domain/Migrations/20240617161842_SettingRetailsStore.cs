using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class SettingRetailsStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettingRetailsStore",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    FeeWholesaler = table.Column<decimal>(nullable: false),
                    FeeRetail = table.Column<decimal>(nullable: false),
                    ServiceCost = table.Column<decimal>(nullable: false),
                    StoreType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingRetailsStore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettingRetailsStore_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettingRetailsStore_AgencyId",
                table: "SettingRetailsStore",
                column: "AgencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettingRetailsStore");
        }
    }
}
