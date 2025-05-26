using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSelfDrivenToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelfDriven",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelfDriven",
                table: "Cars");
        }
    }
}
