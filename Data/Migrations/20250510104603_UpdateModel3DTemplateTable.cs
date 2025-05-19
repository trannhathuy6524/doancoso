using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel3DTemplateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarModel3Ds_Model3DTemplates_Model3DTemplateId",
                table: "CarModel3Ds");

            migrationBuilder.AlterColumn<int>(
                name: "Model3DTemplateId",
                table: "CarModel3Ds",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CarModel3Ds_Model3DTemplates_Model3DTemplateId",
                table: "CarModel3Ds",
                column: "Model3DTemplateId",
                principalTable: "Model3DTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarModel3Ds_Model3DTemplates_Model3DTemplateId",
                table: "CarModel3Ds");

            migrationBuilder.AlterColumn<int>(
                name: "Model3DTemplateId",
                table: "CarModel3Ds",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CarModel3Ds_Model3DTemplates_Model3DTemplateId",
                table: "CarModel3Ds",
                column: "Model3DTemplateId",
                principalTable: "Model3DTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
