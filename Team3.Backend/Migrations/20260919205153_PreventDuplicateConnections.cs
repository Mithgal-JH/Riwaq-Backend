using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Team3.Backend.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Connections_UserAId",
                table: "Connections");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserAId_UserBId",
                table: "Connections",
                columns: new[] { "UserAId", "UserBId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Connections_UserAId_UserBId",
                table: "Connections");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserAId",
                table: "Connections",
                column: "UserAId");
        }
    }
}
