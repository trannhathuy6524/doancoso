using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotoCarRental.Migrations
{
    /// <inheritdoc />
    public partial class update2ndApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IdentityCardIssueDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCardIssuePlace",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityCardNumber",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityCardIssueDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdentityCardIssuePlace",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdentityCardNumber",
                table: "AspNetUsers");
        }
    }
}
