using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugIngredient_Drug_DrugId1",
                table: "DrugIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugIngredient_Ingredients_IngredientId",
                table: "DrugIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientInteractions_Ingredients_IngredientAId",
                table: "IngredientInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientInteractions_Ingredients_IngredientBId",
                table: "IngredientInteractions");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_PrescriptionItems_Drug_DrugId",
            //    table: "PrescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_IngredientInteractions_IngredientAId_IngredientBId",
                table: "IngredientInteractions");

            migrationBuilder.DropIndex(
                name: "IX_IngredientInteractions_IngredientBId",
                table: "IngredientInteractions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DrugIngredient",
                table: "DrugIngredient");

            migrationBuilder.DropIndex(
                name: "IX_DrugIngredient_DrugId1",
                table: "DrugIngredient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drug",
                table: "Drug");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DropColumn(
                name: "DrugId1",
                table: "DrugIngredient");

            migrationBuilder.RenameTable(
                name: "DrugIngredient",
                newName: "DrugIngredients");

            migrationBuilder.RenameTable(
                name: "Drug",
                newName: "Drugs");

            migrationBuilder.RenameIndex(
                name: "IX_DrugIngredient_IngredientId",
                table: "DrugIngredients",
                newName: "IX_DrugIngredients_IngredientId");

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

            

            migrationBuilder.AddColumn<int>(
                name: "InteractsWithIngredientId",
                table: "IngredientInteractions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrimaryIngredientId",
                table: "IngredientInteractions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrugIngredients",
                table: "DrugIngredients",
                columns: new[] { "DrugId", "IngredientId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drugs",
                table: "Drugs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrugIngredients_Drugs_DrugId",
                table: "DrugIngredients",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugIngredients_Ingredients_IngredientId",
                table: "DrugIngredients",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientInteractions_Ingredients_Id",
                table: "IngredientInteractions",
                column: "Id",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Drugs_DrugId",
                table: "PrescriptionItems",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugIngredients_Drugs_DrugId",
                table: "DrugIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugIngredients_Ingredients_IngredientId",
                table: "DrugIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientInteractions_Ingredients_Id",
                table: "IngredientInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrescriptionItems_Drugs_DrugId",
                table: "PrescriptionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drugs",
                table: "Drugs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DrugIngredients",
                table: "DrugIngredients");

            migrationBuilder.DropColumn(
                name: "ChemicalFormula",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "InteractsWithIngredientId",
                table: "IngredientInteractions");

            migrationBuilder.DropColumn(
                name: "PrimaryIngredientId",
                table: "IngredientInteractions");

            migrationBuilder.RenameTable(
                name: "Drugs",
                newName: "Drug");

            migrationBuilder.RenameTable(
                name: "DrugIngredients",
                newName: "DrugIngredient");

            migrationBuilder.RenameIndex(
                name: "IX_DrugIngredients_IngredientId",
                table: "DrugIngredient",
                newName: "IX_DrugIngredient_IngredientId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "IngredientInteractions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "DrugId",
                table: "DrugIngredient",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "DrugId1",
                table: "DrugIngredient",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drug",
                table: "Drug",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrugIngredient",
                table: "DrugIngredient",
                column: "DrugId");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "LastLoginAt", "NationalId", "PasswordHash", "PhoneNumber", "Role", "Status", "UpdatedAt" },
                values: new object[] { 101, new DateTime(2025, 11, 29, 21, 36, 37, 985, DateTimeKind.Utc).AddTicks(9361), "admin@medicalrecords.com", "System Administrator", null, "1000000001", "$2a$11$C3Uy.Hh5PPWBfkC69W9dVeTgoha19v3JBbsCgJSjwq4l38Vz2WUp6", null, 4, 2, null });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientInteractions_IngredientAId_IngredientBId",
                table: "IngredientInteractions",
                columns: new[] { "IngredientAId", "IngredientBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngredientInteractions_IngredientBId",
                table: "IngredientInteractions",
                column: "IngredientBId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugIngredient_DrugId1",
                table: "DrugIngredient",
                column: "DrugId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DrugIngredient_Drug_DrugId1",
                table: "DrugIngredient",
                column: "DrugId1",
                principalTable: "Drug",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugIngredient_Ingredients_IngredientId",
                table: "DrugIngredient",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientInteractions_Ingredients_IngredientAId",
                table: "IngredientInteractions",
                column: "IngredientAId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientInteractions_Ingredients_IngredientBId",
                table: "IngredientInteractions",
                column: "IngredientBId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionItems_Drug_DrugId",
                table: "PrescriptionItems",
                column: "DrugId",
                principalTable: "Drug",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
