using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UTask.Migrations
{
    public partial class UpdateBookingCategoryRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CategoryId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CategoryId",
                table: "Bookings",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CategoryId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CategoryId",
                table: "Bookings",
                column: "CategoryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
