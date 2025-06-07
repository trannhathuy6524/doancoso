using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class ThemDiemTraKhachChoRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DropoffLatitude",
                table: "Rentals",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DropoffLongitude",
                table: "Rentals",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropoffAddress",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DropoffLatitude",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DropoffLongitude",
                table: "Rentals");
        }
    }
}
