using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModel3D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Model3DTemplateId",
                table: "CarModel3Ds",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Model3DTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Model3DTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Model3DTemplates_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Model3DTemplates_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarModel3Ds_Model3DTemplateId",
                table: "CarModel3Ds",
                column: "Model3DTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Model3DTemplates_BrandId",
                table: "Model3DTemplates",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Model3DTemplates_CategoryId",
                table: "Model3DTemplates",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarModel3Ds_Model3DTemplates_Model3DTemplateId",
                table: "CarModel3Ds",
                column: "Model3DTemplateId",
                principalTable: "Model3DTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarModel3Ds_Model3DTemplates_Model3DTemplateId",
                table: "CarModel3Ds");

            migrationBuilder.DropTable(
                name: "Model3DTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CarModel3Ds_Model3DTemplateId",
                table: "CarModel3Ds");

            migrationBuilder.DropColumn(
                name: "Model3DTemplateId",
                table: "CarModel3Ds");
        }
    }
}
