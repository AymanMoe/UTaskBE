using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UTask.Migrations
{
    public partial class UpdateBookingAddAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AddressId",
                table: "Bookings",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Addresses_AddressId",
                table: "Bookings",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "AddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Addresses_AddressId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_AddressId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Bookings");
        }
    }
}
