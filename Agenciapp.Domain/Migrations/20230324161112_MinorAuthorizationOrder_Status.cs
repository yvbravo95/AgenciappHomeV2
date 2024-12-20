﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class MinorAuthorizationOrder_Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MinorAuthorizationOrders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "MinorAuthorizationOrders");
        }
    }
}
