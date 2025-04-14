using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onlink.Migrations
{
    /// <inheritdoc />
    public partial class m3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_Resume_ResumeId",
                table: "Certificate");

            migrationBuilder.DropIndex(
                name: "IX_Certificate_ResumeId",
                table: "Certificate");

            migrationBuilder.DropColumn(
                name: "ResumeId",
                table: "Certificate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResumeId",
                table: "Certificate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_ResumeId",
                table: "Certificate",
                column: "ResumeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Resume_ResumeId",
                table: "Certificate",
                column: "ResumeId",
                principalTable: "Resume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
