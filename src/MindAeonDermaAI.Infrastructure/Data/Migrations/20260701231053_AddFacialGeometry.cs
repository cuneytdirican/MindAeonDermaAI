using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindAeonDermaAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacialGeometry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacialGeometries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    SymmetryScore = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    JawlineAngle = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ThirdsBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FifthsBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Yaw = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ImageQualityPercent = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EyesOpen = table.Column<bool>(type: "bit", nullable: false),
                    FacePresent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacialGeometries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacialGeometries_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacialGeometries_AssessmentId",
                table: "FacialGeometries",
                column: "AssessmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacialGeometries");
        }
    }
}
