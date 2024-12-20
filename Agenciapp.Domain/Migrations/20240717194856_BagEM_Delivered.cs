using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class BagEM_Delivered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDelivered",
                table: "BagEMs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionNotDelivered",
                table: "BagEMs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageDelivered",
                table: "BagEMs",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "BagEMs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotDelivered",
                table: "BagEMs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SignatureDelivered",
                table: "BagEMs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDelivered",
                table: "BagEMs");

            migrationBuilder.DropColumn(
                name: "DescriptionNotDelivered",
                table: "BagEMs");

            migrationBuilder.DropColumn(
                name: "ImageDelivered",
                table: "BagEMs");

            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "BagEMs");

            migrationBuilder.DropColumn(
                name: "IsNotDelivered",
                table: "BagEMs");

            migrationBuilder.DropColumn(
                name: "SignatureDelivered",
                table: "BagEMs");
        }
    }
}
