using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace firstASP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotificationRead",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusUpdatedAt",
                table: "Reservations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotificationRead",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAt",
                table: "Reservations");
        }
    }
}
