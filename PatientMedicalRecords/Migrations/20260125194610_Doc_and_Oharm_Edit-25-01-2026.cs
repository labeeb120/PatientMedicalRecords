using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class Doc_and_Oharm_Edit25012026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedPharmacistId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PinnedPatients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinnedPatients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PinnedPatients_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PinnedPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
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
                name: "IX_Prescriptions_AssignedPharmacistId",
                table: "Prescriptions",
                column: "AssignedPharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_PinnedPatients_DoctorId",
                table: "PinnedPatients",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PinnedPatients_PatientId",
                table: "PinnedPatients",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Pharmacists_AssignedPharmacistId",
                table: "Prescriptions",
                column: "AssignedPharmacistId",
                principalTable: "Pharmacists",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Pharmacists_AssignedPharmacistId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "PinnedPatients");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_AssignedPharmacistId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "AssignedPharmacistId",
                table: "Prescriptions");

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5552));

            migrationBuilder.UpdateData(
                table: "DrugInteractions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5556));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5316));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5322));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5326));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5329));

            migrationBuilder.UpdateData(
                table: "Drugs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(5331));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(4853));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(4862));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(4865));

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 5, 19, 2, 11, 874, DateTimeKind.Utc).AddTicks(4867));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 5, 19, 2, 12, 935, DateTimeKind.Utc).AddTicks(2502), "$2a$11$sXzYIzQJ17vaGGGMp5AUdupb3tlDnWe9JNQ/Y/8WXMvERbd/EBa2y" });
        }
    }
}
