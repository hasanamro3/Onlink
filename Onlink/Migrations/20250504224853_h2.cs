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
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Employer");

            migrationBuilder.DropColumn(
                name: "PasswordConfirmation",
                table: "Employer");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmPasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConfrimPassword",
                table: "RegisterViewModel",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConfrimPassword",
                table: "RegisterViewModel");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Employer",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordConfirmation",
                table: "Employer",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
