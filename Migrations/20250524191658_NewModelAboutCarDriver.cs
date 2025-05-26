using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class NewModelAboutCarDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CarRideAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RideId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarRideAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarRideAssignments_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarRideAssignments_Cars_RideId",
                        column: x => x.RideId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableTo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAvailabilities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarRideAssignments_DriverId",
                table: "CarRideAssignments",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_CarRideAssignments_RideId",
                table: "CarRideAssignments",
                column: "RideId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarRideAssignments");

            migrationBuilder.DropTable(
                name: "ScheduleAvailabilities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cars");
        }
    }
}
