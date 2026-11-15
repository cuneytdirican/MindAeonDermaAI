using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindAeonDermaAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedMultimodalFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AgingVelocity",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BarrierResilienceIndex",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CounterfactualNote",
                table: "ScoreResults",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CounterfactualScore",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CrossModalConcordance",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExposomeBurden",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HyaluronanAxisIndex",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubclinical",
                table: "ScoreResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RemodelingBalanceIndex",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseLikelihoodJson",
                table: "ScoreResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubclinicalNote",
                table: "ScoreResults",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AgeAdjustPerDecade",
                table: "ProteinBiomarkers",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CohortRefMean",
                table: "ProteinBiomarkers",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CohortRefN",
                table: "ProteinBiomarkers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CohortRefSd",
                table: "ProteinBiomarkers",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssayBatch",
                table: "Assessments",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExposomeHumidity",
                table: "Assessments",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExposomePm25",
                table: "Assessments",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExposomeUvIndex",
                table: "Assessments",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacialZones",
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
                    table.PrimaryKey("PK_FacialZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentTypes",
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
                    table.PrimaryKey("PK_InstrumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZoneScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    FacialZoneId = table.Column<int>(type: "int", nullable: false),
                    Overall = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Hydration = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Wrinkle = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RednessHealth = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Pigmentation = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZoneScores_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZoneScores_FacialZones_FacialZoneId",
                        column: x => x.FacialZoneId,
                        principalTable: "FacialZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentalReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    InstrumentTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FacialZoneId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentalReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentalReadings_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrumentalReadings_FacialZones_FacialZoneId",
                        column: x => x.FacialZoneId,
                        principalTable: "FacialZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstrumentalReadings_InstrumentTypes_InstrumentTypeId",
                        column: x => x.InstrumentTypeId,
                        principalTable: "InstrumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentalReadings_AssessmentId",
                table: "InstrumentalReadings",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentalReadings_FacialZoneId",
                table: "InstrumentalReadings",
                column: "FacialZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentalReadings_InstrumentTypeId",
                table: "InstrumentalReadings",
                column: "InstrumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ZoneScores_AssessmentId",
                table: "ZoneScores",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZoneScores_FacialZoneId",
                table: "ZoneScores",
                column: "FacialZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrumentalReadings");

            migrationBuilder.DropTable(
                name: "ZoneScores");

            migrationBuilder.DropTable(
                name: "InstrumentTypes");

            migrationBuilder.DropTable(
                name: "FacialZones");

            migrationBuilder.DropColumn(
                name: "AgingVelocity",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "BarrierResilienceIndex",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "CounterfactualNote",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "CounterfactualScore",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "CrossModalConcordance",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "ExposomeBurden",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "HyaluronanAxisIndex",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "IsSubclinical",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "RemodelingBalanceIndex",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "ResponseLikelihoodJson",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "SubclinicalNote",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "AgeAdjustPerDecade",
                table: "ProteinBiomarkers");

            migrationBuilder.DropColumn(
                name: "CohortRefMean",
                table: "ProteinBiomarkers");

            migrationBuilder.DropColumn(
                name: "CohortRefN",
                table: "ProteinBiomarkers");

            migrationBuilder.DropColumn(
                name: "CohortRefSd",
                table: "ProteinBiomarkers");

            migrationBuilder.DropColumn(
                name: "AssayBatch",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "ExposomeHumidity",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "ExposomePm25",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "ExposomeUvIndex",
                table: "Assessments");
        }
    }
}
