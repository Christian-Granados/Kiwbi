using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trade_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    housing_promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    selection_cut_off_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trade_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trade_categories_housing_promotions_housing_promotion_id",
                        column: x => x.housing_promotion_id,
                        principalTable: "housing_promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trade_categories_housing_promotion_id_name",
                table: "trade_categories",
                columns: new[] { "housing_promotion_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trade_categories");
        }
    }
}
