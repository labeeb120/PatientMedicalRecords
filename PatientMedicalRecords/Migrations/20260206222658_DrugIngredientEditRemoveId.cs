using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class DrugIngredientEditRemoveId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "DrugIngredients");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DrugIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 1, 1 },
                column: "Id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 2, 2 },
                column: "Id",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 3, 3 },
                column: "Id",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 4, 1 },
                column: "Id",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 5, 4 },
                column: "Id",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4955));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4959));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4752));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4758));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4761));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4763));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4766));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4150));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4159));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4162));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 301, DateTimeKind.Utc).AddTicks(4164));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 3, 19, 25, 56, 335, DateTimeKind.Utc).AddTicks(8954), "$2a$11$vF9buRY7Qwr/ucD5NodqROmXXr25KMNTO4H5bsny7XIm.IVY6zToO" });
        }
    }
}
