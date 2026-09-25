using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizationOptionThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "thumbnail_image_path",
                table: "customization_options",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thumbnail_image_path",
                table: "customization_options");
        }
    }
}
