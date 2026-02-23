using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientMedicalRecords.Migrations
{
    public partial class AddPatientProfileInitializedFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfileInitialized",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProfileInitialized",
                table: "Patients");
        }
    }
}
