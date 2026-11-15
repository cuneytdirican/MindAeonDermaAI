using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindAeonDermaAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditWearablesGenomicsConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anonymized",
                table: "Subjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentAtUtc",
                table: "Subjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentGiven",
                table: "Subjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConsentPurpose",
                table: "Subjects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OmicsTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmicsTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WearableMetricTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WearableMetricTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WearableSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WearableSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OmicsMarkers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    OmicsTypeId = table.Column<int>(type: "int", nullable: false),
                    Marker = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Interpretation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmicsMarkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OmicsMarkers_OmicsTypes_OmicsTypeId",
                        column: x => x.OmicsTypeId,
                        principalTable: "OmicsTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OmicsMarkers_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WearableReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    WearableSourceId = table.Column<int>(type: "int", nullable: false),
                    WearableMetricTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WearableReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WearableReadings_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WearableReadings_WearableMetricTypes_WearableMetricTypeId",
                        column: x => x.WearableMetricTypeId,
                        principalTable: "WearableMetricTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WearableReadings_WearableSources_WearableSourceId",
                        column: x => x.WearableSourceId,
                        principalTable: "WearableSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OmicsMarkers_OmicsTypeId",
                table: "OmicsMarkers",
                column: "OmicsTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OmicsMarkers_SubjectId",
                table: "OmicsMarkers",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WearableReadings_SubjectId",
                table: "WearableReadings",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WearableReadings_WearableMetricTypeId",
                table: "WearableReadings",
                column: "WearableMetricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WearableReadings_WearableSourceId",
                table: "WearableReadings",
                column: "WearableSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "OmicsMarkers");

            migrationBuilder.DropTable(
                name: "WearableReadings");

            migrationBuilder.DropTable(
                name: "OmicsTypes");

            migrationBuilder.DropTable(
                name: "WearableMetricTypes");

            migrationBuilder.DropTable(
                name: "WearableSources");

            migrationBuilder.DropColumn(
                name: "Anonymized",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ConsentAtUtc",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ConsentGiven",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ConsentPurpose",
                table: "Subjects");
        }
    }
}
