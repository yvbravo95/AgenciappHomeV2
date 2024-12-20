using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class incomplete_passport_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "AddressCuba1",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "AddressCuba2",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "AddressReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "CategoryProfession",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "City",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "CityCuba1",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "CityCuba2",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ClassificationMigration",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ColorEyes",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "CountryOfBirth",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "DateBirth",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "FamilyRelationship",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Father",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "FirstNameReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "FirstSurname",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "From1",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "From2",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "HairColor",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "JobCenterName",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Mother",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "MunicipalityBirth",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "MunicipalityReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "PassportPhoto",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "PaymentCardId",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "PromoCode",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ProvinceBirth",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ProvinceCuba1",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ProvinceCuba2",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ProvinceReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SchoolLevel",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SecondName",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SecondNameReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SecondSurname",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SecondSurnameReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SkinColor",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "State",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "SurnameReference",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "To1",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "To2",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "IncompletePassport");

            migrationBuilder.DropColumn(
                name: "ZelleName",
                table: "IncompletePassport");

            migrationBuilder.RenameColumn(
                name: "Zip",
                table: "IncompletePassport",
                newName: "Data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "IncompletePassport",
                newName: "Zip");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCuba1",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCuba2",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryProfession",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityCuba1",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityCuba2",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassificationMigration",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColorEyes",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfBirth",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateBirth",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyRelationship",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Father",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstNameReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstSurname",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "From1",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "From2",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HairColor",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "JobCenterName",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mother",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityBirth",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportPhoto",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentCardId",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromoCode",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceBirth",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceCuba1",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceCuba2",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolLevel",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondName",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondNameReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondSurname",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondSurnameReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkinColor",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurnameReference",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "To1",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "To2",
                table: "IncompletePassport",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "IncompletePassport",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ZelleName",
                table: "IncompletePassport",
                nullable: true);
        }
    }
}
