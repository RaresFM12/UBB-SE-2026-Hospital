using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class NavigationOnlyRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staff_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_ERRequests_Staff_AssignedDoctorId",
                table: "ERRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemSubstances_Substances_SubstanceId",
                table: "ItemSubstances");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEvaluations_Staff_EvaluatorId",
                table: "MedicalEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Items_ItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_ClientId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyHandovers_Staff_PharmacistId",
                table: "PharmacyHandovers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftSwapRequests_Shifts_ShiftId",
                table: "ShiftSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDiscounts_Items_ItemId",
                table: "UserDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Items_ItemId",
                table: "UserNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "StaffId",
                table: "MedicalRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DoctorStaffID",
                table: "MedicalEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorStaffID",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalEvaluations_DoctorStaffID",
                table: "MedicalEvaluations",
                column: "DoctorStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorStaffID",
                table: "Appointments",
                column: "DoctorStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staff_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staff_DoctorStaffID",
                table: "Appointments",
                column: "DoctorStaffID",
                principalTable: "Staff",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_ERRequests_Staff_AssignedDoctorId",
                table: "ERRequests",
                column: "AssignedDoctorId",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSubstances_Substances_SubstanceId",
                table: "ItemSubstances",
                column: "SubstanceId",
                principalTable: "Substances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEvaluations_Staff_DoctorStaffID",
                table: "MedicalEvaluations",
                column: "DoctorStaffID",
                principalTable: "Staff",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEvaluations_Staff_EvaluatorId",
                table: "MedicalEvaluations",
                column: "EvaluatorId",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                table: "MedicalRecords",
                column: "MedicalHistoryId",
                principalTable: "MedicalHistories",
                principalColumn: "MedicalHistoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Items_ItemId",
                table: "OrderItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyHandovers_Staff_PharmacistId",
                table: "PharmacyHandovers",
                column: "PharmacistId",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftSwapRequests_Shifts_ShiftId",
                table: "ShiftSwapRequests",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDiscounts_Items_ItemId",
                table: "UserDiscounts",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Items_ItemId",
                table: "UserNotifications",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staff_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staff_DoctorStaffID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_ERRequests_Staff_AssignedDoctorId",
                table: "ERRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemSubstances_Substances_SubstanceId",
                table: "ItemSubstances");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEvaluations_Staff_DoctorStaffID",
                table: "MedicalEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEvaluations_Staff_EvaluatorId",
                table: "MedicalEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Items_ItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_ClientId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyHandovers_Staff_PharmacistId",
                table: "PharmacyHandovers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftSwapRequests_Shifts_ShiftId",
                table: "ShiftSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDiscounts_Items_ItemId",
                table: "UserDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Items_ItemId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_MedicalEvaluations_DoctorStaffID",
                table: "MedicalEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorStaffID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DoctorStaffID",
                table: "MedicalEvaluations");

            migrationBuilder.DropColumn(
                name: "DoctorStaffID",
                table: "Appointments");

            migrationBuilder.AlterColumn<int>(
                name: "StaffId",
                table: "MedicalRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staff_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Staff",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_ERRequests_Staff_AssignedDoctorId",
                table: "ERRequests",
                column: "AssignedDoctorId",
                principalTable: "Staff",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSubstances_Substances_SubstanceId",
                table: "ItemSubstances",
                column: "SubstanceId",
                principalTable: "Substances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEvaluations_Staff_EvaluatorId",
                table: "MedicalEvaluations",
                column: "EvaluatorId",
                principalTable: "Staff",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Patients_PatientId",
                table: "MedicalHistories",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                table: "MedicalRecords",
                column: "MedicalHistoryId",
                principalTable: "MedicalHistories",
                principalColumn: "MedicalHistoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Items_ItemId",
                table: "OrderItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyHandovers_Staff_PharmacistId",
                table: "PharmacyHandovers",
                column: "PharmacistId",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftSwapRequests_Shifts_ShiftId",
                table: "ShiftSwapRequests",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDiscounts_Items_ItemId",
                table: "UserDiscounts",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Items_ItemId",
                table: "UserNotifications",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
