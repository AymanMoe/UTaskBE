using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UTask.Migrations
{
    public partial class ClientPFP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Clients");
        }
    }
}
