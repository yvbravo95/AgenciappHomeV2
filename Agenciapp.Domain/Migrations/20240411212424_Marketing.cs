using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class Marketing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Marketings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    StripeCustomerId = table.Column<string>(nullable: true),
                    NumberFrom = table.Column<string>(nullable: true),
                    AccountSid = table.Column<string>(nullable: true),
                    AuthToken = table.Column<string>(nullable: true),
                    PriceMms = table.Column<decimal>(nullable: false),
                    PriceSms = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marketings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marketings_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "AgencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketingReceiptCampaings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MarketingId = table.Column<Guid>(nullable: false),
                    TotalSend = table.Column<int>(nullable: false),
                    SuccessSend = table.Column<int>(nullable: false),
                    FailSend = table.Column<int>(nullable: false),
                    IsMms = table.Column<bool>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    PaymentReference = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingReceiptCampaings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingReceiptCampaings_Marketings_MarketingId",
                        column: x => x.MarketingId,
                        principalTable: "Marketings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingReceiptCampaings_MarketingId",
                table: "MarketingReceiptCampaings",
                column: "MarketingId");

            migrationBuilder.CreateIndex(
                name: "IX_Marketings_AgencyId",
                table: "Marketings",
                column: "AgencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketingReceiptCampaings");

            migrationBuilder.DropTable(
                name: "Marketings");
        }
    }
}
