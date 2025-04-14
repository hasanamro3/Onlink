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
            migrationBuilder.CreateTable(
                name: "EmployeeJob",
                columns: table => new
                {
                    EmployeeJobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    JobApplicationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeJob", x => x.EmployeeJobId);
                    table.ForeignKey(
                        name: "FK_EmployeeJob_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeJob_JobApplication_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplication",
                        principalColumn: "JobApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJob_EmployeeId",
                table: "EmployeeJob",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJob_JobApplicationId",
                table: "EmployeeJob",
                column: "JobApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeJob");
        }
    }
}
