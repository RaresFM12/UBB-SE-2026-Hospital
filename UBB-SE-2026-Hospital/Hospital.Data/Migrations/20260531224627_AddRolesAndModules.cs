using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleModulePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModulePermissions", x => new { x.RoleId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_RoleModulePermissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleModulePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Description", "Key", "Name" },
                values: new object[,]
                {
                    { 1, "Reporting and statistics dashboards", "statistics", "Statistics" },
                    { 2, "Pharmacy catalogue and inventory", "pharmacy", "Pharmacy" },
                    { 3, "Register and manage patients", "patient-registration", "Patient Registration" },
                    { 4, "ER patient queue", "queue", "Queue" },
                    { 5, "Triage assessment", "triage", "Triage" },
                    { 6, "Assign patients to ER rooms", "room-assignment", "Room Assignment" },
                    { 7, "Patient examinations", "examination", "Examination" },
                    { 8, "Patient transfer records", "transfer-log", "Transfer Log" },
                    { 9, "Manage ER rooms", "room-management", "Room Management" },
                    { 10, "User administration", "users", "Users" },
                    { 11, "Doctor appointments", "appointments", "Appointments" },
                    { 12, "Pharmacy orders and basket", "orders", "Orders" },
                    { 13, "Medical prescriptions", "prescriptions", "Prescriptions" },
                    { 14, "Staff shifts and swaps", "shifts", "Shifts" },
                    { 15, "Staff social hangouts", "hangouts", "Hangouts" },
                    { 16, "Billing and invoicing", "billing", "Billing" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Full system administrator", "Admin" },
                    { 2, "Attending physician", "Doctor" },
                    { 3, "Pharmacy staff", "Pharmacist" },
                    { 4, "Nursing staff", "Nurse" },
                    { 5, "Pharmacy customer", "Client" },
                    { 6, "Registered patient", "Patient" },
                    { 7, "Emergency room physician", "ERDoctor" },
                    { 8, "Laboratory technician", "LabTechnician" }
                });

            migrationBuilder.InsertData(
                table: "RoleModulePermissions",
                columns: new[] { "ModuleId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 13, 1 },
                    { 14, 1 },
                    { 15, 1 },
                    { 16, 1 },
                    { 1, 2 },
                    { 3, 2 },
                    { 4, 2 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 8, 2 },
                    { 11, 2 },
                    { 13, 2 },
                    { 2, 3 },
                    { 12, 3 },
                    { 13, 3 },
                    { 16, 3 },
                    { 3, 4 },
                    { 4, 4 },
                    { 5, 4 },
                    { 6, 4 },
                    { 7, 4 },
                    { 8, 4 },
                    { 2, 5 },
                    { 12, 5 },
                    { 11, 6 },
                    { 3, 7 },
                    { 4, 7 },
                    { 5, 7 },
                    { 6, 7 },
                    { 7, 7 },
                    { 8, 7 },
                    { 9, 7 },
                    { 7, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Key",
                table: "Modules",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleModulePermissions_ModuleId",
                table: "RoleModulePermissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleModulePermissions");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
