using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onlink.Migrations
{
    /// <inheritdoc />
    public partial class h1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Post_Employee_EmployeeId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Employer_EmployerId",
                table: "Post");

            migrationBuilder.DropTable(
                name: "LoginViewModel");

            migrationBuilder.DropTable(
                name: "RegisterViewModel");

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "Post",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Post",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EmployerId",
                table: "EmployeeJob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJob_EmployerId",
                table: "EmployeeJob",
                column: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeJob_Employer_EmployerId",
                table: "EmployeeJob",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Employee_EmployeeId",
                table: "Post",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Employer_EmployerId",
                table: "Post",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "EmployerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeJob_Employer_EmployerId",
                table: "EmployeeJob");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Employee_EmployeeId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Employer_EmployerId",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeJob_EmployerId",
                table: "EmployeeJob");

            migrationBuilder.DropColumn(
                name: "EmployerId",
                table: "EmployeeJob");

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "Post",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Post",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LoginViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RememberMe = table.Column<bool>(type: "bit", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisterViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfrimPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterViewModel", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Employee_EmployeeId",
                table: "Post",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Employer_EmployerId",
                table: "Post",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "EmployerId");
        }
    }
}
