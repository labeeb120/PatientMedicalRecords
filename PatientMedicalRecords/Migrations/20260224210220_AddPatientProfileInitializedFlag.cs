using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientProfileInitializedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfileInitialized",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(2137));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(2143));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1729));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1738));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1742));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1746));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1750));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1289));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1304));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1308));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 24, 21, 2, 12, 500, DateTimeKind.Utc).AddTicks(1312));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 24, 21, 2, 13, 968, DateTimeKind.Utc).AddTicks(3868), "$2a$11$WuLjr49JUpCXIF5whvI9nOAFYm5ufX/tLHBa9yJy4yH9ryEhgkDZe" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProfileInitialized",
                table: "Patients");

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8768));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8772));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8615));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8619));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8621));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8623));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8625));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8182));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8187));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8189));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 6, 22, 26, 54, 986, DateTimeKind.Utc).AddTicks(8191));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 6, 22, 26, 55, 568, DateTimeKind.Utc).AddTicks(1187), "$2a$11$spvNwL5SAxZzEezJDaCwp.vmleE/iIMNoZgVzLwMIWlb/qxzrdma2" });
        }
    }
}
