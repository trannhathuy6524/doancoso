using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class ThemTuyChonDichVuTaiXe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanceFee",
                table: "Rentals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedDistance",
                table: "Rentals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLatitude",
                table: "Rentals",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLongitude",
                table: "Rentals",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Trip",
                table: "Rentals",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceFee",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "EstimatedDistance",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PickupAddress",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PickupLatitude",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PickupLongitude",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Trip",
                table: "Rentals");
        }
    }
}
