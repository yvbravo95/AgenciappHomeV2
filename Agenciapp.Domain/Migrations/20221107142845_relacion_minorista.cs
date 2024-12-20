using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Agenciapp.Domain.Migrations
{
    public partial class relacion_minorista : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RelacionMinoristas",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AgencyId = table.Column<Guid>(nullable: false),
                    MinoristaId = table.Column<Guid>(nullable: false),
                    Tipo = table.Column<int>(nullable: false),
                    Modulo = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionMinoristas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelacionMinoristas");
        }
    }
}
