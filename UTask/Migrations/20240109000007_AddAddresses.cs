using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UTask.Migrations
{
    public partial class AddAddresses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Providers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreetAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Apartment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Building = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Floor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Intercom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Providers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
