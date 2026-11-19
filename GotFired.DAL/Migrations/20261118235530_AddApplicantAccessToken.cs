using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GotFired.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicantAccessToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTokenExpiresAt",
                table: "DismissalCases",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AccessTokenHash",
                table: "DismissalCases",
                type: "varbinary(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessTokenHash",
                table: "DismissalCases",
                column: "AccessTokenHash",
                unique: true,
                filter: "[AccessTokenHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccessTokenHash",
                table: "DismissalCases");

            migrationBuilder.DropColumn(
                name: "AccessTokenExpiresAt",
                table: "DismissalCases");

            migrationBuilder.DropColumn(
                name: "AccessTokenHash",
                table: "DismissalCases");
        }
    }
}
