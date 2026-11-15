using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindAeonDermaAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeConfidenceBenchmark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BarrierAge",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenchmarkGroupLabel",
                table: "ScoreResults",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BenchmarkPercentile",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BiologicalSkinAge",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CollagenAge",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FaceAge",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImageConfidence",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverallConfidence",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProteinAge",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProteinConfidence",
                table: "ScoreResults",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarrierAge",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "BenchmarkGroupLabel",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "BenchmarkPercentile",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "BiologicalSkinAge",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "CollagenAge",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "FaceAge",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "ImageConfidence",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "OverallConfidence",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "ProteinAge",
                table: "ScoreResults");

            migrationBuilder.DropColumn(
                name: "ProteinConfidence",
                table: "ScoreResults");
        }
    }
}
