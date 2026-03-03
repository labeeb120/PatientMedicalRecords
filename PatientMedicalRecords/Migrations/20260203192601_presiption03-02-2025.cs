using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class presiption03022025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientInteractions");

            migrationBuilder.DropTable(
                name: "MedicationIngredients");

            migrationBuilder.DropTable(
                name: "Medications");
            migrationBuilder.DropTable(
                name: "UserRoleAssignments");

            migrationBuilder.CreateTable(
                name: "UserRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "UserRoleAssignments",
                columns: new[] { "Id", "Role", "UserId" },
                values: new object[] { 1, 4, 101 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 3, 19, 25, 56, 335, DateTimeKind.Utc).AddTicks(8954), "$2a$11$vF9buRY7Qwr/ucD5NodqROmXXr25KMNTO4H5bsny7XIm.IVY6zToO" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId",
                table: "UserRoleAssignments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoleAssignments");

            migrationBuilder.CreateTable(
                name: "IngredientInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IngredientAId = table.Column<int>(type: "int", nullable: false),
                    IngredientBId = table.Column<int>(type: "int", nullable: false),
                    InteractsWithIngredientId = table.Column<int>(type: "int", nullable: false),
                    PrimaryIngredientId = table.Column<int>(type: "int", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientInteractions_Ingredients_Id",
                        column: x => x.Id,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicationIngredients",
                columns: table => new
                {
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationIngredients", x => new { x.MedicationId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_MedicationIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationIngredients_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9591));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9599));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9287));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9298));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9302));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9307));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(9310));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(8165));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(8180));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(8186));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 19, 46, 1, 671, DateTimeKind.Utc).AddTicks(8188));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 25, 19, 46, 3, 121, DateTimeKind.Utc).AddTicks(8333), "$2a$11$qbEQXVbsEmN2wHMX8SJWReA9HMxCGH7Ohw41fyH9xvJFezPQlxTG." });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationIngredients_IngredientId",
                table: "MedicationIngredients",
                column: "IngredientId");
        }
    }
}
