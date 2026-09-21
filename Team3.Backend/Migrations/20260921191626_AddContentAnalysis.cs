using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Team3.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddContentAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentVersion",
                table: "EducationalContents",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ContentAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationalContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentVersion = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<string>(type: "text", nullable: false),
                    AnalysisState = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClassificationStatus = table.Column<string>(type: "text", nullable: true),
                    TopicReasonCode = table.Column<string>(type: "text", nullable: true),
                    PrimaryTopicsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SecondaryTopicsJson = table.Column<string>(type: "jsonb", nullable: false),
                    TopicCount = table.Column<int>(type: "integer", nullable: false),
                    DifficultyLevel = table.Column<string>(type: "text", nullable: true),
                    DifficultyConfidence = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SafetyStatus = table.Column<string>(type: "text", nullable: true),
                    SafetyConfidence = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SafetyThreshold = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    SafetyReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RiskCategoriesJson = table.Column<string>(type: "jsonb", nullable: false),
                    RecommendationSignal = table.Column<string>(type: "text", nullable: true),
                    NeedsReview = table.Column<bool>(type: "boolean", nullable: false),
                    TopicModel = table.Column<string>(type: "text", nullable: true),
                    DifficultyModel = table.Column<string>(type: "text", nullable: true),
                    SafetyModel = table.Column<string>(type: "text", nullable: true),
                    PreprocessingVersion = table.Column<string>(type: "text", nullable: true),
                    FailureCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentAnalyses_EducationalContents_EducationalContentId",
                        column: x => x.EducationalContentId,
                        principalTable: "EducationalContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentAnalyses_EducationalContentId_ContentVersion",
                table: "ContentAnalyses",
                columns: new[] { "EducationalContentId", "ContentVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentAnalyses_RequestId",
                table: "ContentAnalyses",
                column: "RequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentAnalyses");

            migrationBuilder.DropColumn(
                name: "ContentVersion",
                table: "EducationalContents");
        }
    }
}
