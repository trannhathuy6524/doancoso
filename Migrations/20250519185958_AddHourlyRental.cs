using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Rentals",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Hours",
                table: "Rentals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Rentals",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "Cars");
        }
    }
}
