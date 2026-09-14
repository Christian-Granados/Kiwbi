using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHousingPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "housing_promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    developer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    master_plan_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_housing_promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_housing_promotions_developer_companies_developer_company_id",
                        column: x => x.developer_company_id,
                        principalTable: "developer_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_housing_promotions_developer_company_id",
                table: "housing_promotions",
                column: "developer_company_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "housing_promotions");
        }
    }
}
