using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class Pharm_Edit1001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "ChemicalFormula",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "DrugClass",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "GenericName",
                table: "Drugs");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Ingredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Ingredients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "ScientificName",
                table: "Drugs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "Drugs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ChemicalName",
                table: "Drugs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BrandName",
                table: "Drugs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Drugs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Drugs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DrugIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DrugInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientAId = table.Column<int>(type: "int", nullable: false),
                    IngredientBId = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugInteractions_Ingredients_IngredientAId",
                        column: x => x.IngredientAId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrugInteractions_Ingredients_IngredientBId",
                        column: x => x.IngredientBId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Drugs",
                columns: new[] { "Id", "BrandName", "ChemicalName", "CreatedAt", "Manufacturer", "NormalizedName", "ScientificName" },
                values: new object[,]
                {
                    { 1, "Adol", null, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2258), "Pharma Co.", "adol", "Paracetamol" },
                    { 2, "Brufen", null, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2265), "Med Co.", "brufen", "Ibuprofen" },
                    { 3, "Amoxil", null, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2267), "Global Drugs", "amoxil", "Amoxicillin" },
                    { 4, "Tylenol", null, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2271), "US Pharma", "tylenol", "Paracetamol" },
                    { 5, "Coumadin", null, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2273), "Chem Lab", "coumadin", "Warfarin" }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CreatedAt", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(1665), "Paracetamol", "paracetamol" },
                    { 2, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(1681), "Ibuprofen", "ibuprofen" },
                    { 3, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(1683), "Amoxicillin", "amoxicillin" },
                    { 4, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(1685), "Warfarin", "warfarin" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 5, 22, 3, 56, 183, DateTimeKind.Utc).AddTicks(7571), "$2a$11$5LwR/aqHQq5lPQMVyfFHS.wCkDVMGuo0cyMO9s4D/8aAQTuCjpaGq" });

            migrationBuilder.InsertData(
                table: "DrugIngredients",
                columns: new[] { "DrugId", "IngredientId", "Id" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 1, 4 },
                    { 5, 4, 5 }
                });

            migrationBuilder.InsertData(
                table: "DrugInteractions",
                columns: new[] { "Id", "CreatedAt", "Description", "IngredientAId", "IngredientBId", "Recommendation", "Severity" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2595), "قد يزيد الإيبوبروفين من تأثير الوارفارين، مما يزيد بشكل كبير من خطر النزيف الحاد.", 2, 4, "تجنب الاستخدام المشترك. يجب استخدام مسكن بديل (مثل باراسيتامول) ومراقبة INR بانتظام.", "Major" },
                    { 2, new DateTime(2025, 12, 5, 22, 3, 55, 932, DateTimeKind.Utc).AddTicks(2601), "لا يوجد تفاعل سريري كبير معروف بين هذين المكونين.", 1, 3, "يمكن الاستخدام المشترك بأمان.", "None/Minor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugIngredients_DrugId_IngredientId",
                table: "DrugIngredients",
                columns: new[] { "DrugId", "IngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugInteractions_IngredientAId_IngredientBId",
                table: "DrugInteractions",
                columns: new[] { "IngredientAId", "IngredientBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrugInteractions_IngredientBId",
                table: "DrugInteractions",
                column: "IngredientBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugInteractions");

            migrationBuilder.DropIndex(
                name: "IX_DrugIngredients_DrugId_IngredientId",
                table: "DrugIngredients");

            migrationBuilder.DeleteData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "DrugIngredients",
                keyColumns: new[] { "DrugId", "IngredientId" },
                keyValues: new object[] { 5, 4 });

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DrugIngredients");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ChemicalFormula",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ScientificName",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChemicalName",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BrandName",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrugClass",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GenericName",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 4, 19, 31, 1, 904, DateTimeKind.Utc).AddTicks(8873), "$2a$11$w0na2b7CZX9zP357CHLpy.6zxQR23HN4OPJIz2Q/vLoMpvGtWU8Ce" });

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
    }
}
