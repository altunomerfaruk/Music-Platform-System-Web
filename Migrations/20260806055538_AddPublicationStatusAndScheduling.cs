using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicProject.Migrations
{
    public partial class AddPublicationStatusAndScheduling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublicationStatus",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAtUtc",
                table: "Songs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPublishAtUtc",
                table: "Songs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicationStatus",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAtUtc",
                table: "Albums",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPublishAtUtc",
                table: "Albums",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Songs
                SET PublicationStatus = 3,
                    PublishedAtUtc = GETUTCDATE();
                """);

            migrationBuilder.Sql("""
                UPDATE Albums
                SET PublicationStatus = 3,
                    PublishedAtUtc = GETUTCDATE();
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationStatus",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "ScheduledPublishAtUtc",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "PublicationStatus",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ScheduledPublishAtUtc",
                table: "Albums");
        }
    }
}