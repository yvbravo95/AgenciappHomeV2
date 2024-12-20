using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class pricesbyprovince_hmpaquetes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<int>(
                name: "PhysicalBags",
                table: "Bag",
                nullable: false,
                defaultValue: 0);*/

            migrationBuilder.CreateTable(
                name: "HMpaquetesPriceByProvinces",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    RetailAgencyId = table.Column<Guid>(nullable: false),
                    ProvinceId = table.Column<Guid>(nullable: false),
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    Price = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HMpaquetesPriceByProvinces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HMpaquetesPriceByProvinces_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HMpaquetesPriceByProvinces_Municipio_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HMpaquetesPriceByProvinces_Provincia_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provincia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HMpaquetesPriceByProvinces_Agency_RetailAgencyId",
                        column: x => x.RetailAgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HMpaquetesPriceByProvinces_AgencyId",
                table: "HMpaquetesPriceByProvinces",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_HMpaquetesPriceByProvinces_MunicipalityId",
                table: "HMpaquetesPriceByProvinces",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_HMpaquetesPriceByProvinces_ProvinceId",
                table: "HMpaquetesPriceByProvinces",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_HMpaquetesPriceByProvinces_RetailAgencyId",
                table: "HMpaquetesPriceByProvinces",
                column: "RetailAgencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HMpaquetesPriceByProvinces");

            /*migrationBuilder.DropColumn(
                name: "PhysicalBags",
                table: "Bag");*/
        }
    }
}
