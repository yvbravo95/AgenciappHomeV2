using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class incomplete_passport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<decimal>(
                name: "Order_CantLb",
                table: "ReporteLiquidacion",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Order_CantLbMedicina",
                table: "ReporteLiquidacion",
                nullable: true);*/

            migrationBuilder.CreateTable(
                name: "IncompletePassport",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    PaymentCardId = table.Column<Guid>(nullable: true),
                    PromoCode = table.Column<string>(nullable: true),
                    ZelleName = table.Column<string>(nullable: true),
                    PaymentType = table.Column<int>(nullable: false),
                    Total = table.Column<decimal>(nullable: false),
                    ServicioConsular = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    SecondName = table.Column<string>(nullable: true),
                    FirstSurname = table.Column<string>(nullable: true),
                    SecondSurname = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    DateBirth = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Father = table.Column<string>(nullable: true),
                    Mother = table.Column<string>(nullable: true),
                    Height = table.Column<decimal>(nullable: false),
                    Sex = table.Column<int>(nullable: false),
                    ColorEyes = table.Column<int>(nullable: false),
                    SkinColor = table.Column<int>(nullable: false),
                    HairColor = table.Column<int>(nullable: false),
                    ClassificationMigration = table.Column<int>(nullable: false),
                    DepartureDate = table.Column<DateTime>(nullable: true),
                    CountryOfBirth = table.Column<string>(nullable: true),
                    ProvinceBirth = table.Column<string>(nullable: true),
                    MunicipalityBirth = table.Column<string>(nullable: true),
                    JobCenterName = table.Column<string>(nullable: true),
                    Profession = table.Column<string>(nullable: true),
                    Occupation = table.Column<string>(nullable: true),
                    CategoryProfession = table.Column<int>(nullable: false),
                    SchoolLevel = table.Column<string>(nullable: true),
                    FirstNameReference = table.Column<string>(nullable: true),
                    SecondNameReference = table.Column<string>(nullable: true),
                    SurnameReference = table.Column<string>(nullable: true),
                    SecondSurnameReference = table.Column<string>(nullable: true),
                    AddressReference = table.Column<string>(nullable: true),
                    ProvinceReference = table.Column<string>(nullable: true),
                    MunicipalityReference = table.Column<string>(nullable: true),
                    FamilyRelationship = table.Column<string>(nullable: true),
                    AddressCuba1 = table.Column<string>(nullable: true),
                    ProvinceCuba1 = table.Column<string>(nullable: true),
                    CityCuba1 = table.Column<string>(nullable: true),
                    From1 = table.Column<DateTime>(nullable: true),
                    To1 = table.Column<DateTime>(nullable: true),
                    AddressCuba2 = table.Column<string>(nullable: true),
                    ProvinceCuba2 = table.Column<string>(nullable: true),
                    CityCuba2 = table.Column<string>(nullable: true),
                    From2 = table.Column<string>(nullable: true),
                    To2 = table.Column<string>(nullable: true),
                    PassportPhoto = table.Column<string>(nullable: true),
                    Signature = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncompletePassport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncompletePassport_UserClients_UserId",
                        column: x => x.UserId,
                        principalTable: "UserClients",
                        principalColumn: "UserClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            /*migrationBuilder.CreateTable(
                name: "OrdersByProvince",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(nullable: false),
                    PrincipalDistributorId = table.Column<Guid>(nullable: false),
                    Estado = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    City = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersByProvince", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "OrdersReceivedByAgency",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(nullable: false),
                    PrincipalDistributorId = table.Column<Guid>(nullable: false),
                    Estado = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    AgencyName = table.Column<string>(nullable: true),
                    AgencyTransferredName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersReceivedByAgency", x => x.OrderId);
                });*/

            migrationBuilder.CreateIndex(
                name: "IX_IncompletePassport_UserId",
                table: "IncompletePassport",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncompletePassport");

            /*migrationBuilder.DropTable(
                name: "OrdersByProvince");

            migrationBuilder.DropTable(
                name: "OrdersReceivedByAgency");

            migrationBuilder.DropColumn(
                name: "Order_CantLb",
                table: "ReporteLiquidacion");

            migrationBuilder.DropColumn(
                name: "Order_CantLbMedicina",
                table: "ReporteLiquidacion");*/
        }
    }
}
