using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UTask.Migrations
{
    public partial class DeleteCategoryInBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionMappings_AspNetUsers_UserId",
                table: "ConnectionMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConnectionMappings",
                table: "ConnectionMappings");

            migrationBuilder.DropIndex(
                name: "IX_ConnectionMappings_UserId",
                table: "ConnectionMappings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ConnectionMappings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionId",
                table: "ConnectionMappings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ConnectionMappings",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConnectionMappings",
                table: "ConnectionMappings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionMappings_UserId",
                table: "ConnectionMappings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionMappings_AspNetUsers_UserId",
                table: "ConnectionMappings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionMappings_AspNetUsers_UserId",
                table: "ConnectionMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConnectionMappings",
                table: "ConnectionMappings");

            migrationBuilder.DropIndex(
                name: "IX_ConnectionMappings_UserId",
                table: "ConnectionMappings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ConnectionMappings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ConnectionMappings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionId",
                table: "ConnectionMappings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConnectionMappings",
                table: "ConnectionMappings",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionMappings_UserId",
                table: "ConnectionMappings",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Categories_CategoryId",
                table: "Bookings",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionMappings_AspNetUsers_UserId",
                table: "ConnectionMappings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
