using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Paquete_Delivered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDelivered",
                table: "Paquete",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionNotDelivered",
                table: "Paquete",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageDelivered",
                table: "Paquete",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "Paquete",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotDelivered",
                table: "Paquete",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SignatureDelivered",
                table: "Paquete",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDelivered",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "DescriptionNotDelivered",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "ImageDelivered",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "IsNotDelivered",
                table: "Paquete");

            migrationBuilder.DropColumn(
                name: "SignatureDelivered",
                table: "Paquete");
        }
    }
}
