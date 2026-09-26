using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Team3.Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), null, "Programming and Web" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), null, "Artificial Intelligence and Data" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), null, "Electronics and Embedded Systems" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), null, "Robotics" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), null, "Cybersecurity" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), null, "Design" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), null, "Mathematics" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), null, "Natural Sciences" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), null, "Health and Medicine" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), null, "Business and Economics" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), null, "Languages and Communication" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), null, "Humanities and Social Sciences" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));
        }
    }
}
