using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace firstASP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BenildeEmail",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentIdNumber",
                table: "Reservations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenildeEmail",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "StudentIdNumber",
                table: "Reservations");
        }
    }
}
