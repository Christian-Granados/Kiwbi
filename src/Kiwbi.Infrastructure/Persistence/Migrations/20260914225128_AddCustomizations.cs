using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customizations_trade_categories_trade_category_id",
                        column: x => x.trade_category_id,
                        principalTable: "trade_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customization_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    housing_typology_id = table.Column<Guid>(type: "uuid", nullable: true),
                    housing_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customization_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customization_assignments_customizations_customization_id",
                        column: x => x.customization_id,
                        principalTable: "customizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customization_assignments_housing_typologies_housing_typolo~",
                        column: x => x.housing_typology_id,
                        principalTable: "housing_typologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customization_assignments_housing_units_housing_unit_id",
                        column: x => x.housing_unit_id,
                        principalTable: "housing_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customization_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    surcharge_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    customization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customization_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customization_options_customizations_customization_id",
                        column: x => x.customization_id,
                        principalTable: "customizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customization_assignments_customization_id_housing_typology~",
                table: "customization_assignments",
                columns: new[] { "customization_id", "housing_typology_id" },
                unique: true,
                filter: "housing_typology_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customization_assignments_customization_id_housing_unit_id",
                table: "customization_assignments",
                columns: new[] { "customization_id", "housing_unit_id" },
                unique: true,
                filter: "housing_unit_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customization_assignments_housing_typology_id",
                table: "customization_assignments",
                column: "housing_typology_id");

            migrationBuilder.CreateIndex(
                name: "IX_customization_assignments_housing_unit_id",
                table: "customization_assignments",
                column: "housing_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_customization_options_customization_id_name",
                table: "customization_options",
                columns: new[] { "customization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customizations_trade_category_id_name",
                table: "customizations",
                columns: new[] { "trade_category_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customization_assignments");

            migrationBuilder.DropTable(
                name: "customization_options");

            migrationBuilder.DropTable(
                name: "customizations");
        }
    }
}
