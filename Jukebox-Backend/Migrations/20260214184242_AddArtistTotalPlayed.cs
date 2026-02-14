using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jukebox_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistTotalPlayed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalPlayed",
                table: "Artists",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPlayed",
                table: "Artists");
        }
    }
}
