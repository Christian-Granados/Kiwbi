using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHousingTypologiesAndUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "housing_typologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    housing_promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_housing_typologies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_housing_typologies_housing_promotions_housing_promotion_id",
                        column: x => x.housing_promotion_id,
                        principalTable: "housing_promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "housing_units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    housing_promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    housing_typology_id = table.Column<Guid>(type: "uuid", nullable: true),
                    floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    door = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    built_area_sqm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    usable_area_sqm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    floor_plan_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_housing_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_housing_units_housing_promotions_housing_promotion_id",
                        column: x => x.housing_promotion_id,
                        principalTable: "housing_promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_housing_units_housing_typologies_housing_typology_id",
                        column: x => x.housing_typology_id,
                        principalTable: "housing_typologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_housing_typologies_housing_promotion_id_name",
                table: "housing_typologies",
                columns: new[] { "housing_promotion_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_housing_units_housing_promotion_id_floor_door",
                table: "housing_units",
                columns: new[] { "housing_promotion_id", "floor", "door" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_housing_units_housing_typology_id",
                table: "housing_units",
                column: "housing_typology_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "housing_units");

            migrationBuilder.DropTable(
                name: "housing_typologies");
        }
    }
}
