using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class nationalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 816, DateTimeKind.Utc).AddTicks(175));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 816, DateTimeKind.Utc).AddTicks(178));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9970));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9976));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9980));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9982));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9984));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9512));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9522));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9525));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 31, 18, 56, 28, 815, DateTimeKind.Utc).AddTicks(9528));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 31, 18, 56, 29, 454, DateTimeKind.Utc).AddTicks(9593), "$2a$11$6wm.lso7ijAzEikTwSYTl.B0XASE/RPajogaXu3YudvuiOHphuHNu" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(5128));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(5132));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4853));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4858));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4864));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4867));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4869));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4353));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4362));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4365));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 21, 22, 7, 722, DateTimeKind.Utc).AddTicks(4367));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 21, 21, 22, 9, 513, DateTimeKind.Utc).AddTicks(912), "$2a$11$U5nZGJcCbM8Ig5rgH1sCA..A/FUsSo/5Aho.QoMhQsFynsP9/ZYOa" });
        }
    }
}
