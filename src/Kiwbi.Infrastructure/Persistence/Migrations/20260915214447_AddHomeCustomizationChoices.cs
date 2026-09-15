using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeCustomizationChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "home_customization_choices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    housing_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_option_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    selected_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_customization_choices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_home_customization_choices_customizations_customization_id",
                        column: x => x.customization_id,
                        principalTable: "customizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_home_customization_choices_housing_units_housing_unit_id",
                        column: x => x.housing_unit_id,
                        principalTable: "housing_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_home_customization_choices_customization_id",
                table: "home_customization_choices",
                column: "customization_id");

            migrationBuilder.CreateIndex(
                name: "IX_home_customization_choices_housing_unit_id_customization_id",
                table: "home_customization_choices",
                columns: new[] { "housing_unit_id", "customization_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "home_customization_choices");
        }
    }
}
