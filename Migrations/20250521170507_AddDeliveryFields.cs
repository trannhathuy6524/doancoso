using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Rentals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLatitude",
                table: "Rentals",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLongitude",
                table: "Rentals",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeliveryRequested",
                table: "Rentals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DeliveryLatitude",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DeliveryLongitude",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "IsDeliveryRequested",
                table: "Rentals");
        }
    }
}
