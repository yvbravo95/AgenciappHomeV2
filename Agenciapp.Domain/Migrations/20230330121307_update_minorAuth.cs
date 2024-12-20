using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class update_minorAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildAddressProvince",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildAge",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildCivilRegistMunicipality",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildCivilRegistPlace",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildCivilRegistProvincia",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildDayOfBirth",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildFolio",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ChildTomo",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "Grandfather",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "Grandmother",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "GrantorName",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "IndentityNumber",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "MigratoryCategory",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "ParentAge",
                table: "MinorAuthorizationOrders");

            migrationBuilder.DropColumn(
                name: "PassportExpireDate",
                table: "MinorAuthorizationOrders");

            migrationBuilder.AlterColumn<int>(
                name: "MaritalStatus",
                table: "MinorAuthorizationOrders",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MaritalStatus",
                table: "MinorAuthorizationOrders",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildAddressProvince",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildAge",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildCivilRegistMunicipality",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildCivilRegistPlace",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildCivilRegistProvincia",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildDayOfBirth",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildFolio",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildTomo",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grandfather",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grandmother",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrantorName",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndentityNumber",
                table: "MinorAuthorizationOrders",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MigratoryCategory",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentAge",
                table: "MinorAuthorizationOrders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PassportExpireDate",
                table: "MinorAuthorizationOrders",
                nullable: true);
        }
    }
}
