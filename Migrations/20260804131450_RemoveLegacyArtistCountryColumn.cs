using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicProject.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyArtistCountryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Artists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Artists",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
