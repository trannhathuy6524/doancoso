using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceAndDetailedAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetailedAddress",
                table: "Cars",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "Cars",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_ProvinceId",
                table: "Cars",
                column: "ProvinceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_Provinces_ProvinceId",
                table: "Cars",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_Provinces_ProvinceId",
                table: "Cars");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_Cars_ProvinceId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "DetailedAddress",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "Cars");
        }
    }
}
