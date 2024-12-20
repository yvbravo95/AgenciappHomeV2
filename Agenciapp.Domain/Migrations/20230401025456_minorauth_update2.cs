using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class minorauth_update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChildAddressNumber",
                table: "MinorAuthorizationOrders",
                newName: "CountryOfBirth");

            migrationBuilder.RenameColumn(
                name: "ChildAddressBetweenStreet2",
                table: "MinorAuthorizationOrders",
                newName: "ChildPassportNumber");

            migrationBuilder.RenameColumn(
                name: "ChildAddressBetweenStreet1",
                table: "MinorAuthorizationOrders",
                newName: "ChildCountryOfBirth");

            migrationBuilder.RenameColumn(
                name: "AddressNumber",
                table: "MinorAuthorizationOrders",
                newName: "ChildAddressProvince");

            migrationBuilder.RenameColumn(
                name: "AddressBetweenStreet2",
                table: "MinorAuthorizationOrders",
                newName: "ChildAddressCountry");

            migrationBuilder.RenameColumn(
                name: "AddressBetweenStreet1",
                table: "MinorAuthorizationOrders",
                newName: "AddressCountry");

            migrationBuilder.AddColumn<int>(
                name: "MigratoryStatus",
                table: "MinorAuthorizationOrders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MigratoryStatus",
                table: "MinorAuthorizationOrders");

            migrationBuilder.RenameColumn(
                name: "CountryOfBirth",
                table: "MinorAuthorizationOrders",
                newName: "ChildAddressNumber");

            migrationBuilder.RenameColumn(
                name: "ChildPassportNumber",
                table: "MinorAuthorizationOrders",
                newName: "ChildAddressBetweenStreet2");

            migrationBuilder.RenameColumn(
                name: "ChildCountryOfBirth",
                table: "MinorAuthorizationOrders",
                newName: "ChildAddressBetweenStreet1");

            migrationBuilder.RenameColumn(
                name: "ChildAddressProvince",
                table: "MinorAuthorizationOrders",
                newName: "AddressNumber");

            migrationBuilder.RenameColumn(
                name: "ChildAddressCountry",
                table: "MinorAuthorizationOrders",
                newName: "AddressBetweenStreet2");

            migrationBuilder.RenameColumn(
                name: "AddressCountry",
                table: "MinorAuthorizationOrders",
                newName: "AddressBetweenStreet1");
        }
    }
}
