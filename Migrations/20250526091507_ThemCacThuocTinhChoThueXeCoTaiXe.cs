using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class ThemCacThuocTinhChoThueXeCoTaiXe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DriverFee",
                table: "Rentals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverPhone",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Service",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverFeePerDay",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverFeePerHour",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "OfferDriverService",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverFee",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DriverPhone",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Service",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DriverFeePerDay",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "DriverFeePerHour",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "OfferDriverService",
                table: "Cars");
        }
    }
}
