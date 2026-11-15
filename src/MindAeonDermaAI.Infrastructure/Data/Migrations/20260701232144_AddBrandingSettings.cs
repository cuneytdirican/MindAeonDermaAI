using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindAeonDermaAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrandingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LogoSvg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandingSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandingSettings");
        }
    }
}
