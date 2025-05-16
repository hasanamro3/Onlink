using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onlink.Migrations
{
    /// <inheritdoc />
    public partial class h2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Job_Employer_EmployerId",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_Job_JobId",
                table: "JobApplication");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Job",
                table: "Job");

            migrationBuilder.RenameTable(
                name: "Job",
                newName: "Jobs");

            migrationBuilder.RenameIndex(
                name: "IX_Job_EmployerId",
                table: "Jobs",
                newName: "IX_Jobs_EmployerId");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Resume",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_Jobs_JobId",
                table: "JobApplication",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Employer_EmployerId",
                table: "Jobs",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "EmployerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplication_Jobs_JobId",
                table: "JobApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Employer_EmployerId",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs");

            migrationBuilder.RenameTable(
                name: "Jobs",
                newName: "Job");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_EmployerId",
                table: "Job",
                newName: "IX_Job_EmployerId");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Resume",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Job",
                table: "Job",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Employer_EmployerId",
                table: "Job",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplication_Job_JobId",
                table: "JobApplication",
                column: "JobId",
                principalTable: "Job",
                principalColumn: "JobId");
        }
    }
}
