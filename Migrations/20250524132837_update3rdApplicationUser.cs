using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class update3rdApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityCardImageBase64",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityCardImageBase64",
                table: "AspNetUsers");
        }
    }
}
