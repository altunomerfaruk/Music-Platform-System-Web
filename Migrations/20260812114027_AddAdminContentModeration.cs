using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminContentModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AdminHiddenAtUtc",
                table: "Songs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminHiddenReason",
                table: "Songs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminHidden",
                table: "Songs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdminHiddenAtUtc",
                table: "Albums",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminHiddenReason",
                table: "Albums",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminHidden",
                table: "Albums",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminHiddenAtUtc",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "AdminHiddenReason",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "IsAdminHidden",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "AdminHiddenAtUtc",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "AdminHiddenReason",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "IsAdminHidden",
                table: "Albums");
        }
    }
}
