﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class FlightSearchCuba_CreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FlightCubaSearch",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FlightCubaSearch");
        }
    }
}
