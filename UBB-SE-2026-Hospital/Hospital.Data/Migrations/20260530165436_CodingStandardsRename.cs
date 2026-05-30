using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class CodingStandardsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staff_DoctorStaffID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEvaluations_Staff_DoctorStaffID",
                table: "MedicalEvaluations");

            migrationBuilder.RenameColumn(
                name: "StaffID",
                table: "Staff",
                newName: "StaffId");

            migrationBuilder.RenameColumn(
                name: "MedName",
                table: "PrescriptionItems",
                newName: "MedicationName");

            migrationBuilder.RenameColumn(
                name: "PhoneNo",
                table: "Patients",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "DoctorStaffID",
                table: "MedicalEvaluations",
                newName: "DoctorStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalEvaluations_DoctorStaffID",
                table: "MedicalEvaluations",
                newName: "IX_MedicalEvaluations_DoctorStaffId");

            migrationBuilder.RenameColumn(
                name: "DoctorStaffID",
                table: "Appointments",
                newName: "DoctorStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorStaffID",
                table: "Appointments",
                newName: "IX_Appointments_DoctorStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staff_DoctorStaffId",
                table: "Appointments",
                column: "DoctorStaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEvaluations_Staff_DoctorStaffId",
                table: "MedicalEvaluations",
                column: "DoctorStaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staff_DoctorStaffId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEvaluations_Staff_DoctorStaffId",
                table: "MedicalEvaluations");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Staff",
                newName: "StaffID");

            migrationBuilder.RenameColumn(
                name: "MedicationName",
                table: "PrescriptionItems",
                newName: "MedName");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Patients",
                newName: "PhoneNo");

            migrationBuilder.RenameColumn(
                name: "DoctorStaffId",
                table: "MedicalEvaluations",
                newName: "DoctorStaffID");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalEvaluations_DoctorStaffId",
                table: "MedicalEvaluations",
                newName: "IX_MedicalEvaluations_DoctorStaffID");

            migrationBuilder.RenameColumn(
                name: "DoctorStaffId",
                table: "Appointments",
                newName: "DoctorStaffID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorStaffId",
                table: "Appointments",
                newName: "IX_Appointments_DoctorStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staff_DoctorStaffID",
                table: "Appointments",
                column: "DoctorStaffID",
                principalTable: "Staff",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEvaluations_Staff_DoctorStaffID",
                table: "MedicalEvaluations",
                column: "DoctorStaffID",
                principalTable: "Staff",
                principalColumn: "StaffID");
        }
    }
}
