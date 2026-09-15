using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiwbi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHousingUnitBuyers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "home_buyer_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    housing_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_buyer_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_home_buyer_assignments_AspNetUsers_buyer_user_id",
                        column: x => x.buyer_user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_home_buyer_assignments_housing_units_housing_unit_id",
                        column: x => x.housing_unit_id,
                        principalTable: "housing_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_home_buyer_assignments_buyer_user_id",
                table: "home_buyer_assignments",
                column: "buyer_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_home_buyer_assignments_housing_unit_id_buyer_user_id",
                table: "home_buyer_assignments",
                columns: new[] { "housing_unit_id", "buyer_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "home_buyer_assignments");
        }
    }
}
