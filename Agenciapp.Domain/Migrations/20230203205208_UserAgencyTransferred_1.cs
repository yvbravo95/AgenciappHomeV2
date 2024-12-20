using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class UserAgencyTransferred_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAgencyTransferred_Agency_AgencyId",
                table: "UserAgencyTransferred");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAgencyTransferred_User_UserId",
                table: "UserAgencyTransferred");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAgencyTransferred",
                table: "UserAgencyTransferred");

            migrationBuilder.RenameTable(
                name: "UserAgencyTransferred",
                newName: "UserAgencyTransferreds");

            migrationBuilder.RenameIndex(
                name: "IX_UserAgencyTransferred_UserId",
                table: "UserAgencyTransferreds",
                newName: "IX_UserAgencyTransferreds_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserAgencyTransferreds",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAgencyTransferreds",
                table: "UserAgencyTransferreds",
                columns: new[] { "AgencyId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAgencyTransferreds_UserId1",
                table: "UserAgencyTransferreds",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAgencyTransferreds_Agency_AgencyId",
                table: "UserAgencyTransferreds",
                column: "AgencyId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAgencyTransferreds_User_UserId",
                table: "UserAgencyTransferreds",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAgencyTransferreds_User_UserId1",
                table: "UserAgencyTransferreds",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAgencyTransferreds_Agency_AgencyId",
                table: "UserAgencyTransferreds");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAgencyTransferreds_User_UserId",
                table: "UserAgencyTransferreds");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAgencyTransferreds_User_UserId1",
                table: "UserAgencyTransferreds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAgencyTransferreds",
                table: "UserAgencyTransferreds");

            migrationBuilder.DropIndex(
                name: "IX_UserAgencyTransferreds_UserId1",
                table: "UserAgencyTransferreds");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserAgencyTransferreds");

            migrationBuilder.RenameTable(
                name: "UserAgencyTransferreds",
                newName: "UserAgencyTransferred");

            migrationBuilder.RenameIndex(
                name: "IX_UserAgencyTransferreds_UserId",
                table: "UserAgencyTransferred",
                newName: "IX_UserAgencyTransferred_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAgencyTransferred",
                table: "UserAgencyTransferred",
                columns: new[] { "AgencyId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAgencyTransferred_Agency_AgencyId",
                table: "UserAgencyTransferred",
                column: "AgencyId",
                principalTable: "Agency",
                principalColumn: "AgencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAgencyTransferred_User_UserId",
                table: "UserAgencyTransferred",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
