using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Team3.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUserLearningDirectionForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_LearningDirections_LearningDirectionId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Skills_LearningDirectionId",
                table: "AspNetUsers",
                column: "LearningDirectionId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Skills_LearningDirectionId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_LearningDirections_LearningDirectionId",
                table: "AspNetUsers",
                column: "LearningDirectionId",
                principalTable: "LearningDirections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
