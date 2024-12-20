using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Bag_CheckedNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropIndex(
                name: "IX_BagItem_ProductId",
                table: "BagItem");*/

            migrationBuilder.AddColumn<string>(
                name: "CheckedNote",
                table: "Bag",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "Bag",
                nullable: false,
                defaultValue: true);

            /*migrationBuilder.CreateIndex(
                name: "IX_BagItem_ProductId",
                table: "BagItem",
                column: "ProductId",
                unique: true);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropIndex(
                name: "IX_BagItem_ProductId",
                table: "BagItem");*/

            migrationBuilder.DropColumn(
                name: "CheckedNote",
                table: "Bag");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "Bag");

            /*migrationBuilder.CreateIndex(
                name: "IX_BagItem_ProductId",
                table: "BagItem",
                column: "ProductId");*/
        }
    }
}
