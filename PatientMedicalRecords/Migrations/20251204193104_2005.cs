using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class _2005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "LastLoginAt", "NationalId", "PasswordHash", "PhoneNumber", "Role", "Status", "UpdatedAt" },
                values: new object[] { 101, new DateTime(2025, 12, 4, 19, 31, 1, 904, DateTimeKind.Utc).AddTicks(8873), "admin@medicalrecords.com", "System Administrator", null, "1000000001", "$2a$11$w0na2b7CZX9zP357CHLpy.6zxQR23HN4OPJIz2Q/vLoMpvGtWU8Ce", null, 4, 2, null });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientInteractions_InteractsWithIngredientId",
                table: "IngredientInteractions",
                column: "InteractsWithIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientInteractions_PrimaryIngredientId",
                table: "IngredientInteractions",
                column: "PrimaryIngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientInteractions_Ingredients_InteractsWithIngredientId",
                table: "IngredientInteractions",
                column: "InteractsWithIngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientInteractions_Ingredients_PrimaryIngredientId",
                table: "IngredientInteractions",
                column: "PrimaryIngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IngredientInteractions_Ingredients_InteractsWithIngredientId",
                table: "IngredientInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientInteractions_Ingredients_PrimaryIngredientId",
                table: "IngredientInteractions");

            migrationBuilder.DropIndex(
                name: "IX_IngredientInteractions_InteractsWithIngredientId",
                table: "IngredientInteractions");

            migrationBuilder.DropIndex(
                name: "IX_IngredientInteractions_PrimaryIngredientId",
                table: "IngredientInteractions");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101);
        }
    }
}
