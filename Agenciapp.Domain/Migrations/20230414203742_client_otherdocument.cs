using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class client_otherdocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OtherDocument_ExpiryDate",
                table: "Client",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherDocument_Number",
                table: "Client",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherDocument_Type",
                table: "Client",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherDocument_ExpiryDate",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "OtherDocument_Number",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "OtherDocument_Type",
                table: "Client");
        }
    }
}
