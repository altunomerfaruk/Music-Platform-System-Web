using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicProject.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowedArtistTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowedArtists",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowedArtists", x => new { x.UserId, x.ArtistId });
                    table.ForeignKey(
                        name: "FK_FollowedArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FollowedArtists_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowedArtists_ArtistId",
                table: "FollowedArtists",
                column: "ArtistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowedArtists");
        }
    }
}
