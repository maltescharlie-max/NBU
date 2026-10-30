using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NBU.Data.Migrations
{
    /// <inheritdoc />
    public partial class RegistrationReviewAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferredByCode",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentFileName",
                table: "ScholarshipApplications",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentStorageName",
                table: "ScholarshipApplications",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ReferredByCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DocumentFileName",
                table: "ScholarshipApplications");

            migrationBuilder.DropColumn(
                name: "DocumentStorageName",
                table: "ScholarshipApplications");
        }
    }
}
