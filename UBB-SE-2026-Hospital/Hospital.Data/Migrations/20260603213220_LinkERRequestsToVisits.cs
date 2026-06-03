using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkERRequestsToVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "ERRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ERRequests_VisitId",
                table: "ERRequests",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ERRequests_ERVisits_VisitId",
                table: "ERRequests",
                column: "VisitId",
                principalTable: "ERVisits",
                principalColumn: "VisitId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ERRequests_ERVisits_VisitId",
                table: "ERRequests");

            migrationBuilder.DropIndex(
                name: "IX_ERRequests_VisitId",
                table: "ERRequests");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "ERRequests");
        }
    }
}
