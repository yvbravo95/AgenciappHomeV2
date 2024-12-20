using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class prorroga_enissiondate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmissionDate",
                table: "Passport",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProrrogaNumber",
                table: "Passport",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmissionDate",
                table: "Passport");

            migrationBuilder.DropColumn(
                name: "ProrrogaNumber",
                table: "Passport");
        }
    }
}
