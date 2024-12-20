using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class MinorAuthorizationOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinorAuthorizationOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ClientId = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    OrderNumber = table.Column<string>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    MunicipalityOfBirth = table.Column<string>(nullable: true),
                    ProvinceOfBirth = table.Column<string>(nullable: true),
                    Grandfather = table.Column<string>(nullable: true),
                    Grandmother = table.Column<string>(nullable: true),
                    Ocuppation = table.Column<string>(nullable: true),
                    AddressStreet = table.Column<string>(nullable: true),
                    AddressNumber = table.Column<string>(nullable: true),
                    AddressBetweenStreet1 = table.Column<string>(nullable: true),
                    AddressBetweenStreet2 = table.Column<string>(nullable: true),
                    AddressMunicipality = table.Column<string>(nullable: true),
                    AddressProvince = table.Column<string>(nullable: true),
                    IndentityNumber = table.Column<string>(nullable: true),
                    PassportNumber = table.Column<string>(nullable: true),
                    PassportExpireDate = table.Column<string>(nullable: true),
                    ChildFullName = table.Column<string>(nullable: true),
                    Child = table.Column<string>(nullable: true),
                    ChildMunicipalityOfBirth = table.Column<string>(nullable: true),
                    ChildProvinceOfBirth = table.Column<string>(nullable: true),
                    ChildAge = table.Column<string>(nullable: true),
                    ChildAddressStreet = table.Column<string>(nullable: true),
                    ChildAddressNumber = table.Column<string>(nullable: true),
                    ChildAddressBetweenStreet1 = table.Column<string>(nullable: true),
                    ChildAddressBetweenStreet2 = table.Column<string>(nullable: true),
                    ChildAddressMunicipality = table.Column<string>(nullable: true),
                    ChildAddressProvince = table.Column<string>(nullable: true),
                    ChildIdentityNumber = table.Column<string>(nullable: true),
                    ChildDayOfBirth = table.Column<string>(nullable: true),
                    ChildCivilRegistPlace = table.Column<string>(nullable: true),
                    ChildCivilRegistMunicipality = table.Column<string>(nullable: true),
                    ChildCivilRegistProvincia = table.Column<string>(nullable: true),
                    ChildFolio = table.Column<string>(nullable: true),
                    ChildTomo = table.Column<string>(nullable: true),
                    GrantorName = table.Column<string>(nullable: true),
                    Notary = table.Column<string>(nullable: true),
                    ParentAge = table.Column<int>(nullable: false),
                    MaritalStatus = table.Column<int>(nullable: false),
                    MigratoryCategory = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinorAuthorizationOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MinorAuthorizationOrders_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MinorAuthorizationOrders_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MinorAuthorizationOrders_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_AgencyId",
                table: "MinorAuthorizationOrders",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_ClientId",
                table: "MinorAuthorizationOrders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_OrderNumber",
                table: "MinorAuthorizationOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinorAuthorizationOrders_UserId",
                table: "MinorAuthorizationOrders",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinorAuthorizationOrders");
        }
    }
}
