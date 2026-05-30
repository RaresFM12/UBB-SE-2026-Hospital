using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransplantMatches_ReceiverId",
                table: "TransplantMatches",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_TransplantMatches_TransplantId",
                table: "TransplantMatches",
                column: "TransplantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferLogs_VisitId",
                table: "TransferLogs",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferLogs_ERVisits_VisitId",
                table: "TransferLogs",
                column: "VisitId",
                principalTable: "ERVisits",
                principalColumn: "VisitId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransplantMatches_Patients_ReceiverId",
                table: "TransplantMatches",
                column: "ReceiverId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransplantMatches_Transplants_TransplantId",
                table: "TransplantMatches",
                column: "TransplantId",
                principalTable: "Transplants",
                principalColumn: "TransplantId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferLogs_ERVisits_VisitId",
                table: "TransferLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TransplantMatches_Patients_ReceiverId",
                table: "TransplantMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_TransplantMatches_Transplants_TransplantId",
                table: "TransplantMatches");

            migrationBuilder.DropIndex(
                name: "IX_TransplantMatches_ReceiverId",
                table: "TransplantMatches");

            migrationBuilder.DropIndex(
                name: "IX_TransplantMatches_TransplantId",
                table: "TransplantMatches");

            migrationBuilder.DropIndex(
                name: "IX_TransferLogs_VisitId",
                table: "TransferLogs");
        }
    }
}
