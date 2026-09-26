using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Team3.Backend.Migrations
{
    /// <inheritdoc />
    public partial class HardenPointsConcurrencyAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_UserId",
                table: "PointsTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "MentoringSessionId",
                table: "PointsTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedUserId",
                table: "PointsTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "PointsTransactions",
                type: "text",
                nullable: false,
                defaultValue: "Legacy");

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 50,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "PointsPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<string>(type: "text", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsPurchases_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_MentoringSessionId",
                table: "PointsTransactions",
                column: "MentoringSessionId",
                unique: true,
                filter: "\"MentoringSessionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_RelatedUserId",
                table: "PointsTransactions",
                column: "RelatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_UserId_TransactionType",
                table: "PointsTransactions",
                columns: new[] { "UserId", "TransactionType" },
                unique: true,
                filter: "\"TransactionType\" = 'InitialBalance'");

            migrationBuilder.CreateIndex(
                name: "IX_PointsPurchases_UserId_IdempotencyKey",
                table: "PointsPurchases",
                columns: new[] { "UserId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PointsTransactions_AspNetUsers_RelatedUserId",
                table: "PointsTransactions",
                column: "RelatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointsTransactions_AspNetUsers_RelatedUserId",
                table: "PointsTransactions");

            migrationBuilder.DropTable(
                name: "PointsPurchases");

            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_MentoringSessionId",
                table: "PointsTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_RelatedUserId",
                table: "PointsTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_UserId_TransactionType",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "MentoringSessionId",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "RelatedUserId",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "PointsTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 50);

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_UserId",
                table: "PointsTransactions",
                column: "UserId");
        }
    }
}
