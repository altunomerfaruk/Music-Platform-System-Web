using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicProject.Migrations
{
    /// <inheritdoc />
    public partial class MakeSongStatSongIdPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongStats",
                table: "SongStats");

            migrationBuilder.DropIndex(
                name: "IX_SongStats_SongId",
                table: "SongStats");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SongStats");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SongStats");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SongStats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongStats",
                table: "SongStats",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongStats",
                table: "SongStats");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SongStats",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SongStats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SongStats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongStats",
                table: "SongStats",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SongStats_SongId",
                table: "SongStats",
                column: "SongId",
                unique: true);
        }
    }
}
